﻿// <copyright file="IGroupCreationActionsSync.cs" company="Stormpath, Inc.">
// Copyright (c) 2015 Stormpath, Inc.
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

namespace Stormpath.SDK.Group
{
    internal interface IGroupCreationActionsSync
    {
        /// <summary>
        /// Synchronously reates a new <see cref="IGroup"/>.
        /// </summary>
        /// <param name="group">The group to create.</param>
        /// <returns>The new <see cref="IGroup"/>.</returns>
        IGroup CreateGroup(IGroup group);

        /// <summary>
        /// Synchronously creates a new <see cref="IGroup"/>.
        /// </summary>
        /// <param name="group">The group to create.</param>
        /// <param name="creationOptionsAction">
        /// An inline builder for an instance of <see cref="IGroupCreationOptions"/>, which will be used when sending the request.
        /// </param>
        /// <returns>The new <see cref="IGroup"/>.</returns>
        IGroup CreateGroup(IGroup group, Action<GroupCreationOptionsBuilder> creationOptionsAction);

        /// <summary>
        /// Synchronously reates a new <see cref="IGroup"/>.
        /// </summary>
        /// <param name="group">The group to create.</param>
        /// <param name="creationOptions">An <see cref="IGroupCreationOptions"/> instance to use when sending the request.</param>
        /// <returns>The new <see cref="IGroup"/>.</returns>
        IGroup CreateGroup(IGroup group, IGroupCreationOptions creationOptions);
    }
}
