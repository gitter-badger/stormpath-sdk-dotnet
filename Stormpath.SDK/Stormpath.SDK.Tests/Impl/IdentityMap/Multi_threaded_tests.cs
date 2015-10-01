﻿// <copyright file="Multi_threaded_tests.cs" company="Stormpath, Inc.">
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
using System.Threading.Tasks;
using Shouldly;
using Stormpath.SDK.Impl.DataStore;
using Stormpath.SDK.Impl.IdentityMap;
using Xunit;

namespace Stormpath.SDK.Tests.Impl.IdentityMap
{
    public class Multi_threaded_tests : IDisposable
    {
        private readonly IIdentityMap<string, TestEntity> identityMap;

        public Multi_threaded_tests()
        {
            // Arbitrary expiration policy. We won't be validating expirations in these tests
            // because it's tricky to do so with MemoryCache.
            this.identityMap = new MemoryCacheIdentityMap<string, TestEntity>(TimeSpan.FromSeconds(10));
        }

        private TestEntity CreateEntity(string id)
        {
            if (string.IsNullOrEmpty(id))
                id = Guid.NewGuid().ToString();

            return new TestEntity(id);
        }

        [Fact]
        public void Creating_items_in_another_thread()
        {
            var foo = this.identityMap.GetOrAdd("foo", () => this.CreateEntity("foo"));
            foo.SetCount(17);

            var tasks = new List<Task>();

            for (var i = 0; i < 5; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var itemId = Guid.NewGuid().ToString();
                    var bar = this.identityMap.GetOrAdd(itemId, () => this.CreateEntity(itemId));
                    bar.SetCount(i * 10);

                    var fooAgain = this.identityMap.GetOrAdd("foo", () => this.CreateEntity("foo"));
                    fooAgain.Count.ShouldBe(17);
                }));
            }

            Task.WhenAll(tasks).Wait();
            foo.Count.ShouldBe(17);
            this.identityMap.LifetimeItemsAdded.ShouldBe(6);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(10000)]
        [InlineData(100000)]
        public void Making_many_items(int items)
        {
            var persistentItemId = $"Making_many_items_{items}";
            var foo = this.identityMap.GetOrAdd(persistentItemId, () => this.CreateEntity(persistentItemId));
            foo.SetCount(1337);

            Parallel.For(0, items, i =>
            {
                var itemId = $"item-{i}";
                this.identityMap.GetOrAdd(itemId, () => this.CreateEntity(itemId)).SetCount(i);

                foo.Count.ShouldBe(1337);
                this.identityMap.GetOrAdd(persistentItemId, () => this.CreateEntity(persistentItemId)).Count.ShouldBe(1337);
            });

            this.identityMap.LifetimeItemsAdded.ShouldBe(items + 1);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(10000)]
        [InlineData(100000)]
        public void Making_many_duplicates(int times)
        {
            var persistentItemId = $"Making_many_duplicates_{times}";
            this.identityMap.GetOrAdd(persistentItemId, () => this.CreateEntity(persistentItemId)).SetCount(1337);

            Parallel.For(0, times, i =>
            {
                var foo = this.identityMap.GetOrAdd(persistentItemId, () => this.CreateEntity(persistentItemId));
                foo.Count.ShouldBe(1337);
            });

            this.identityMap.LifetimeItemsAdded.ShouldBe(1);
        }

        public void Dispose()
        {
            this.identityMap.Dispose();
        }
    }
}
