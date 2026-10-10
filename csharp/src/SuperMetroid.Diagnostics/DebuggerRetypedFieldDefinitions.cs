using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Desktop;

/// <summary>
/// Exact historical payload types of fields whose primitive was replaced by a domain type
/// (#627). Each conversion decodes the older payload through the domain's own validation, so
/// an out-of-domain saved value is rejected rather than restored.
/// </summary>
internal static class DebuggerRetypedFieldDefinitions
{
    // An explicit inventory, not permission to coerce arbitrary mismatched payloads.
    private static readonly Dictionary<(Type DeclaringType, string Name, Type SavedType), Func<object, object>> Conversions = new()
    {
        // The door header's orientation byte became its decoded direction and closing behavior.
        [(typeof(CartridgeDoorHeader), "<Orientation>k__BackingField", typeof(byte))] =
            saved => CartridgeDoorOrientation.Decode((byte)saved),
        // The door-opening scroll kept the masked two-bit direction as an int.
        [(CoreType("SuperMetroid.Core.Runtime.DoorOpeningScrollState"), "<Direction>k__BackingField", typeof(int))] =
            saved => DebuggerEnumValues.Decode(typeof(DoorDirection), checked((byte)(int)saved)),
    };

    private static Type CoreType(string fullName) =>
        typeof(SuperMetroidGame).Assembly.GetType(fullName, throwOnError: true)!;

    /// <summary>Converts an older payload of a retyped field to the field's current type.</summary>
    internal static bool TryConvert(FieldInfo field, object saved, out object converted)
    {
        if (Conversions.TryGetValue((field.DeclaringType!, field.Name, saved.GetType()), out Func<object, object>? convert))
        {
            converted = convert(saved);
            return true;
        }
        converted = saved;
        return false;
    }
}
