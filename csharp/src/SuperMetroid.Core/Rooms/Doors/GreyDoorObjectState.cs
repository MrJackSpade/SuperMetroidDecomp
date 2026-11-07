using SuperMetroid.Core.Rooms.RoomObjects;
namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>A resident grey door cap: its unlock condition and hit counter.</summary>
internal sealed class GreyDoorObjectState(GreyDoorCondition condition) : RoomObjectFamilyState
{
    /// <inheritdoc/>
    internal override RoomObjectFamily Family => RoomObjectFamily.GreyDoor;

    /// <summary>The condition that unlocks the cap.</summary>
    public GreyDoorCondition Condition { get; } = condition;
    /// <summary>Hits counted toward the grey threshold. It only counts up, by one per hit, and the door opens once it reaches its threshold, so it stays far below the 255 the original byte counter could hold.</summary>
    public int HitCounter { get; set; }
}
