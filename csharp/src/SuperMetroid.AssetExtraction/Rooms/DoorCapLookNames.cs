using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Names what a door cap's draw shows in its installed picture's identity, and finds it in the caps' draw tables (#1994).</summary>
internal static partial class DoorCapLookNames
{
    /// <summary>The look's words in a cap's editable visual identity.</summary>
    internal static string VisualName(DoorCapLook look) => look switch
    {
        DoorCapLook.Closed => "closed",
        DoorCapLook.Ajar => "ajar",
        DoorCapLook.HalfOpen => "half-open",
        DoorCapLook.NearlyOpen => "nearly-open",
        DoorCapLook.Cleared or DoorCapLook.ClosedSolid => throw new InvalidOperationException($"Door cap look {look} is named by its own family's identity."),
        _ => throw new InvalidOperationException($"Undefined {nameof(DoorCapLook)} {look}."),
    };

    /// <summary>
    /// The position of a closed or sliding cap among its facing's four records in the cartridge's cap tables: closed, then
    /// each step aside in order.
    /// </summary>
    internal static int TableIndex(DoorCapLook look) => look switch
    {
        DoorCapLook.Closed => 0,
        DoorCapLook.Ajar => 1,
        DoorCapLook.HalfOpen => 2,
        DoorCapLook.NearlyOpen => 3,
        DoorCapLook.Cleared or DoorCapLook.ClosedSolid => throw new InvalidDataException($"Door cap look {look} is not a closed or sliding cap."),
        _ => throw new InvalidDataException($"Undefined {nameof(DoorCapLook)} {look}."),
    };
}
