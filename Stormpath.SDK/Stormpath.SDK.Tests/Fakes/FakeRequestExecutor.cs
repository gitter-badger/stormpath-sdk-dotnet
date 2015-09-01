﻿// <copyright file="FakeRequestExecutor.cs" company="Stormpath, Inc.">
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

using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Stormpath.SDK.Impl.Http;
using Stormpath.SDK.Impl.Http.Support;

namespace Stormpath.SDK.Tests.Fakes
{
    public class FakeRequestExecutor<T>
    {
        private readonly IRequestExecutor fakeRequestExecutor;
        private readonly string resourceJson;

        public FakeRequestExecutor(string resourceJson)
        {
            this.resourceJson = resourceJson;
            this.fakeRequestExecutor = Substitute.For<IRequestExecutor>();

            // All GETs return 200 OK
            this.fakeRequestExecutor
                .ExecuteAsync(
                    Arg.Is<IHttpRequest>(req => req.Method == HttpMethod.Get), Arg.Any<CancellationToken>())
                .Returns(
                    Task.FromResult(new DefaultHttpResponse(200, "OK", new HttpHeaders(), resourceJson, "application/json") as IHttpResponse));

            // All POSTs return 204 Created
            this.fakeRequestExecutor
                .ExecuteAsync(
                    Arg.Is<IHttpRequest>(req => req.Method == HttpMethod.Post), Arg.Any<CancellationToken>())
                .Returns(
                    Task.FromResult(new DefaultHttpResponse(204, "Created", new HttpHeaders(), resourceJson, "application/json") as IHttpResponse));
        }

        internal IRequestExecutor Object => this.fakeRequestExecutor;
    }
}