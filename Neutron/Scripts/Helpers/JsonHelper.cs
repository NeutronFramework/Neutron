using System.Collections;
using System.Text.Json;

namespace Neutron.Scripts.Helpers;

public static class JsonHelper
{
    /// <summary>
    /// Help deserialize a JsonElement and map it to a T? type
    /// </summary>
    /// <typeparam name="T">The type to map</typeparam>
    /// <param name="element">The javascript parameter as an JsonElement</param>
    /// <returns>The result in T type</returns>
    /// <exception cref="InvalidOperationException">Throwed when it failed to deserialize something</exception>
    public static T? DeserializeToCSharp<T>(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Null)
        {
            return default;
        }

        Type type = typeof(T);

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = Nullable.GetUnderlyingType(type)!;
        }

        if (type == typeof(byte) && element.ValueKind == JsonValueKind.Number)
        {
            return (T)(object)element.GetByte();
        }
        if (type == typeof(short) && element.ValueKind == JsonValueKind.Number)
        {
            return (T)(object)element.GetInt16();
        }
        if (type == typeof(ushort) && element.ValueKind == JsonValueKind.Number)
        {
            return (T)(object)element.GetUInt16();
        }
        if (type == typeof(int) && element.ValueKind == JsonValueKind.Number)
        {
            return (T)(object)element.GetInt32();
        }
        if (type == typeof(uint) && element.ValueKind == JsonValueKind.Number)
        {
            return (T)(object)element.GetUInt32();
        }
        if (type == typeof(long) && element.ValueKind == JsonValueKind.Number)
        {
            return (T)(object)element.GetInt64();
        }
        if (type == typeof(ulong) && element.ValueKind == JsonValueKind.Number)
        {
            return (T)(object)element.GetUInt64();
        }
        if (type == typeof(float) && element.ValueKind == JsonValueKind.Number)
        {
            return (T)(object)element.GetSingle();
        }
        if (type == typeof(double) && element.ValueKind == JsonValueKind.Number)
        {
            return (T)(object)element.GetDouble();
        }
        if (type == typeof(decimal) && element.ValueKind == JsonValueKind.Number)
        {
            return (T)(object)element.GetDecimal();
        }
        if (type == typeof(bool) && (element.ValueKind == JsonValueKind.True || element.ValueKind == JsonValueKind.False))
        {
            return (T)(object)element.GetBoolean();
        }
        if (type == typeof(string) && element.ValueKind == JsonValueKind.String)
        {
            return (T)(object)element.GetString()!;
        }
        if (type == typeof(DateTime) && element.ValueKind == JsonValueKind.String)
        {
            return (T)(object)element.GetDateTime();
        }
        if (type == typeof(DateTimeOffset) && element.ValueKind == JsonValueKind.String)
        {
            return (T)(object)element.GetDateTimeOffset();
        }
        if (type == typeof(Guid) && element.ValueKind == JsonValueKind.String)
        {
            return (T)(object)element.GetGuid();
        }
        if (type == typeof(JsonElement))
        {
            return (T)(object)element;
        }

        if (type.IsEnum)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                return (T)Enum.Parse(type, element.GetString()!);
            }
            if (element.ValueKind == JsonValueKind.Number)
            {
                return (T)Enum.ToObject(type, element.GetInt32());
            }
        }

        if (typeof(IEnumerable).IsAssignableFrom(type) && element.ValueKind == JsonValueKind.Array)
        {
            return (T)DeserializeEnumerable(type, element);
        }

        if (element.ValueKind == JsonValueKind.Object || (type.IsClass && !type.IsPrimitive))
        {
            return JsonSerializer.Deserialize<T>(element.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        throw new InvalidOperationException($"Unable to deserialize type {typeof(T)} from JsonElement of kind {element.ValueKind}");
    }

    private static object DeserializeEnumerable(Type type, JsonElement element)
    {
        if (type.IsArray)
        {
            Type elementType = type.GetElementType()!;
            
            var array = Array.CreateInstance(elementType, element.GetArrayLength());
            int index = 0;
            
            foreach (var item in element.EnumerateArray())
            {
                array.SetValue(DeserializeToType(elementType, item), index++);
            }

            return array;
        }

        if (type.IsGenericType)
        {
            Type genericType = type.GetGenericTypeDefinition();
            
            if (genericType == typeof(List<>) || genericType == typeof(IList<>) || genericType == typeof(ICollection<>) || genericType == typeof(IEnumerable<>))
            {
                Type elementType = type.GetGenericArguments()[0];
                
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;

                foreach (var item in element.EnumerateArray())
                {
                    list.Add(DeserializeToType(elementType, item));
                }
                return list;
            }
        }

        throw new InvalidOperationException($"Unsupported collection type: {type}");
    }

    private static object? DeserializeToType(Type type, JsonElement element)
    {
        var method = typeof(JsonHelper).GetMethod(nameof(DeserializeToCSharp))!.MakeGenericMethod(type);

        return method.Invoke(null, [element]);
    }

}
