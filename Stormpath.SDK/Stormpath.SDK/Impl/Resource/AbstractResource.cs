﻿// <copyright file="AbstractResource.cs" company="Stormpath, Inc.">
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
using System.Collections;
using Stormpath.SDK.Impl.DataStore;
using Stormpath.SDK.Resource;

namespace Stormpath.SDK.Impl.Resource
{
    internal abstract class AbstractResource : IResource
    {
        private static readonly string HrefPropertyName = "href";

        private readonly object writeLock = new object();

        private readonly IDataStore dataStore;
        private readonly Hashtable properties;
        private readonly Hashtable dirtyProperties;

        private bool isDirty = false;

        public AbstractResource(IDataStore dataStore)
        {
            this.dataStore = dataStore;

            properties = new Hashtable();
            dirtyProperties = new Hashtable();
        }

        public AbstractResource(IDataStore dataStore, Hashtable properties)
        {
            this.dataStore = dataStore;

            this.properties = new Hashtable(properties);
            this.dirtyProperties = new Hashtable(properties.Count);
        }

        string IResource.Href => GetProperty<string>(HrefPropertyName);

        protected IDataStore GetDataStore() => dataStore;

        public LinkProperty GetLinkProperty(string name)
        {
            return GetProperty<LinkProperty>(name) ?? new LinkProperty();
        }

        public T GetProperty<T>(string name)
        {
            var value = GetProperty(name);

            // Todo add a converter list to handle multiple target types
            if (typeof(T) == typeof(SDK.Account.AccountStatus))
            {
                return (T)(SDK.Account.AccountStatus.Parse(value.ToString()) as object);
            }

            return (T)value;
        }

        public object GetProperty(string name)
        {
            object value;

            value = dirtyProperties[name];
            if (value != null)
                return value;

            return properties[name];
        }

        public void SetProperty<T>(string name, T value)
        {
            SetProperty(name, value);
        }

        public void SetProperty(string name, object value)
        {
            lock (writeLock)
            {
                dirtyProperties[name] = value;
                isDirty = true;
            }
        }

        public void SetProperties(Hashtable newProperties)
        {
            try
            {
                lock (writeLock)
                {
                    foreach (DictionaryEntry item in newProperties)
                    {
                        this.properties[item.Key] = item.Value;
                    }

                    isDirty = false;
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException("Could not load properties into resource item.", e);
            }
        }
    }
}