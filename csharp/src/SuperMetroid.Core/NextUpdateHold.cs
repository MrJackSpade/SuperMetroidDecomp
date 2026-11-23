namespace SuperMetroid.Core;

/// <summary>
/// A hold one update of an actor asks for and its next update keeps (#2562): the game decides at the end of an update that
/// the actor waits, and the actor waits on the update after. The actor releases the hold where its update decides afresh;
/// its owner's documentation names what the hold keeps still and why the game takes it a whole update later.
/// </summary>
public sealed partial class NextUpdateHold
{
    /// <summary>Whether the update before asked for the hold, which this update keeps.</summary>
    public bool IsHeld { get; private set; }

    /// <summary>Asks the actor's next update to hold.</summary>
    internal void HoldNextUpdate() => IsHeld = true;

    /// <summary>Releases the hold where the actor's update decides afresh whether its next update holds.</summary>
    internal void Release() => IsHeld = false;
}
