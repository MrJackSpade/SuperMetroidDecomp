using SuperMetroid.Core.Rooms.Plms;
namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>A resident grey door cap: its unlock condition and native hit counter (<c>$7E:DF0C</c>).</summary>
internal sealed class GreyDoorPlmState(GreyDoorCondition condition) : PlmFamilyState
{
    /// <inheritdoc/>
    internal override PlmFamily Family => PlmFamily.GreyDoor;

    /// <summary>The condition that unlocks the cap.</summary>
    public GreyDoorCondition Condition { get; } = condition;
    /// <summary>Hits counted toward the grey threshold.</summary>
    public byte HitCounter { get; set; }
}
