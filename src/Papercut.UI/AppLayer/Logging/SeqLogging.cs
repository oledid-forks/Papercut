﻿// Papercut
// 
// Copyright © 2008 - 2012 Ken Robertson
// Copyright © 2013 - 2024 Jaben Cargman
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.


using Autofac;

using Serilog.Configuration;

namespace Papercut.AppLayer.Logging;

public class SeqLogging : ILoggerSettings
{
    public void Configure(LoggerConfiguration loggerConfiguration)
    {
        ArgumentNullException.ThrowIfNull(loggerConfiguration, nameof(loggerConfiguration));

        loggerConfiguration.WriteTo.Seq("http://localhost:5341");
    }

    #region Begin Static Container Registrations

    /// <summary>
    /// Called dynamically from the RegisterStaticMethods() call in the container module.
    /// </summary>
    /// <param name="builder"></param>
    [UsedImplicitly]
    static void Register(ContainerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        builder.RegisterType<SeqLogging>().As<ILoggerSettings>();
    }

    #endregion
}