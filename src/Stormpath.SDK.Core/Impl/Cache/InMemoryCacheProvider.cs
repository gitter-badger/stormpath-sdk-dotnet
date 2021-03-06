﻿// <copyright file="InMemoryCacheProvider.cs" company="Stormpath, Inc.">
// Copyright (c) 2016 Stormpath, Inc.
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
using Stormpath.SDK.Cache;

namespace Stormpath.SDK.Impl.Cache
{
    internal class InMemoryCacheProvider : AbstractCacheProvider
    {
        public InMemoryCacheProvider()
            : base(syncSupported: true, asyncSupported: true)
        {
        }

        protected override ISynchronousCache CreateSyncCache(string name, TimeSpan? ttl, TimeSpan? tti)
            => new InMemoryCache(name, ttl, tti);

        protected override IAsynchronousCache CreateAsyncCache(string name, TimeSpan? ttl, TimeSpan? tti)
            => new InMemoryCache(name, ttl, tti);
    }
}
