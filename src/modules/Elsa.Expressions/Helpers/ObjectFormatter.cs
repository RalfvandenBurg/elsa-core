using System.Collections;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elsa.Expressions.Helpers;

/// <summary>
/// Provides a set of static methods for formatting objects.
/// </summary>
public static class ObjectFormatter
{
    /// <summary>
    /// Formats the specified value as a string.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A string representation of the value.</returns>
    public static string? Format(this object? value)
    {
        if (value == null)
            return null;

        if (value is string s)
            return s;

        var sourceType = value.GetType();
        var underlyingSourceType = Nullable.GetUnderlyingType(sourceType) ?? sourceType;

        if (underlyingSourceType == typeof(string))
            return value as string;

        if (value is byte[] byteArray)
            return Convert.ToBase64String(byteArray);

        if (IsMemoryLike(sourceType))
        {
            var sourceTypeConverter = TypeDescriptor.GetConverter(underlyingSourceType);
            if (sourceTypeConverter.CanConvertTo(typeof(string)))
                return (string?)sourceTypeConverter.ConvertTo(value, typeof(string));

            return value.ToString();
        }

        if (value is IEnumerable)
            return JsonSerializer.Serialize(value, JsonSerializerOptions);

        if (!IsSimpleValue(value))
            return JsonSerializer.Serialize(value, JsonSerializerOptions);

        var valueTypeConverter = TypeDescriptor.GetConverter(underlyingSourceType);

        if (valueTypeConverter.CanConvertTo(typeof(string)))
            return (string?)valueTypeConverter.ConvertTo(value, typeof(string));

        return value.ToString();
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = null,
        DictionaryKeyPolicy = null,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
    };

    private static bool IsMemoryLike(Type type)
    {
        if (!type.IsGenericType)
            return false;

        var genericTypeDef = type.GetGenericTypeDefinition();
        return genericTypeDef == typeof(Memory<>) || genericTypeDef == typeof(ReadOnlyMemory<>);
    }

    private static bool IsSimpleValue(object value)
    {
        var valueType = value.GetType();
        return valueType.IsPrimitive || valueType.IsEnum || value is string || value is decimal || value is DateTime || value is DateTimeOffset || value is DateOnly || value is TimeOnly || value is Guid || value is TimeSpan;
    }
}