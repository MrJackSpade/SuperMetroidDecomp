namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// The Morph Ball eye's eyelid and mount animations (#641).
/// Their steps are <see cref="MorphBallEyeAnimations"/>.
/// </summary>
internal enum MorphBallEyeSegment
{
    /// <summary>
    /// The eye on the right-facing mount half open, looking across, then half open again before it closes, held shut.
    /// </summary>
    FacingRightDeactivating,
    /// <summary>The eye on the right-facing mount shut, held.</summary>
    FacingRightClosed,
    /// <summary>
    /// The eye on the left-facing mount half open, looking across, then half open again before it closes, held shut.
    /// </summary>
    FacingLeftDeactivating,
    /// <summary>The eye on the left-facing mount shut, held.</summary>
    FacingLeftClosed,
    /// <summary>
    /// The eye on the right-facing mount shut, then opening halfway and looking across, ending half open.
    /// </summary>
    FacingRightActivating,
    /// <summary>
    /// The eye on the left-facing mount shut, then opening halfway and looking across, ending half open.
    /// </summary>
    FacingLeftActivating,
    /// <summary>The eye's mount facing right, held.</summary>
    MountFacingRight,
    /// <summary>The eye's mount facing down, held.</summary>
    MountFacingDown,
    /// <summary>The eye's mount facing left, held.</summary>
    MountFacingLeft,
    /// <summary>The eye's mount facing up, held.</summary>
    MountFacingUp,
}
