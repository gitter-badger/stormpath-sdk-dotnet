﻿// <copyright file="Deserialization_tests.cs" company="Stormpath, Inc.">
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

using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;
using Stormpath.SDK.Serialization;
using Xunit;

namespace Stormpath.SDK.Extensions.Serialization.JsonNet.Tests
{
    public class Deserialization_tests
    {
        private readonly IJsonSerializer serializer;

        public Deserialization_tests()
        {
            this.serializer = new JsonNetSerializer();
        }

        [Fact]
        public void Arrays_are_deserialized_to_nested_list_of_IDictionary()
        {
            var json = @"
{
    ""items"":
    [
        {
            ""foo"": 123,
            ""bar"": ""baz""
        },
        {
            ""anotherFoo"": 456,
            ""anotherBar"": ""baz2""
        }
    ]
}";
            var result = this.serializer.Deserialize(json);

            result.Count.ShouldBe(1);
            result.Single().Key.ShouldBe("items");
            result.Single().Value.ShouldBeAssignableTo<IEnumerable<IDictionary<string, object>>>();

            var nested = result.Single().Value as IEnumerable<IDictionary<string, object>>;
            nested.Count().ShouldBe(2);

            var firstItem = nested.ElementAt(0);
            firstItem.Count.ShouldBe(2);
            firstItem.First().Key.ShouldBe("foo");
            firstItem.First().Value.ShouldBe(123);
            firstItem.Last().Key.ShouldBe("bar");
            firstItem.Last().Value.ShouldBe("baz");

            var secondItem = nested.ElementAt(1);
            secondItem.Count.ShouldBe(2);
            secondItem.First().Key.ShouldBe("anotherFoo");
            secondItem.First().Value.ShouldBe(456);
            secondItem.Last().Key.ShouldBe("anotherBar");
            secondItem.Last().Value.ShouldBe("baz2");
        }

        [Fact]
        public void Scalar_arrays_are_deserialized_to_list()
        {
            var json = @"
{
    ""urls"":
    [
        ""http://foo.bar"",
        ""http://baz.bar""
    ]
}";
            var result = this.serializer.Deserialize(json);

            result.Count.ShouldBe(1);

            var nestedList = result.Single().Value as IEnumerable<object>;
            nestedList.Count().ShouldBe(2);

            var firstItem = nestedList.ElementAt(0);
            firstItem.ShouldBe("http://foo.bar");

            var secondItem = nestedList.ElementAt(1);
            secondItem.ShouldBe("http://baz.bar");
        }

        [Fact]
        public void Objects_are_deserialized_to_IDictionary()
        {
            var json = @"{ link: { href: ""http://foobar/myprop"" } }";

            var result = this.serializer.Deserialize(json);

            result.Count.ShouldBe(1);
            result.Single().Key.ShouldBe("link");
            result.Single().Value.ShouldBeAssignableTo<IDictionary<string, object>>();

            var nested = result.Single().Value as IDictionary<string, object>;
            nested.Single().Key.ShouldBe("href");
            nested.Single().Value.ShouldBe("http://foobar/myprop");
        }

        [Fact]
        public void DateTimeOffset_is_deserialized_properly()
        {
            var result = this.serializer.Deserialize(@"{ createdAt: '2015-06-01T12:30:00Z' }");

            result.Count.ShouldBe(1);
            result.Single().Key.ShouldBe("createdAt");
            result.Single().Value.ShouldBeOfType<DateTimeOffset>();
            result.Single().Value.ShouldBe(new DateTimeOffset(2015, 06, 01, 12, 30, 00, TimeSpan.Zero));
        }

        [Fact]
        public void Int_is_deserialized_properly()
        {
            var result = this.serializer.Deserialize(@"{ items: 12 }");

            result.Count.ShouldBe(1);
            result.Single().Key.ShouldBe("items");
            result.Single().Value.ShouldBeOfType<int>();
            result.Single().Value.ShouldBe(12);
        }

        [Fact]
        public void Long_is_deserialized_properly()
        {
            var result = this.serializer.Deserialize(@"{ items: 2147483648 }");

            result.Count.ShouldBe(1);
            result.Single().Key.ShouldBe("items");
            result.Single().Value.ShouldBeOfType<long>();
            result.Single().Value.ShouldBe(2147483648);
        }

        [Fact]
        public void Bool_is_deserialized_properly()
        {
            var result = this.serializer.Deserialize(@"{ isFoo: true }");

            result.Count.ShouldBe(1);
            result.Single().Key.ShouldBe("isFoo");
            result.Single().Value.ShouldBeOfType<bool>();
            result.Single().Value.ShouldBe(true);
        }

        [Fact]
        public void Null_is_deserialized_properly()
        {
            var result = this.serializer.Deserialize(@"{ items: null }");

            result.Count.ShouldBe(1);
            result.Single().Key.ShouldBe("items");
            result.Single().Value.ShouldBeNull();
        }

        [Fact]
        public void String_is_deserialized_properly()
        {
            var result = this.serializer.Deserialize(@"{ name: 'foobar' }");

            result.Count.ShouldBe(1);
            result.Single().Key.ShouldBe("name");
            result.Single().Value.ShouldBeOfType<string>();
            result.Single().Value.ShouldBe("foobar");
        }

        [Fact]
        public void Unknown_types_are_deserialized_as_strings()
        {
            var floatResult = this.serializer.Deserialize(@"{ pi: 3.14 }");
            floatResult.Single().Value.ShouldBeOfType<string>();
            floatResult.Single().Value.ShouldBe("3.14");
        }
    }
}