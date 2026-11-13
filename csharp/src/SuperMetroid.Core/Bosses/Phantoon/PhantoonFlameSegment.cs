namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's typed flame animations (#641), in authored order.</summary>
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
    /// <summary>A casual flame's first falling phase: loads the repeat counter.</summary>
    CasualFalling0,
    /// <summary>The first falling phase's two poses, repeated while counting.</summary>
    CasualFalling0Loop,
    /// <summary>The second falling phase: loads the repeat counter.</summary>
    CasualFalling1,
    /// <summary>The second falling phase's two poses, repeated while counting.</summary>
    CasualFalling1Loop,
    /// <summary>The third falling phase: loads the repeat counter.</summary>
    CasualFalling2,
    /// <summary>The third falling phase's two poses, repeated while counting, then the first phase again.</summary>
    CasualFalling2Loop,
    /// <summary>A starting flame's three-pose loop.</summary>
    StartingFlame,
    /// <summary>Deletes the flame.</summary>
    Delete,
    /// <summary>A shot flame's four poses, its drop and its deletion.</summary>
    DestroyableShot,
}