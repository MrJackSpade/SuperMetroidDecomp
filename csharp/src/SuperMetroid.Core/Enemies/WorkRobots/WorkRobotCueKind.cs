namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>What a cue of the Work Robot's animations does: one per kind of step its lists use.</summary>
internal enum WorkRobotCueKind
{
    /// <summary>Counts the laser cooldown down.</summary>
    CountDownLaserCooldown,
    /// <summary>Plays the footstep sound when the Work Robot is on screen.</summary>
    Footstep,
    /// <summary>Steps backward facing left; a wall redirects the walk.</summary>
    LeftBackward,
    /// <summary>Steps backward facing left; a wall or ledge redirects the walk.</summary>
    LeftBackwardWatchingLedges,
    /// <summary>Steps forward facing left; a wall redirects the walk.</summary>
    LeftForward,
    /// <summary>Steps forward facing left; a wall or ledge redirects the walk.</summary>
    LeftForwardWatchingLedges,
    /// <summary>Steps backward facing right; a wall redirects the walk.</summary>
    RightBackward,
    /// <summary>Steps backward facing right; a wall or ledge redirects the walk.</summary>
    RightBackwardWatchingLedges,
    /// <summary>Steps forward facing right; a wall redirects the walk.</summary>
    RightForward,
    /// <summary>Steps forward facing right; a wall or ledge redirects the walk.</summary>
    RightForwardWatchingLedges,
    /// <summary>Fires a down-left laser when the cooldown allows, continuing at its firing animation.</summary>
    TryLaserDownLeft,
    /// <summary>Fires a down-right laser when the cooldown allows, continuing at its firing animation.</summary>
    TryLaserDownRight,
    /// <summary>Fires a laser to the left when the cooldown allows, continuing at its firing animation.</summary>
    TryLaserLeft,
    /// <summary>Fires a laser to the right when the cooldown allows, continuing at its firing animation.</summary>
    TryLaserRight,
    /// <summary>Fires an up-left laser when the cooldown allows, continuing at its firing animation.</summary>
    TryLaserUpLeft,
    /// <summary>Fires an up-right laser when the cooldown allows, continuing at its firing animation.</summary>
    TryLaserUpRight,
    /// <summary>Continues at the left-facing walk.</summary>
    WalkLeft,
    /// <summary>Continues at the right-facing walk.</summary>
    WalkRight,
}
