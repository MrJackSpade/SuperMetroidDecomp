namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>How one of Phantoon's collision rectangles reacts when Samus touches it.</summary>
internal enum PhantoonHitboxTouch
{
    /// <summary>Phantoon's own contact reaction: every rectangle of the visible body and its vulnerable eye.</summary>
    Contact = 1,
    /// <summary>No reaction: the single point carried by hidden parts and the tentacle frames.</summary>
    Inert,
}
