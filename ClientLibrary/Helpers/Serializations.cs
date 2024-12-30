using System.Text.Json;

namespace ClientLibrary.Helpers;

public class Serializations
{
    /// <summary>
    /// Serializes an object to a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="modelObject">The object to serialize.</param>
    /// <returns>The JSON string representation of the object.</returns>
    public static string SerializeObj<T>(T modelObject)
    {
        try
        {
            return JsonSerializer.Serialize(modelObject, new JsonSerializerOptions
            {
                WriteIndented = true // Pretty print the JSON
            });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Serialization failed.", ex);
        }
    }

    /// <summary>
    /// Deserializes a JSON string to an object.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="jsonString">The JSON string to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    public static T? DeserializeJsonString<T>(string jsonString)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(jsonString);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Deserialization failed.", ex);
        }
    }

    /// <summary>
    /// Deserializes a JSON string into a list of objects.
    /// </summary>
    /// <typeparam name="T">The type of the objects in the list.</typeparam>
    /// <param name="jsonString">The JSON string to deserialize.</param>
    /// <returns>A list of objects of type T.</returns>
    public static IList<T> DeserializeJsonStringList<T>(string jsonString)
    {
        try
        {
            // Deserialize the JSON string into a list of type T
            var result = JsonSerializer.Deserialize<IList<T>>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Allow case-insensitive property matching
            });

            // Return the result or an empty list if null
            return result ?? new List<T>();
        }
        catch (JsonException jsonEx)
        {
            // Log JSON-specific errors
            Console.WriteLine($"JSON Error: {jsonEx.Message}");
            throw new InvalidOperationException("Failed to deserialize JSON string to list.", jsonEx);
        }
        catch (Exception ex)
        {
            // Log general errors
            Console.WriteLine($"Error: {ex.Message}");
            throw new InvalidOperationException("An unexpected error occurred during deserialization.", ex);
        }
    }
}