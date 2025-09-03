using System;
using FluxJson.Core.Configuration;

namespace FluxJson.Core.Extensions
{
    public static class JsonParserExtensions
    {
        public static T To<T>(this JsonParser parser)
        {
            return parser.To<T>();
        }

        public static JsonParser UseCamelCase(this JsonParser parser)
        {
            parser.Configure(config => config.NamingStrategy = NamingStrategy.CamelCase);
            return parser;
        }

        public static JsonParser CaseInsensitive(this JsonParser parser)
        {
            parser.Configure(config => config.CaseSensitive = false);
            return parser;
        }

        public static JsonParser AllowTrailingCommas(this JsonParser parser)
        {
            parser.Configure(config => config.AllowTrailingCommas = true);
            return parser;
        }

        public static JsonParser WithPerformanceMode(this JsonParser parser, PerformanceMode mode)
        {
            parser.Configure(config => config.PerformanceMode = mode);
            return parser;
        }
    }
}
