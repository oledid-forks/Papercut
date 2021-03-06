﻿// Papercut
// 
// Copyright © 2008 - 2012 Ken Robertson
// Copyright © 2013 - 2020 Jaben Cargman
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

namespace Papercut.Helpers
{
    using System;
    using System.Threading.Tasks;

    using Autofac;

    using Caliburn.Micro;

    public class ViewModelWindowManager : WindowManager, IViewModelWindowManager
    {
        readonly ILifetimeScope _lifetimeScope;

        public ViewModelWindowManager(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public async Task<bool?> ShowDialogWithViewModel<TViewModel>(
            Action<TViewModel> setViewModel = null,
            object context = null)
            where TViewModel : PropertyChangedBase
        {
            var viewModel = _lifetimeScope.Resolve<TViewModel>();
            setViewModel?.Invoke(viewModel);
            return await ShowDialogAsync(viewModel, context);
        }

        public async Task ShowWindowWithViewModelAsync<TViewModel>(
            Action<TViewModel> setViewModel = null,
            object context = null)
            where TViewModel : PropertyChangedBase
        {
            var viewModel = _lifetimeScope.Resolve<TViewModel>();
            setViewModel?.Invoke(viewModel);
            await ShowWindowAsync(viewModel, context);
        }

        public async Task ShowPopupWithViewModel<TViewModel>(
            Action<TViewModel> setViewModel = null,
            object context = null)
            where TViewModel : PropertyChangedBase
        {
            var viewModel = _lifetimeScope.Resolve<TViewModel>();
            setViewModel?.Invoke(viewModel);
            await ShowPopupAsync(viewModel, context);
        }
    }
}