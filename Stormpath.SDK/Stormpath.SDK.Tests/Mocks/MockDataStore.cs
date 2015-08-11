﻿// <copyright file="MockDataStore.cs" company="Stormpath, Inc.">
//      Copyright (c) 2015 Stormpath, Inc.
// </copyright>
// <remarks>
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </remarks>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Stormpath.SDK.Impl.DataStore;
using Stormpath.SDK.Impl.Resource;

namespace Stormpath.SDK.Tests.Mocks
{
    // TODO: Make this an actual server with valid responses
    public class MockDataStore<TType> : IDataStore
    {
        public MockDataStore(IEnumerable<TType> items)
        {
            this.Items = items.ToList();
        }

        public List<TType> Items { get; private set; }

        async Task<CollectionResponsePageDto<T>> IDataStore.GetCollectionAsync<T>(string href, CancellationToken cancellationToken)
        {
            bool typesMatch = typeof(T) == typeof(TType);
            if (!typesMatch)
                throw new ArgumentException("Requested type must match type of fake data.");

            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();

            return new CollectionResponsePageDto<T>()
            {
                Href = href,
                Items = this.Items.OfType<T>().ToList(),
                Limit = this.Items.Count, // todo
                Offset = 0, // todo
                Size = this.Items.Count // todo
            };
        }
    }
}
