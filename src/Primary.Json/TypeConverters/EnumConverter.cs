namespace Primary.Json.TypeConverters;
/// <summary>
/// Serializes enum values as numbers while trying to handle string values while deserializing
/// </summary>
internal sealed class EnumConverter : JsonConverterFactory
{
    /// <summary>
    /// Default instance
    /// </summary>
    public static readonly EnumConverter Instance = new();

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

    /// <inheritdoc/>
    public override JsonConverter? CreateConverter(Type type, JsonSerializerOptions options)
    {
        return (JsonConverter?)Activator.CreateInstance(
            typeof(EnumValueConverter<>).MakeGenericType([type]),
            bindingAttr: BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: [],
            culture: null);
    }

    private class EnumValueConverter<T> : JsonConverter<T> where T : struct
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            static T EnsureDefined(long value)
            {
                var val = (T)Enum.Parse(typeof(T), value.ToString());
                if (!Enum.IsDefined(typeof(T), val))
                    throw new InvalidCastException($"{value} is not a valid {typeof(T).Name} enum value!");
                return val;
            }

            long l;
            switch (reader.TokenType)
            {
                case JsonTokenType.True:
                case JsonTokenType.False:
                case JsonTokenType.Number:
                    if (reader.TokenType == JsonTokenType.Number)
                        l = reader.GetInt64();
                    else
                        l = reader.TokenType == JsonTokenType.True ? 1 : 0;
                    return EnsureDefined(l);

                case JsonTokenType.String:
                    string s = reader.GetString()!.Trim();
                    if (long.TryParse(s, out l))
                        return EnsureDefined(l);

                    return (T)Enum.Parse(typeof(T), s, true);

                default:
                    throw new JsonException($"Unexpected token when deserializing enum type {typeof(T).FullName}: " + reader.TokenType);
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(Convert.ToInt64(value));
        }
    }
}
