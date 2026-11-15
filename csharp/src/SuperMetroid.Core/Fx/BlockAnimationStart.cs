namespace SuperMetroid.Core.Fx;

/// <summary>When a block animation's loop starts showing its frames (#1988). Its values are arbitrary.</summary>
public enum BlockAnimationStart
{
    /// <summary>The loop shows its first frame on its first pass.</summary>
    Immediately,

    /// <summary>
    /// The loop waits, showing nothing, until a pass on which the area's boss is recorded defeated, and shows its first
    /// frame on that pass; it never tests the boss again. The Wrecked Ship treadmills wait for Phantoon this way.
    /// </summary>
    OnceAreaBossIsDefeated,
}
