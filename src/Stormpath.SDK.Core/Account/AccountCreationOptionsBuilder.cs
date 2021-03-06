﻿// <copyright file="AccountCreationOptionsBuilder.cs" company="Stormpath, Inc.">
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

using Stormpath.SDK.Impl.Account;
using Stormpath.SDK.Impl.Resource;
using Stormpath.SDK.Resource;

namespace Stormpath.SDK.Account
{
    /// <summary>
    /// A builder to construct <see cref="IAccountCreationOptions"/> instances.
    /// </summary>
    public sealed class AccountCreationOptionsBuilder
    {
        /// <summary>
        /// Gets or sets whether to explicitly override the registration workflow of the Login Source for new Accounts.
        /// </summary>
        /// <value>
        /// <para>If set to <see langword="true"/>, the account registration workflow will be triggered no matter what the Login Source configuration is.</para>
        /// <para>If set to <see langword="false"/>, the account registration workflow will <b>NOT</b> be triggered, no matter what the Login Source configuration is.</para>
        /// <para>If you want to ensure the registration workflow behavior matches the Login Source default, leave this <see langword="null"/>.</para>
        /// </value>
        public bool? RegistrationWorkflowEnabled { get; set; }

        /// <summary>
        /// Gets or sets the password format, used for importing passwords.
        /// </summary>
        /// <remarks>This value should be <see langword="null"/> unless you are importing existing password hashes into Stormpath.</remarks>
        /// <value>The password format.</value>
        public PasswordFormat PasswordFormat { get; set; }

        /// <summary>
        /// Gets the response options to apply to the request.
        /// </summary>
        /// <value>The response options to apply to the request.</value>
        /// <example>
        /// <code source="CreationOptionsBuilderExamples.cs" region="RequestCustomData" lang="C#" title="Request and cache Custom Data" />
        /// </example>
        public IRetrievalOptions<IAccount> ResponseOptions { get; } = new DefaultRetrievalOptions<IAccount>();

        /// <summary>
        /// Creates a new <see cref="IAccountCreationOptions"/> instance based on the current builder state.
        /// </summary>
        /// <returns>A new <see cref="IAccountCreationOptions"/> based on the current builder state.</returns>
        public IAccountCreationOptions Build()
        {
            return new DefaultAccountCreationOptions(this.RegistrationWorkflowEnabled, this.PasswordFormat, this.ResponseOptions);
        }
    }
}
