namespace SpanJson.Resolvers
{
    public class SpanJsonOptions
    {
        public NamingConventions NamingConvention { get; set; } // OriginalCase, CamelCase
        public NullOptions NullOption { get; set; } // ExcludeNulls, IncludeNulls
        public EnumOptions EnumOption { get; set; } // String, Integer
        public ByteArrayOptions ByteArrayOption { get; set; } // Array, Base64
        public bool AllowUnquotedStrings { get; set; } = false;
    }
}