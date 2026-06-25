using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Rps.Api.Http.Shared.Extensions;

public static class EnumExtensions
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, object>> _cache = new();

    public static string ToDisplayString(this Enum value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.GetType()
                .GetField(value.ToString())
                ?.GetCustomAttribute<DescriptionAttribute>()
                ?.Description ?? value.ToString();
    }

    public static T ToEnum<T>(this string description) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Cannot parse null or empty string to enum.", nameof(description));

        var map = _cache.GetOrAdd(typeof(T), _ => BuildMap<T>());

        if (map.TryGetValue(description, out var value))
        {
            return (T)value;
        }

        if (Enum.TryParse<T>(description, ignoreCase: true, out var parsedEnum))
        {
            return parsedEnum;
        }

        throw new ArgumentException($"No enum member with description '{description}' found in {typeof(T).Name}");
    }

    private static Dictionary<string, object> BuildMap<T>() where T : struct, Enum
    {
        var map = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var enumValue = field.GetValue(null)!;
            
            map[field.Name] = enumValue;

            if (field.GetCustomAttribute<DescriptionAttribute>() is { } attr)
            {
                map[attr.Description] = enumValue;
            }
        }

        return map;
    }
}
