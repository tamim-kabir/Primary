namespace Primary.Json;

/// <summary>
/// Contains default options for System.Text.Json serialization
/// </summary>
public static class JOptions
{
    static JOptions()
    {
        IgnoreNulls = CreateOptions(new JsonSerializerOptions(), skipMember: false, writeNulls: false);
        IgnoreWritingNull = CreateOptions(new JsonSerializerOptions(), skipMember: false, writeNulls: true);
        SkipMembers = CreateOptions(new JsonSerializerOptions(), skipMember: true, writeNulls: false);
        SkipMembersWriteNulls = CreateOptions(new JsonSerializerOptions(), skipMember: true, writeNulls: true);
    }

    /// <summary>
    /// The stricter settings, raises error on missing members / reference loops, skips nulls when serializing
    /// </summary>
    public static JsonSerializerOptions IgnoreNulls { get; private set; }

    /// <summary>
    /// The stricter settings, raises error on missing members / reference loops, writes nulls
    /// </summary>
    public static JsonSerializerOptions IgnoreWritingNull { get; private set; }

    /// <summary>
    /// The SkipProperties settings, ignores missing members, reference loops on deserialization,
    /// Silently skips any unmapped properties when serializing
    /// </summary>
    public static JsonSerializerOptions SkipMembers { get; private set; }

    /// <summary>
    /// The SkipMembersWriteNulls settings, ignores missing members, reference loops on deserialization,
    /// writes nulls
    /// </summary>
    public static JsonSerializerOptions SkipMembersWriteNulls { get; private set; }

    /// <summary>
    /// Creates a JsonSerializerSettings object with common values and converters.
    /// </summary>
    /// <param name="skipMember">True to ignore deserializing unmapped members</param>
    /// <param name="writeNulls">True to write null values</param>
    public static JsonSerializerOptions CreateOptions(JsonSerializerOptions options, bool skipMember = false, bool writeNulls = false)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        options.PropertyNamingPolicy = null;
        options.PropertyNameCaseInsensitive = true;
        options.ReadCommentHandling = JsonCommentHandling.Skip;
        options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(UnicodeRanges.All);
        options.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals |
                                 JsonNumberHandling.AllowReadingFromString;
        options.Converters.Add(EnumConverter.Instance);
        options.Converters.Add(Int64Converter.Instance);
        options.Converters.Add(ObjectConverter.Instance);
        options.Converters.Add(NullableConverter.Instance);

        if (!writeNulls)
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        if (!skipMember)
            options.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
        return options;
    }
}