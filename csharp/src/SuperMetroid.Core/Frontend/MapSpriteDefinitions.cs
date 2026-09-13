namespace SuperMetroid.Core.Frontend;

/// <summary>Semantic identities for $82:C569 menu spritemaps used by map presentation.</summary>
public static class MapSpriteDefinitions
{
    /// <summary>$82:C749 selects spritemap $38 for the world-map title, followed by six area names.</summary>
    public const ushort WorldTitle = 0x38;
    /// <summary>Native map draw calls bind these frames; authored files contain names, not these numeric dispatcher identities.</summary>
    private static readonly (ushort NativeId, string Name)[] frames =
    [
        (4, "Arrow.Right"), (5, "Arrow.Left"), (6, "Arrow.Up"), (7, "Arrow.Down"),
        (9, "Marker.Boss"), (10, "Station.Energy"), (11, "Station.Missile"), (0x4e, "Station.Map"),
        (0x62, "Marker.DefeatedBoss"), (0x63, "Marker.Gunship"), (0x12, "Indicator.Backing"),
        (0x5f, "Indicator.Frame0"), (0x60, "Indicator.Frame1"), (0x61, "Indicator.Frame2"),
        (0x59, "Elevator.Crateria"), (0x5a, "Elevator.Brinstar"), (0x5b, "Elevator.Norfair"),
        (0x5c, "Elevator.WreckedShip"), (0x5d, "Elevator.Maridia"),
        (WorldTitle, "World.Title"), (0x39, "World.Crateria"), (0x3a, "World.Brinstar"),
        (0x3b, "World.Norfair"), (0x3c, "World.WreckedShip"), (0x3d, "World.Maridia"), (0x3e, "World.Tourian")
    ];
    public static ReadOnlySpan<(ushort NativeId, string Name)> Frames => frames;
    public static bool Contains(ushort id) { foreach (var frame in frames) if (frame.NativeId == id) return true; return false; }
    public static string Name(ushort id)
    {
        foreach (var frame in frames) if (frame.NativeId == id) return frame.Name;
        throw new ArgumentOutOfRangeException(nameof(id), $"Menu sprite {id:X4} is not a map frame.");
    }
}
