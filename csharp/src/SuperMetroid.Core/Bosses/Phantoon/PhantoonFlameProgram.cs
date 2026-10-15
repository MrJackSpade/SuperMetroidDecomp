namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's typed flame programs (#641), in the authored order of their bank-$86 lists.</summary>
internal enum PhantoonFlameProgram
{
    /// <summary>The destroyable flame's three-pose idle loop ($86:975C).</summary>
    DestroyableIdle,
    /// <summary>A casual flame's landing pose ($86:976C).</summary>
    CasualHitGround,
    /// <summary>A casual flame's three-pose bouncing loop ($86:9772).</summary>
    CasualBouncing,
    /// <summary>A casual flame's six resting poses, running on into <see cref="Dying"/> ($86:9782).</summary>
    CasualResting,
    /// <summary>A flame's four dying poses, then its deletion ($86:979A).</summary>
    Dying,
    /// <summary>A rain flame's impact pose, then <see cref="Dying"/> ($86:97AC).</summary>
    RainImpact,
    /// <summary>A casual flame's first falling phase: loads the repeat counter ($86:97B4).</summary>
    CasualFalling0,
    /// <summary>The first falling phase's two poses, repeated while counting ($86:97B8).</summary>
    CasualFalling0Loop,
    /// <summary>The second falling phase: loads the repeat counter ($86:97C4).</summary>
    CasualFalling1,
    /// <summary>The second falling phase's two poses, repeated while counting ($86:97C8).</summary>
    CasualFalling1Loop,
    /// <summary>The third falling phase: loads the repeat counter ($86:97D4).</summary>
    CasualFalling2,
    /// <summary>The third falling phase's two poses, repeated while counting, then the first phase again ($86:97D8).</summary>
    CasualFalling2Loop,
    /// <summary>A starting flame's three-pose loop ($86:97E8).</summary>
    StartingFlame,
    /// <summary>Deletes the flame ($86:97F8).</summary>
    Delete,
    /// <summary>A shot flame's four poses, its drop and its deletion ($86:97FA).</summary>
    DestroyableShot,
}