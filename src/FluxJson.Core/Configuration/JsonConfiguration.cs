using System.Text;

namespace FluxJson.Core.Configuration
{
    public class JsonConfiguration
    {
        public NamingStrategy NamingStrategy { get; set; } = NamingStrategy.PascalCase;
        public NullHandling NullHandling { get; set; } = NullHandling.Include;
        public DateTimeFormat DateTimeFormat { get; set; } = DateTimeFormat.ISO8601;
        public string CustomDateTimeFormat { get; set; } = "yyyy-MM-ddTHH:mm:ss.fffffffZ";
        public int MaxDepth { get; set; } = 32;
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public bool WriteIndented { get; set; } = false;
        public bool AllowTrailingCommas { get; set; } = false;
        public bool CaseSensitive { get; set; } = true;
        public PerformanceMode PerformanceMode { get; set; } = PerformanceMode.Balanced;
        public bool IgnoreReadOnlyProperties { get; set; } = false;

        public List<IJsonConverter> Converters { get; set; } = new List<IJsonConverter>();
        public Dictionary<Type, Dictionary<string, IJsonConverter>> PropertyConverters { get; set; } = new Dictionary<Type, Dictionary<string, IJsonConverter>>();
        public Dictionary<Type, IJsonConverter> TypeConverters { get; set; } = new Dictionary<Type, IJsonConverter>();

        public JsonConfiguration Clone()
        {
            return new JsonConfiguration
            {
                NamingStrategy = NamingStrategy,
                NullHandling = NullHandling,
                DateTimeFormat = DateTimeFormat,
                CustomDateTimeFormat = CustomDateTimeFormat,
                MaxDepth = MaxDepth,
                Encoding = Encoding,
                WriteIndented = WriteIndented,
                AllowTrailingCommas = AllowTrailingCommas,
                CaseSensitive = CaseSensitive,
                PerformanceMode = PerformanceMode,
                IgnoreReadOnlyProperties = IgnoreReadOnlyProperties,
                Converters = new List<IJsonConverter>(Converters)
            };
        }

        public void RegisterTypeConverter(Type type, IJsonConverter converter)
        {
            TypeConverters[type] = converter;
        }

        public void RegisterPropertyConverter(Type type, string propertyName, IJsonConverter converter)
        {
            if (!PropertyConverters.ContainsKey(type))
            {
                PropertyConverters[type] = new Dictionary<string, IJsonConverter>();
            }

            PropertyConverters[type][propertyName] = converter;
        }
    }
}
