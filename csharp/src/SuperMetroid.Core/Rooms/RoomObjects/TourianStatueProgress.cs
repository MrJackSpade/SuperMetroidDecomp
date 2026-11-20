namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Whether the room's Tourian entrance statue has finished sinking, as the room's grey caps read it (#2486, #2554). Every
/// room's load starts it unfinished, as every room's effect setup clears it; only the statue room's statues finish
/// it, so a cap waiting for it anywhere else stays locked.
/// </summary>
[RoomLifetime]
public sealed class TourianStatueProgress : ITourianStatueProgress
{
    /// <inheritdoc/>
    public bool IsFinished { get; private set; }

    /// <summary>The entrance statue has finished sinking.</summary>
    internal void Finish() => IsFinished = true;
}
