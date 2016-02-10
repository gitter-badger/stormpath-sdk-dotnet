﻿// <copyright file="DefaultClientApiKeyBuilder.cs" company="Stormpath, Inc.">
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
using Microsoft.Extensions.PlatformAbstractions;
using Stormpath.SDK.Api;
using Stormpath.SDK.Impl.Logging;
using Stormpath.SDK.Impl.Utility;
using Stormpath.SDK.Logging;

namespace Stormpath.SDK.Impl.Api
{
    internal sealed class DefaultClientApiKeyBuilder : IClientApiKeyBuilder
    {
        // Used when reading values from a properties file
        private static readonly string DefaultFileIdPropertyName = "apiKey.id";
        private static readonly string DefaultFileSecretPropertyName = "apiKey.secret";

        // Used when retrieving values directly from environment variables or app.config
        private static readonly string DefaultDirectIdPropertyName = "STORMPATH_API_KEY_ID";
        private static readonly string DefaultDirectSecretPropertyName = "STORMPATH_API_KEY_SECRET";

        private static readonly string DefaultApiKeyFileLocation = System.IO.Path.Combine("~", ".stormpath", "apiKey.properties");

        // Wrappers for static .NET Framework calls (for easier unit testing)
        private readonly IConfigurationManager config;
        private readonly IEnvironment env;
        private readonly IFile file;

        private readonly ILogger logger;

        // Instance fields
        private string apiKeyId;
        private string apiKeySecret;
        private System.IO.Stream apiKeyFileInputStream;
        private string apiKeyFilePath;
        private string apiKeyIdPropertyName;
        private string apiKeySecretPropertyName;

        public DefaultClientApiKeyBuilder(IConfigurationManager configuration, IEnvironment environment, IFile file, ILogger logger)
        {
            this.config = configuration;
            this.env = environment;
            this.file = file;

            this.logger = logger != null
                ? logger
                : new NullLogger();
        }

        IClientApiKeyBuilder IClientApiKeyBuilder.SetId(string id)
        {
            this.apiKeyId = id;
            return this;
        }

        IClientApiKeyBuilder IClientApiKeyBuilder.SetSecret(string secret)
        {
            this.apiKeySecret = secret;
            return this;
        }

        IClientApiKeyBuilder IClientApiKeyBuilder.SetInputStream(System.IO.Stream stream)
        {
            this.apiKeyFileInputStream = stream;
            return this;
        }

        IClientApiKeyBuilder IClientApiKeyBuilder.SetFileLocation(string path)
        {
            this.apiKeyFilePath = path;
            return this;
        }

        IClientApiKeyBuilder IClientApiKeyBuilder.SetIdPropertyName(string idPropertyName)
        {
            this.apiKeyIdPropertyName = idPropertyName;
            return this;
        }

        IClientApiKeyBuilder IClientApiKeyBuilder.SetSecretPropertyName(string secretPropertyName)
        {
            this.apiKeySecretPropertyName = secretPropertyName;
            return this;
        }

        IClientApiKey IClientApiKeyBuilder.Build()
        {
            // 1. Try to load default API key properties file. Lowest priority
            var defaultProperties = this.GetDefaultApiKeyFileProperties();
            string id = defaultProperties?.GetProperty(this.apiKeyIdPropertyName ?? DefaultFileIdPropertyName);
            string secret = defaultProperties?.GetProperty(this.apiKeySecretPropertyName ?? DefaultFileSecretPropertyName);

            // 2. Try file location specified by environment variables
            var envFileLocation = this.env.GetEnvironmentVariable("STORMPATH_API_KEY_FILE");
            envFileLocation = this.ResolveHomePath(envFileLocation);
            if (!string.IsNullOrEmpty(envFileLocation))
            {
                this.logger.Trace($"Found STORMPATH_API_KEY_FILE environment variable. Value: '{envFileLocation}'");

                var envProperties = this.GetPropertiesFromEnvironmentVariableFileLocation(envFileLocation);
                id = envProperties?.GetProperty(this.apiKeyIdPropertyName ?? DefaultFileIdPropertyName, defaultValue: id);
                secret = envProperties?.GetProperty(this.apiKeySecretPropertyName ?? DefaultFileSecretPropertyName, defaultValue: secret);
            }

            // 3. Try environment variables directly
            var apiIdFromEnvironment = this.env.GetEnvironmentVariable(this.apiKeyIdPropertyName ?? DefaultDirectIdPropertyName);
            var secretFromEnvironment = this.env.GetEnvironmentVariable(this.apiKeySecretPropertyName ?? DefaultDirectSecretPropertyName);
            bool didRetrieveValuesFromEnvironment = !string.IsNullOrEmpty(apiIdFromEnvironment) && !string.IsNullOrEmpty(secretFromEnvironment);
            if (didRetrieveValuesFromEnvironment)
            {
                this.logger.Trace("Found API Key and Secret in environment variables.");

                id = apiIdFromEnvironment;
                secret = secretFromEnvironment;
            }

            // 4. Try file location specified by web.config/app.config
            var appConfigFileLocation = this.config.AppSettings?["STORMPATH_API_KEY_FILE"];
            appConfigFileLocation = this.ResolveHomePath(appConfigFileLocation);
            if (!string.IsNullOrEmpty(appConfigFileLocation))
            {
                this.logger.Trace($"Found STORMPATH_API_KEY_FILE key in .config file. Value: '{appConfigFileLocation}'");

                var appConfigProperties = this.GetPropertiesFromAppConfigFileLocation(appConfigFileLocation);
                id = appConfigProperties?.GetProperty(this.apiKeyIdPropertyName ?? DefaultFileIdPropertyName, defaultValue: id);
                secret = appConfigProperties?.GetProperty(this.apiKeySecretPropertyName ?? DefaultFileSecretPropertyName, defaultValue: secret);
            }

            // 5. Try web.config/app.config keys directly
            var apiIdFromAppConfig = this.config.AppSettings?[this.apiKeyIdPropertyName ?? DefaultDirectIdPropertyName];
            var secretFromAppConfig = this.config.AppSettings?[this.apiKeySecretPropertyName ?? DefaultDirectSecretPropertyName];
            bool didRetrieveValuesFromAppConfig = !string.IsNullOrEmpty(apiIdFromAppConfig) && !string.IsNullOrEmpty(secretFromAppConfig);
            if (didRetrieveValuesFromAppConfig)
            {
                this.logger.Trace("Found API Key and Secret in .config file.");

                id = apiIdFromAppConfig;
                secret = secretFromAppConfig;
            }

            // 6. Try configured property file
            if (!string.IsNullOrEmpty(this.apiKeyFilePath))
            {
                this.apiKeyFilePath = this.ResolveHomePath(this.apiKeyFilePath);
                this.logger.Trace($"Using specified API Key file path '{this.apiKeyFilePath}'");

                var fileProperties = this.GetPropertiesFromFile();
                id = fileProperties?.GetProperty(this.apiKeyIdPropertyName ?? DefaultFileIdPropertyName, defaultValue: id);
                secret = fileProperties?.GetProperty(this.apiKeySecretPropertyName ?? DefaultFileSecretPropertyName, defaultValue: secret);
            }

            // 7. Try an input stream that was passed to us
            if (this.apiKeyFileInputStream != null)
            {
                this.logger.Trace("Reading input stream for API Key and Secret");

                var streamProperties = this.GetPropertiesFromStream();
                id = streamProperties?.GetProperty(this.apiKeyIdPropertyName ?? DefaultFileIdPropertyName, defaultValue: id);
                secret = streamProperties?.GetProperty(this.apiKeySecretPropertyName ?? DefaultFileSecretPropertyName, defaultValue: secret);
            }

            // 8. Explicitly-configured values always take precedence
            id = this.apiKeyId ?? id;
            secret = this.apiKeySecret ?? secret;

            if (string.IsNullOrEmpty(id))
            {
                var message = "Unable to find an API Key ID, either from explicit configuration (for example, " +
                    nameof(IClientApiKeyBuilder) + ".setApiKeyId), or from a file location.\r\n" +
                    "Please provide the API Key ID by one of these methods.";
                throw new Exception(message);
            }

            if (string.IsNullOrEmpty(secret))
            {
                var message = "Unable to find an API Key Secret, either from explicit configuration (for example, " +
                    nameof(IClientApiKeyBuilder) + ".setApiKeySecret), or from a file location.\r\n" +
                    "Please provide the API Key Secret by one of these methods.";
                throw new Exception(message);
            }

            return new DefaultClientApiKey(id, secret);
        }

        private Properties GetDefaultApiKeyFileProperties()
        {
            var expandedLocation = this.ResolveHomePath(DefaultApiKeyFileLocation);

            try
            {
                var source = this.file.ReadAllText(expandedLocation);

                return new Properties(source);
            }
            catch (Exception ex)
            {
                var msg =
                    $"Unable to find or load default API Key properties file [{expandedLocation}] " +
                    "This can safely be ignored as this is a fallback location - other more specific locations will be checked.\n" +
                    $"Exception: '{ex.Message}' at '{ex.Source}'";
                this.logger.Trace(msg);

                return null;
            }
        }

        private Properties GetPropertiesFromEnvironmentVariableFileLocation(string path)
        {
            try
            {
                var source = this.file.ReadAllText(path);
                var properties = new Properties(source);
                return properties;
            }
            catch (Exception ex)
            {
                var msg =
                    $"Unable to load API Key properties file [{path}] specified by environment variable " +
                    "STORMPATH_API_KEY_FILE. This can safely be ignored as this is a fallback location - " +
                    "other more specific locations will be checked.\n" +
                    $"Exception: '{ex.Message}' at '{ex.Source}'";
                this.logger.Trace(msg);

                return null;
            }
        }

        private Properties GetPropertiesFromAppConfigFileLocation(string path)
        {
            try
            {
                var source = this.file.ReadAllText(path);
                var properties = new Properties(source);
                return properties;
            }
            catch (Exception ex)
            {
                var msg =
                    $"Unable to load API Key properties file [{path}] specified by config key " +
                    "STORMPATH_API_KEY_FILE. This can safely be ignored as this is a fallback location - " +
                    "other more specific locations will be checked.\n" +
                    $"Exception: '{ex.Message}' at '{ex.Source}'";
                this.logger.Trace(msg);

                return null;
            }
        }

        private Properties GetPropertiesFromFile()
        {
            try
            {
                var source = this.file.ReadAllText(this.apiKeyFilePath);
                var properties = new Properties(source);
                return properties;
            }
            catch (Exception ex)
            {
                var msg =
                    $"Unable to load API Key properties file [{this.apiKeyFilePath}].\n" +
                    $"Exception: '{ex.Message}' at '{ex.Source}'";
                this.logger.Trace(msg);

                return null;
            }
        }

        private Properties GetPropertiesFromStream()
        {
            if (!this.apiKeyFileInputStream.CanRead)
            {
                return null;
            }

            try
            {
                using (var reader = new System.IO.StreamReader(this.apiKeyFileInputStream))
                {
                    var source = reader.ReadToEnd();
                    return new Properties(source);
                }
            }
            catch (Exception ex)
            {
                var msg =
                    "Unable to read properties from specified input stream.\n" +
                    $"Exception: '{ex.Message}' at '{ex.Source}'";
                this.logger.Trace(msg);

                return null;
            }
        }

        private string ResolveHomePath(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            if (!input.StartsWith("~"))
            {
                return input;
            }

            var homePath = GetHome();

            return System.IO.Path.Combine(homePath, input.Replace($"~{System.IO.Path.DirectorySeparatorChar}", string.Empty));
        }


        // Copied from DNX's DnuEnvironment.cs
        //todo use IEnvironment?
        private static string GetHome()
        {
#if NET451
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
#else
            var runtimeEnv = PlatformServices.Default.Runtime;
            if (runtimeEnv.OperatingSystem == "Windows")
            {
                return Environment.GetEnvironmentVariable("USERPROFILE") ??
                    Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH");
            }
            else
            {
                var home = Environment.GetEnvironmentVariable("HOME");

                if (string.IsNullOrEmpty(home))
                {
                    throw new Exception("Home directory not found. The HOME environment variable is not set.");
                }

                return home;
            }
#endif
        }
    }
}
