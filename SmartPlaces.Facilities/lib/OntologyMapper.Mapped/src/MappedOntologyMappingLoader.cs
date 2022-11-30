﻿// -----------------------------------------------------------------------
// <copyright file="MappedOntologyMappingLoader.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.OntologyMapper.Mapped
{
    using System.Reflection;
    using System.Text.Json;
    using Microsoft.Extensions.Logging;

    public class MappedOntologyMappingLoader : IOntologyMappingLoader
    {
        private readonly ILogger logger;
        private readonly string resourcePath = string.Empty;

        public MappedOntologyMappingLoader(ILogger logger, string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                throw new ArgumentNullException(nameof(resourcePath));
            }

            this.logger = logger;
            this.resourcePath = resourcePath;
        }

        public OntologyMapping LoadOntologyMapping()
        {
            logger.LogInformation("Loading Ontology Mapping file: {fileName}", resourcePath);

            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();
            var resourceName = resources.Single(str => str.ToLowerInvariant().EndsWith(resourcePath.ToLowerInvariant()));

            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
            };

            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string result = reader.ReadToEnd();
                        OntologyMapping? mappings;
                        try
                        {
                            mappings = JsonSerializer.Deserialize<OntologyMapping>(result, options);
                        }
                        catch (JsonException jex)
                        {
                            throw new MappingFileException($"Mappings file '{resourcePath}' is malformed.", resourcePath, jex);
                        }

                        if (mappings == null)
                        {
                            throw new MappingFileException($"Mappings file '{resourcePath}' is empty.", resourcePath);
                        }

                        return mappings;
                    }
                }
                else
                {
                    throw new FileNotFoundException(resourcePath);
                }
            }
        }
    }
}
