﻿// <copyright file="DefaultDirectoryCreationOptions.cs" company="Stormpath, Inc.">
// Copyright (c) 2015 Stormpath, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using Stormpath.SDK.Directory;
using Stormpath.SDK.Impl.Resource;
using Stormpath.SDK.Provider;
using Stormpath.SDK.Resource;

namespace Stormpath.SDK.Impl.Directory
{
    internal sealed class DefaultDirectoryCreationOptions : IDirectoryCreationOptions
    {
        private readonly IProvider provider;
        private readonly Action<IRetrievalOptions<IDirectory>> responseOptionsAction;

        public DefaultDirectoryCreationOptions(IProvider provider, Action<IRetrievalOptions<IDirectory>> responseOptionsAction)
        {
            this.provider = provider;
            this.responseOptionsAction = responseOptionsAction;
        }

        IProvider IDirectoryCreationOptions.Provider
            => this.provider;

        public string GetQueryString()
        {
            if (this.responseOptionsAction == null)
                return string.Empty;

            var responseOptions = new DefaultRetrivalOptions<IDirectory>();
            this.responseOptionsAction(responseOptions);

            return responseOptions.ToString();
        }
    }
}
