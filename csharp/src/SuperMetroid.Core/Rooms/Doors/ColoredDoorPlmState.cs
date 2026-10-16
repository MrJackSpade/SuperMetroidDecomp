using SuperMetroid.Core.Rooms.Plms;
namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>A resident coloured door cap: its colour and native hit counter (<c>$7E:DF0C</c>).</summary>
internal sealed class ColoredDoorPlmState(ColoredDoorColor color) : PlmFamilyState
{
    /// <inheritdoc/>
    internal override PlmFamily Family => PlmFamily.ColoredDoor;

    /// <summary>The cap's colour, which selects the projectile that opens it.</summary>
    public ColoredDoorColor Color { get; } = color;
    /// <summary>Hits counted toward the colour's threshold.</summary>
    public byte HitCounter { get; set; }
}
