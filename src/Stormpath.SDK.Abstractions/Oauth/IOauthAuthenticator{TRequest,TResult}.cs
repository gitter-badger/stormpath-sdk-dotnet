﻿// <copyright file="IOauthAuthenticator{TRequest,TResult}.cs" company="Stormpath, Inc.">
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

using System.Threading;
using System.Threading.Tasks;

namespace Stormpath.SDK.Oauth
{
    /// <summary>
    /// Represents the operations that every OAuth 2.0 Authenticator type must support.
    /// </summary>
    /// <typeparam name="TRequest">The request kind that the authenticator will accept.</typeparam>
    /// <typeparam name="TResult">The response kind that the authenticator will return.</typeparam>
    public interface IOauthAuthenticator<TRequest, TResult>
    {
        /// <summary>
        /// Executes the OAuth 2.0 Authentication Request and returns the result.
        /// </summary>
        /// <param name="authenticationRequest">The Authentication Request this authenticator will attempt.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An Authentication Result representing the successful authentication.</returns>
        /// <exception cref="Error.ResourceException">The authentication failed.</exception>
        Task<TResult> AuthenticateAsync(TRequest authenticationRequest, CancellationToken cancellationToken = default(CancellationToken));
    }
}
