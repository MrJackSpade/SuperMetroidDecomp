namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>A resident coloured door cap: its colour, facing and native hit counter (<c>$7E:DF0C</c>).</summary>
internal sealed class ColoredDoorPlmState(ColoredDoorColor color, ColoredDoorOrientation orientation)
{
    /// <summary>The cap's colour, which selects the projectile that opens it.</summary>
    public ColoredDoorColor Color { get; } = color;
    /// <summary>The cap's facing.</summary>
    public ColoredDoorOrientation Orientation { get; } = orientation;
    /// <summary>Hits counted toward the colour's threshold.</summary>
    public byte HitCounter { get; set; }
}
