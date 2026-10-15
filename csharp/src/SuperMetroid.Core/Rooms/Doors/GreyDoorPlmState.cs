namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>A resident grey door cap: its facing, unlock condition and native hit counter (<c>$7E:DF0C</c>).</summary>
internal sealed class GreyDoorPlmState(ColoredDoorOrientation orientation, GreyDoorCondition condition)
{
    /// <summary>The cap's facing.</summary>
    public ColoredDoorOrientation Orientation { get; } = orientation;
    /// <summary>The condition that unlocks the cap.</summary>
    public GreyDoorCondition Condition { get; } = condition;
    /// <summary>Hits counted toward the grey threshold.</summary>
    public byte HitCounter { get; set; }
}
