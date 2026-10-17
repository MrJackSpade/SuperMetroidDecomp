using SuperMetroid.Core.Rooms.Plms;
namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>A resident grey door cap: its unlock condition and native hit counter (<c>$7E:DF0C</c>).</summary>
internal sealed class GreyDoorPlmState(GreyDoorCondition condition) : PlmFamilyState
{
    /// <inheritdoc/>
    internal override PlmFamily Family => PlmFamily.GreyDoor;

    /// <summary>The condition that unlocks the cap.</summary>
    public GreyDoorCondition Condition { get; } = condition;
    /// <summary>Hits counted toward the grey threshold. It only counts up, by one per hit, and the door opens once it reaches its threshold, so it stays far below the 255 the cartridge's byte could hold.</summary>
    public int HitCounter { get; set; }
}
