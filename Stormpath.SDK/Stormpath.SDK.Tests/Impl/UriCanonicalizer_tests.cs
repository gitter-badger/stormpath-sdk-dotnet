﻿// <copyright file="UriCanonicalizer_tests.cs" company="Stormpath, Inc.">
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
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Stormpath.SDK.Impl.Http.Support;
using Xunit;

namespace Stormpath.SDK.Tests.Impl
{
    public class UriCanonicalizer_tests
    {
        private readonly string fakeBaseUrl = @"http://api.foo.bar";

        [Fact]
        public void Relative_paths_are_fully_qualified()
        {
            var uc = new UriCanonicalizer(fakeBaseUrl);

            var uri = uc.Canonicalize("path/to/resource");

            uri.ToString().ShouldBe($"{fakeBaseUrl}/path/to/resource");
        }

        [Fact]
        public void Relative_paths_with_leading_slash_are_fully_qualified()
        {
            var uc = new UriCanonicalizer(fakeBaseUrl);

            var uri = uc.Canonicalize("/path/to/resource");

            uri.ToString().ShouldBe($"{fakeBaseUrl}/path/to/resource");
        }
    }
}
