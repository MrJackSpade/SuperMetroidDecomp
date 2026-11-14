namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's typed flame animations (#641, #1788): one for each animation a flame's spawn or reaction starts, in authored order.</summary>
internal enum PhantoonFlameSegment
{
    /// <summary>The destroyable flame's three-pose idle loop.</summary>
    DestroyableIdle,
    /// <summary>A casual flame's landing pose.</summary>
    CasualHitGround,
    /// <summary>A casual flame's three-pose bouncing loop.</summary>
    CasualBouncing,
    /// <summary>A casual flame's six resting poses, running on into <see cref="Dying"/>.</summary>
    CasualResting,
    /// <summary>A flame's four dying poses, then its deletion.</summary>
    Dying,
    /// <summary>A rain flame's impact pose, then <see cref="Dying"/>.</summary>
    RainImpact,
    /// <summary>A casual flame's fall: each of its three falling phases' two poses four times, the phases in turn for good.</summary>
    CasualFalling,
    /// <summary>A starting flame's three-pose loop.</summary>
    StartingFlame,
    /// <summary>Deletes the flame.</summary>
    Delete,
    /// <summary>A shot flame's four poses, its drop and its deletion.</summary>
    DestroyableShot,
}
