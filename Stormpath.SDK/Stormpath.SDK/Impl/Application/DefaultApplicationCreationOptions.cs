﻿// <copyright file="DefaultApplicationCreationOptions.cs" company="Stormpath, Inc.">
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
using System.Collections.Generic;
using System.Linq;
using Stormpath.SDK.Application;
using Stormpath.SDK.Impl.Extensions;
using Stormpath.SDK.Impl.Resource;
using Stormpath.SDK.Resource;

namespace Stormpath.SDK.Impl.Application
{
    internal sealed class DefaultApplicationCreationOptions : IApplicationCreationOptions
    {
        private readonly bool createDirectory;
        private readonly string directoryName;
        private readonly Action<IRetrievalOptions<IApplication>> responseOptionsAction;

        public DefaultApplicationCreationOptions(bool createDirectory, string directoryName, Action<IRetrievalOptions<IApplication>> responseOptionsAction)
        {
            this.createDirectory = createDirectory;
            this.directoryName = directoryName;
            this.responseOptionsAction = responseOptionsAction;
        }

        bool IApplicationCreationOptions.CreateDirectory => this.createDirectory;

        string IApplicationCreationOptions.DirectoryName => this.directoryName;

        private bool IsDirectoryNameSpecified => !string.IsNullOrEmpty(this.directoryName);

        public string GetQueryString()
        {
            var arguments = new List<string>(2);

            if (this.createDirectory)
                arguments.Add("createDirectory=" + (this.IsDirectoryNameSpecified ? this.directoryName : "true"));

            if (this.responseOptionsAction != null)
            {
                var responseOptions = new DefaultRetrivalOptions<IApplication>();
                this.responseOptionsAction(responseOptions);

                arguments.Add(responseOptions.ToString());
            }

            return arguments
                .Where(x => !string.IsNullOrEmpty(x))
                .Join("&");
        }
    }
}
