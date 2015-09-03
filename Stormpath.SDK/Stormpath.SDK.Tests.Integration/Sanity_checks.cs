﻿// <copyright file="Namespace_tests.cs" company="Stormpath, Inc.">
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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Stormpath.SDK.Tests.Integration
{
    public class Sanity_checks
    {
        [Fact]
        public void All_Impl_members_are_hidden()
        {
            var typesInNamespace = Assembly
                .GetAssembly(typeof(Stormpath.SDK.Client.IClient))
                .GetTypes()
                .Where(x =>
                    x.Namespace != null &&
                    x.Namespace.StartsWith("Stormpath.SDK.Impl", StringComparison.InvariantCultureIgnoreCase))
                .Where(x => x.IsPublic)
                .ToList();

            typesInNamespace.Count.ShouldBe(
                expected: 0,
                customMessage: $"These types are visible: {string.Join(", ", typesInNamespace)}");
        }

        [Fact]
        public void All_async_methods_have_CancellationToken_parameters()
        {
            var methodsInAssembly = Assembly
                .GetAssembly(typeof(Stormpath.SDK.Client.IClient))
                .GetTypes()
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));

            var asyncMethods = methodsInAssembly
                .Where(method =>
                    method.ReturnType == typeof(Task) ||
                    (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)));

            var asyncMethodsWithoutCancellationToken = asyncMethods
                .Where(method => !method.GetParameters().Any(p => p.ParameterType == typeof(CancellationToken)));

            asyncMethodsWithoutCancellationToken
                .Any()
                .ShouldBe(
                    expected: false,
                    customMessage: $"These methods do not have a CancellationToken parameter: {string.Join(", ", asyncMethodsWithoutCancellationToken.Select(m => m.Name))}");
        }

        [Fact]
        public void All_Impl_async_methods_have_required_CancellationToken_parameters()
        {
            var methodsInAssembly = Assembly
                .GetAssembly(typeof(Stormpath.SDK.Client.IClient))
                .GetTypes()
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));

            var asyncMethods = methodsInAssembly
                .Where(method =>
                    method.ReturnType == typeof(Task) ||
                    (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)));

            var asyncMethodsWithOptionalCT = asyncMethods
                .Where(method => method.GetParameters().Any(p => p.ParameterType == typeof(CancellationToken) && p.IsOptional))
                .Where(method => method.DeclaringType.Namespace.StartsWith("Stormpath.SDK.Impl"));

            // No optional/default values here!
            asyncMethodsWithOptionalCT
                .Any()
                .ShouldBe(
                    expected: false,
                    customMessage: $"These methods should not have an optional CancellationToken parameter: {string.Join(", ", asyncMethodsWithOptionalCT.Select(m => m.Name))}");
        }

        [Fact]
        public void All_SDK_async_methods_have_optional_CancellationToken_parameters()
        {
            var methodsInAssembly = Assembly
                .GetAssembly(typeof(Stormpath.SDK.Client.IClient))
                .GetTypes()
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));

            var asyncMethods = methodsInAssembly
                .Where(method =>
                    method.ReturnType == typeof(Task) ||
                    (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)));

            var asyncMethodsWithRequiredCT = asyncMethods
                .Where(method => method.GetParameters().Any(p => p.ParameterType == typeof(CancellationToken) && !p.IsOptional))
                .Where(method => !method.DeclaringType.Namespace.StartsWith("Stormpath.SDK.Impl"));

            // Must be all optional
            asyncMethodsWithRequiredCT
                .Any()
                .ShouldBe(
                    expected: false,
                    customMessage: $"These methods must have an optional CancellationToken parameter: {string.Join(", ", asyncMethodsWithRequiredCT.Select(m => m.Name))}");
        }
    }
}