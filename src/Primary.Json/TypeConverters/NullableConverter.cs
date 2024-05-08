namespace Primary.Json.TypeConverters;
/// <summary>
/// Tries to handle empty string for nullable values like JSON.NET does
/// </summary>
internal sealed class NullableConverter : JsonConverterFactory
{
    /// <summary>
    /// Default instance
    /// </summary>
    public static readonly NullableConverter Instance = new();

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert) => Nullable.GetUnderlyingType(typeToConvert) != null;

    /// <inheritdoc/>
    public override JsonConverter? CreateConverter(Type type, JsonSerializerOptions options)
    {
        return (JsonConverter?)Activator
            .CreateInstance(type: typeof(NullableValueConverter<>)
            .MakeGenericType([Nullable.GetUnderlyingType(type)!]),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: [],
            culture: null);
    }

    class NullableValueConverter<T> : JsonConverter<T?> where T : struct
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                if (reader.ValueTextEquals(ReadOnlySpan<byte>.Empty))
                    return null;
            }

            return JsonSerializer.Deserialize<T>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
        {
            if (value == null)
                writer.WriteNullValue();
            else
                JsonSerializer.Serialize(writer, value.Value, options);
        }
    }
}