namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>How one of Phantoon's collision rectangles reacts when a Samus projectile hits it.</summary>
internal enum PhantoonHitboxShot
{
    /// <summary>Phantoon's own shot reaction, which decides whether the hit damages it: every visible body and eye rectangle.</summary>
    Vulnerable = 1,
    /// <summary>No reaction: the single point carried by hidden parts and the tentacle frames.</summary>
    Inert,
}
