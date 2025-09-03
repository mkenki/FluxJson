using FluxJson.Core.Configuration;

namespace FluxJson.Core.Extensions
{
    public static class JsonConfigurationExtensions
    {
        public static JsonConfiguration UseNaming(this JsonConfiguration config, NamingStrategy strategy)
        {
            config.NamingStrategy = strategy;
            return config;
        }

        public static JsonConfiguration HandleNulls(this JsonConfiguration config, NullHandling handling)
        {
            config.NullHandling = handling;
            return config;
        }

        public static JsonConfiguration WriteIndented(this JsonConfiguration config, bool indented)
        {
            config.WriteIndented = indented;
            return config;
        }

        public static JsonConfiguration WithPerformanceMode(this JsonConfiguration config, PerformanceMode mode)
        {
            config.PerformanceMode = mode;
            return config;
        }
    }
}
