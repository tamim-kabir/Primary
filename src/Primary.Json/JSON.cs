namespace Primary.Json;
/// <summary>
/// Contains shortcuts to Json serialization / deserialization methods, and default
/// JavaScript Type Method Naming Convention.
/// To User The Library in your application the use in you Program File:
/// builder.Services.Configure<JsonOptions>(options => JOptions.CreateSettings(options.JsonSerializerOptions));
/// </summary>
public static class JSON
{
    /// <summary>
    /// Deserializes a JSON string to an object
    /// </summary>
    /// <typeparam name="T">Type to deserialize</typeparam>
    /// <param name="input">JSON string</param>
    /// <param name="options">Serializer options. Defaults to IgnoreWritingNull.</param>
    /// <returns>Deserialized object</returns>
    public static T? Parse<T>(string input, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<T>(input, options ?? JOptions.IgnoreWritingNull);
    }

    /// <summary>
    /// Deserializes a JSON string to an object
    /// </summary>
    /// <param name="targetType">Type to deserialize</param>
    /// <param name="options">Serializer options. Defaults to IgnoreWritingNull.</param>
    /// <param name="input">JSON string</param>
    /// <returns>Deserialized object</returns>
    public static object? Parse(string input, Type targetType, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize(input, targetType, options ?? JOptions.IgnoreWritingNull);
    }

    /// <summary>
    /// Deserializes a JSON string to an object, ignore missing members using tolerant settings.
    /// </summary>
    /// <typeparam name="T">Type to deserialize</typeparam>
    /// <param name="input">JSON string</param>
    /// <returns>Deserialized object</returns>
    public static T? Parse<T>(string input)
    {
        return JsonSerializer.Deserialize<T>(input, JOptions.SkipMembersWriteNulls);
    }

    /// <summary>
    /// Deserializes a JSON string to an object, ignore missing members using tolerant settings.
    /// </summary>
    /// <param name="targetType">Type to deserialize</param>
    /// <param name="input">JSON string</param>
    /// <returns>Deserialized object</returns>
    public static object? Parse(string input, Type targetType)
    {
        return JsonSerializer.Deserialize(input, targetType, JOptions.SkipMembersWriteNulls);
    }

    /// <summary>
    /// Converts object to its JSON representation
    /// </summary>
    /// <param name="value">Value to convert to JSON</param>
    /// <param name="writeNulls">If true, serializes null values.</param>
    /// <returns>Serialized JSON string</returns>
    public static string Stringify(object? value, bool writeNulls = false)
    {
        return JsonSerializer.Serialize(value, writeNulls ? JOptions.IgnoreWritingNull : JOptions.IgnoreNulls);
    }

    /// <summary>
    /// Converts object to its JSON representation
    /// </summary>
    /// <param name="value">Value to convert to JSON</param>
    /// <param name="options">Serializer options.</param>
    /// <returns>Serialized JSON string</returns>
    public static string Stringify(object? value, JsonSerializerOptions options)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(options));

        return JsonSerializer.Serialize(value, options);
    }
}
