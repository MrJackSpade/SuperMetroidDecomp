namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// What Samus's most recent vertical movement or wall jump published as its solid vertical collision result, read by
/// the released-grapple movement and by the next downward pass.
/// </summary>
/// <remarks>
/// This is one result the game holds once, whose last writer wins (#2560): a wall jump writes its own mark into the same
/// result a vertical pass writes, a later landing, ceiling hit or upward pass replaces it, and only a downward pass that
/// meets nothing leaves it in place. Held as two values, the wall jump's mark would outlive the results that replace it.
/// </remarks>
public enum SamusVerticalCollisionResult
{
    /// <summary>Upward movement without a collision, or a movement that clears the result.</summary>
    None,

    /// <summary>Downward movement hit a solid surface.</summary>
    Landed,

    /// <summary>Downward movement without a collision.</summary>
    Falling,

    /// <summary>Upward movement hit a solid surface.</summary>
    HitCeiling,

    /// <summary>A wall jump was triggered; a downward pass without a collision leaves this result in place.</summary>
    WallJump,
}
