namespace SuperMetroid.Core.Animation;

/// <summary>
/// Whether an animation's play moves on (#2560): one value where the player kept a resting and a stopped flag, of which
/// the second implied the first.
/// </summary>
internal enum AnimationMotion
{
    /// <summary>The play moves on as each frame's or pause's updates run out.</summary>
    Playing,
    /// <summary>The play rests for good: on a held frame, or past its end where it holds.</summary>
    Resting,
    /// <summary>The play has stopped: it played its last phase and ends there, showing nothing more.</summary>
    Stopped,
}
