﻿// <copyright file="ToListAsync_tests.cs" company="Stormpath, Inc.">
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

using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Stormpath.SDK.Account;
using Stormpath.SDK.Tests.Common.Fakes;
using Xunit;

namespace Stormpath.SDK.Tests.Linq
{
    public class ToListAsync_tests : Linq_test<IAccount>
    {
        [Fact]
        public async Task Returns_empty_list_for_no_items()
        {
            var empty = await this.Queryable.ToListAsync();

            empty.ShouldBeEmpty();
        }

        [Fact]
        public async Task Retrieves_all_items()
        {
            this.InitializeClientWithCollection(TestAccounts.RebelAlliance);

            var alliance = await this.Queryable.ToListAsync();

            alliance.Count.ShouldBe(TestAccounts.RebelAlliance.Count);
        }

        [Fact]
        public async Task Correct_number_of_calls()
        {
            // Scenario: 51 items in a server-side collection. The default limit is 25,
            // so two calls will return 25 items, and the 3rd will return 1.
            this.InitializeClientWithCollection(Enumerable.Repeat(TestAccounts.Chewbacca, 51));

            var longList = await this.Queryable.ToListAsync();

            longList.Count.ShouldBe(51);
            this.FakeHttpClient.Calls.Count().ShouldBe(3);
        }
    }
}
