﻿// <copyright file="SAuthc1RequestAuthenticator_tests.cs" company="Stormpath, Inc.">
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
using System.Globalization;
using System.Linq;
using System.Net.Http;
using Shouldly;
using Stormpath.SDK.Api;
using Stormpath.SDK.Impl.Api;
using Stormpath.SDK.Impl.Extensions;
using Stormpath.SDK.Impl.Http.Authentication;
using Stormpath.SDK.Impl.Utility;
using Xunit;

namespace Stormpath.SDK.Tests.Impl.Authentication
{
    public class SAuthc1RequestAuthenticator_tests
    {
        private readonly SAuthc1RequestAuthenticator authenticator;
        private readonly DateTimeOffset fakeNow;
        private readonly string fakeNonce = "b3a8dfee-af7a-4dc0-b008-b2433040dfbd";

        public SAuthc1RequestAuthenticator_tests()
        {
            authenticator = new SAuthc1RequestAuthenticator();
            fakeNow = new DateTimeOffset(2015, 08, 02, 12, 30, 59, TimeSpan.Zero);
        }

        [Fact]
        public void Throws_for_empty_request_URI()
        {
            var apiKey = new ClientApiKey("foo", "bar");
            var myRequest = new HttpRequestMessage(HttpMethod.Get, string.Empty);

            Should.Throw<RequestAuthenticationException>(() =>
            {
                authenticator.AuthenticateCore(myRequest, apiKey, fakeNow, fakeNonce);
            });
        }

        [Fact]
        public void Adds_XStormpathDate_header()
        {
            var apiKey = new ClientApiKey("foo", "bar");
            var myRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.foo.bar/stuff");

            authenticator.AuthenticateCore(myRequest, apiKey, fakeNow, fakeNonce);

            // X-Stormpath-Date -> current time in UTC
            var XStormpathDateHeader = Iso8601.Parse(myRequest.Headers.GetValues("X-Stormpath-Date").Single());
            XStormpathDateHeader.ShouldBe(fakeNow);
        }

        [Fact]
        public void Adds_Host_header()
        {
            var apiKey = new ClientApiKey("foo", "bar");
            var myRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.foo.bar/stuff");

            authenticator.AuthenticateCore(myRequest, apiKey, fakeNow, fakeNonce);

            // Host: [hostname]
            myRequest.Headers.Host.ShouldBe("api.foo.bar");
        }

        [Fact]
        public void Adds_Host_header_with_nondefault_port()
        {
            var apiKey = new ClientApiKey("foo", "bar");
            var myRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.foo.bar:8088");

            authenticator.AuthenticateCore(myRequest, apiKey, fakeNow, fakeNonce);

            // Host: [hostname]
            myRequest.Headers.Host.ShouldBe("api.foo.bar:8088");
        }

        [Fact]
        public void Adds_SAuthc1_authorization_header()
        {
            IClientApiKey apiKey = new ClientApiKey("myAppId", "super-secret");
            var myRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.stormpath.com/v1/accounts");

            authenticator.AuthenticateCore(myRequest, apiKey, fakeNow, fakeNonce);

            // Authorization: "SAuthc1 [signed hash]"
            var authenticationHeader = myRequest.Headers.Authorization;
            authenticationHeader.Scheme.ShouldBe("SAuthc1");
            authenticationHeader.Parameter.ShouldNotBe(null);

            // Format "sauthc1Id=[id string], sauthc1SignedHeaders=[host;x-stormpath-date;...], sauthc1Signature=[signature in hex]"
            var parts = authenticationHeader.Parameter.Split(' ');
            var sauthc1Id = parts[0].TrimEnd(',').SplitToKeyValuePair('=');
            var sauthc1SignedHeaders = parts[1].TrimEnd(',').SplitToKeyValuePair('=');
            var sauthc1Signature = parts[2].SplitToKeyValuePair('=');
            var dateString = fakeNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

            sauthc1Id.Key.ShouldBe("sauthc1Id");
            sauthc1Id.Value.ShouldBe($"{apiKey.GetId()}/{dateString}/{fakeNonce}/sauthc1_request");

            sauthc1SignedHeaders.Key.ShouldBe("sauthc1SignedHeaders");
            sauthc1SignedHeaders.Value.ShouldBe("host;x-stormpath-date");

            sauthc1Signature.Key.ShouldBe("sauthc1Signature");
            sauthc1Signature.Value.ShouldNotBe(null);
        }

        [Fact]
        public void Should_authenticate_request_without_query_params()
        {
            IClientApiKey apiKey = new ClientApiKey("MyId", "Shush!");
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.stormpath.com/v1/");
            var now = new DateTimeOffset(2013, 7, 1, 0, 0, 0, TimeSpan.Zero);
            var nonce = "a43a9d25-ab06-421e-8605-33fd1e760825";

            authenticator.AuthenticateCore(request, apiKey, now, nonce);

            var sauthc1Id = request.Headers.Authorization.Parameter.Split(' ')[0];
            var sauthc1SignedHeaders = request.Headers.Authorization.Parameter.Split(' ')[1];
            var sauthc1Signature = request.Headers.Authorization.Parameter.Split(' ')[2];

            request.Headers.Authorization.Scheme.ShouldBe("SAuthc1");
            sauthc1Id.ShouldBe("sauthc1Id=MyId/20130701/a43a9d25-ab06-421e-8605-33fd1e760825/sauthc1_request,");
            sauthc1SignedHeaders.ShouldBe("sauthc1SignedHeaders=host;x-stormpath-date,");
            sauthc1Signature.ShouldBe("sauthc1Signature=990a95aabbcbeb53e48fb721f73b75bd3ae025a2e86ad359d08558e1bbb9411c");
        }

        [Fact]
        public void Should_authenticate_request_with_query_params()
        {
            IClientApiKey apiKey = new ClientApiKey("MyId", "Shush!");
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.stormpath.com/v1/directories?orderBy=name+asc");
            var now = new DateTimeOffset(2013, 7, 1, 0, 0, 0, TimeSpan.Zero);
            var nonce = "a43a9d25-ab06-421e-8605-33fd1e760825";

            authenticator.AuthenticateCore(request, apiKey, now, nonce);

            var sauthc1Id = request.Headers.Authorization.Parameter.Split(' ')[0];
            var sauthc1SignedHeaders = request.Headers.Authorization.Parameter.Split(' ')[1];
            var sauthc1Signature = request.Headers.Authorization.Parameter.Split(' ')[2];

            request.Headers.Authorization.Scheme.ShouldBe("SAuthc1");
            sauthc1Id.ShouldBe("sauthc1Id=MyId/20130701/a43a9d25-ab06-421e-8605-33fd1e760825/sauthc1_request,");
            sauthc1SignedHeaders.ShouldBe("sauthc1SignedHeaders=host;x-stormpath-date,");
            sauthc1Signature.ShouldBe("sauthc1Signature=fc04c5187cc017bbdf9c0bb743a52a9487ccb91c0996267988ceae3f10314176");
        }

        [Fact]
        public void Should_authenticate_request_with_multiple_query_params()
        {
            IClientApiKey apiKey = new ClientApiKey("MyId", "Shush!");
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.stormpath.com/v1/applications/77JnfFiREjdfQH0SObMfjI/groups?q=group&limit=25&offset=25");
            var now = new DateTimeOffset(2013, 7, 1, 0, 0, 0, TimeSpan.Zero);
            var nonce = "a43a9d25-ab06-421e-8605-33fd1e760825";

            authenticator.AuthenticateCore(request, apiKey, now, nonce);

            var sauthc1Id = request.Headers.Authorization.Parameter.Split(' ')[0];
            var sauthc1SignedHeaders = request.Headers.Authorization.Parameter.Split(' ')[1];
            var sauthc1Signature = request.Headers.Authorization.Parameter.Split(' ')[2];

            request.Headers.Authorization.Scheme.ShouldBe("SAuthc1");
            sauthc1Id.ShouldBe("sauthc1Id=MyId/20130701/a43a9d25-ab06-421e-8605-33fd1e760825/sauthc1_request,");
            sauthc1SignedHeaders.ShouldBe("sauthc1SignedHeaders=host;x-stormpath-date,");
            sauthc1Signature.ShouldBe("sauthc1Signature=e30a62c0d03ca6cb422e66039786865f3eb6269400941ede6226760553a832d3");
        }
    }
}