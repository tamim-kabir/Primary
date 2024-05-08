﻿namespace Primary.Json.TypeConverters;
/// <summary>
///   Serialize/deserialize a long value converting it to string if it is higher than precision double can handle</summary>
/// <summary>
internal sealed class Int64Converter : JsonConverter<long>
{
    /// <summary>
    /// The default instance
    /// </summary>
    public static readonly Int64Converter Instance = new();

    /// <inheritdoc/>
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            if (!long.TryParse(reader.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long number))
                throw new JsonException();

            return number;
        }

        return reader.GetInt64();
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        if (value > 9007199254740992 ||
            value < -9007199254740992)
            writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
        else
            writer.WriteNumberValue(value);
    }
}
