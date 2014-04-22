﻿/*  
 * Papercut
 *
 *  Copyright © 2008 - 2012 Ken Robertson
 *  Copyright © 2013 - 2014 Jaben Cargman
 *  
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *  
 *  http://www.apache.org/licenses/LICENSE-2.0
 *  
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 *  
 */

namespace Papercut.Core.Message
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Papercut.Core.Configuration;

    public class MessageRepository : IDisposable
    {
        public const string MessageFileSearchPattern = "*.eml";

        readonly IMessagePathConfigurator _messagePathConfigurator;

        List<FileSystemWatcher> _watchers;

        public MessageRepository(IMessagePathConfigurator messagePathConfigurator)
        {
            _messagePathConfigurator = messagePathConfigurator;
            SetupMessageWatchers();
        }

        public void Dispose()
        {
            foreach (var watch in _watchers)
            {
                try
                {
                    watch.EnableRaisingEvents = false;
                    watch.Dispose();
                }
                catch (Exception)
                {
                }
            }
        }

        void SetupMessageWatchers()
        {
            _watchers = new List<FileSystemWatcher>();

            // setup watcher for each path...
            foreach (var path in _messagePathConfigurator.LoadPaths)
            {
                var watcher = new FileSystemWatcher(path, MessageFileSearchPattern)
                {
                    NotifyFilter = NotifyFilters.LastAccess
                                   | NotifyFilters.LastWrite
                                   | NotifyFilters.FileName
                };

                // Add event handlers.
                watcher.Created += OnChanged;
                watcher.Deleted += OnDeleted;
                watcher.Renamed += OnRenamed;

                // Begin watching.
                watcher.EnableRaisingEvents = true;

                _watchers.Add(watcher);
            }
        }

        void OnDeleted(object sender, FileSystemEventArgs e)
        {
            RefreshNeeded(this, new EventArgs());
        }

        void OnRenamed(object sender, RenamedEventArgs e)
        {
            RefreshNeeded(this, new EventArgs());
        }

        void OnChanged(object sender, FileSystemEventArgs e)
        {
            NewMessage(this, new NewMessageEventArgs(new MessageEntry(e.FullPath)));
        }

        public bool DeleteMessage(MessageEntry entry)
        {
            // Delete the file and remove the entry
            if (!File.Exists(entry.File))
            {
                return false;
            }

            File.Delete(entry.File);
            return true;
        }

        public byte[] GetMessage(string file)
        {
            return File.ReadAllBytes(file);
        }

        public IList<MessageEntry> LoadMessages()
        {
            var files = _messagePathConfigurator.LoadPaths.SelectMany(p => Directory.GetFiles(p, MessageFileSearchPattern));

            return files.Select(file => new MessageEntry(file))
                .OrderByDescending(m => m.ModifiedDate)
                .ThenBy(m => m.Name)
                .ToList();
        }

        public event EventHandler<NewMessageEventArgs> NewMessage;

        public event EventHandler RefreshNeeded;

        protected virtual void OnRefreshNeeded()
        {
            var handler = RefreshNeeded;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        protected virtual void OnNewMessage(NewMessageEventArgs e)
        {
            var handler = NewMessage;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public string SaveMessage(IList<string> output)
        {
            string file = null;

            try
            {
                do
                {
                    // the file must not exists.  the resolution of DataTime.Now may be slow w.r.t. the speed of the received files
                    var fileNameUnique = string.Format(
                        "{0}-{1}.eml",
                        DateTime.Now.ToString("yyyyMMddHHmmssFF"),
                        Guid.NewGuid().ToString().Substring(0, 2));

                    file = Path.Combine(_messagePathConfigurator.DefaultSavePath, fileNameUnique);
                }
                while (File.Exists(file));

                File.WriteAllLines(file, output);
            }
            catch (Exception ex)
            {
                Logger.WriteError(string.Format("Failure saving email message: {0}", file), ex);
            }

            return file;
        }
    }
}