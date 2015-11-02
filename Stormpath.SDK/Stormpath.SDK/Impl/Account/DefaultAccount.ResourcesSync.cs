﻿// <copyright file="DefaultAccount.ResourcesSync.cs" company="Stormpath, Inc.">
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

using Stormpath.SDK.Directory;
using Stormpath.SDK.Impl.Provider;
using Stormpath.SDK.Provider;
using Stormpath.SDK.Tenant;

namespace Stormpath.SDK.Impl.Account
{
    internal sealed partial class DefaultAccount
    {
        IDirectory IAccountSync.GetDirectory()
            => this.GetInternalSyncDataStore().GetResource<IDirectory>(this.Directory.Href);

        ITenant IAccountSync.GetTenant()
            => this.GetTenant(this.Tenant.Href);

        IProviderData IAccountSync.GetProviderData()
            => this.GetInternalSyncDataStore().GetResource<IProviderData>(this.ProviderData.Href, ProviderTypeConverter.DataTypeLookup);
    }
}