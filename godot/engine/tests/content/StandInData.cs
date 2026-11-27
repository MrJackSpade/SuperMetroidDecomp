using System.Reflection;
using System.Text.Json.Nodes;
using Engine.Content;
using Engine.Shared;

namespace Engine.Tests.Content;

/// <summary>
/// Makes the text of a data file out of the values a thing has: for a check that must give something a data file of
/// its own to say one value, where a data file has to give every value. The check adds what it means to say and what
/// the thing reads for itself, and nothing has to be typed that the engine may add to later.
/// </summary>
public static class StandInData
{
    /// <summary>
    /// Every value of <paramref name="target"/> that its data file gives it directly, as it stands now, by the name
    /// the file gives it under. Values the target reads for itself (<see cref="DataValueAttribute.ReadByOwner"/>) are
    /// left for the caller.
    /// </summary>
    /// <exception cref="NotSupportedException">A value is of a kind this cannot write.</exception>
    public static JsonObject ValuesOf(IHasValues target)
    {
        ArgumentNullException.ThrowIfNull(target);
        var data = new JsonObject();
        foreach (PropertyInfo property in target.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (property.GetCustomAttribute<DataValueAttribute>() is not { ReadByOwner: false } value)
                continue;
            data[value.Name] = property.GetValue(target) switch
            {
                int whole => whole,
                bool flag => flag,
                string text => text,
                double number => number,
                Pixels pixels => pixels.ToNumber(),
                Enum named => named.ToString(),
                string[] texts => new JsonArray([.. texts.Select(text => (JsonNode?)text)]),
                int[] wholes => new JsonArray([.. wholes.Select(whole => (JsonNode?)whole)]),
                object other => throw new NotSupportedException($"The value \"{value.Name}\" is a {other.GetType().Name}, which this does not write."),
                null => throw new NotSupportedException($"The value \"{value.Name}\" is not set."),
            };
        }

        return data;
    }
}
