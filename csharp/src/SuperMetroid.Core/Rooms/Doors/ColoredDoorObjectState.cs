using SuperMetroid.Core.Rooms.RoomObjects;
namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>A resident coloured door cap: its colour and hit counter.</summary>
internal sealed class ColoredDoorObjectState(ColoredDoorColor color) : RoomObjectFamilyState
{
    /// <inheritdoc/>
    internal override RoomObjectFamily Family => RoomObjectFamily.ColoredDoor;

    /// <summary>The cap's colour, which selects the projectile that opens it.</summary>
    public ColoredDoorColor Color { get; } = color;
    /// <summary>Hits counted toward the colour's threshold. It only counts up, by one per hit or to 120 on a Super Missile hit, and the door opens once it reaches its threshold, so it stays far below the 255 the original byte counter could hold.</summary>
    public int HitCounter { get; set; }
}
