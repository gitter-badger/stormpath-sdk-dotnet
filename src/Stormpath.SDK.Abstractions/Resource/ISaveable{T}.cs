﻿// <copyright file="ISaveable{T}.cs" company="Stormpath, Inc.">
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

namespace Stormpath.SDK.Resource
{
    /// <summary>
    /// Represents a resource that can be created or modified.
    /// </summary>
    /// <typeparam name="T">The <see cref="IResource">Resource</see> type.</typeparam>
    public interface ISaveable<T>
        where T : IResource
    {
        /// <summary>
        /// Creates or updates the resource.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The persisted resource data.</returns>
        /// <exception cref="Error.ResourceException">The save operation failed.</exception>
        Task<T> SaveAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
