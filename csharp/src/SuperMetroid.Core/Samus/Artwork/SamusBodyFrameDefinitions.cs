using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The body-frame lists' extents and the grapple's angular allocation sets.</summary>
/// <remarks>The exact remaining source selections name chosen character images and bounded transfer-slot content. These image identities and named sequence memberships cannot be generated from physics without restating the selected animation. Batch 104 records precise membership; pixel data, OAM parts and delays are separate owners.</remarks>
internal static class SamusBodyFrameDefinitions
{
    /// <summary>$92:DC20..DC33 and DC34..DC47: each crouching X-ray list has five four-byte records. Its extent derives from the adjacent named allocation.</summary>
    internal static int XrayFrameCount => FrameExtent(SamusPoseId.XrayingCrouchingRightPose, SamusPoseId.XrayingCrouchingLeftPose);

    /// <summary>$92:DC48/DC70/DC98/DCC0: the shared moving lower-body gait has ten records; the extent derives from the adjacent named allocation.</summary>
    internal static int MovingFrameCount => FrameExtent(SamusPoseId.MovingRightNormalPose, SamusPoseId.MovingLeftNormalPose);

    /// <summary>$92:DB48/DB6C and DE18/DE3C: selected normal standing/crouching lists contain nine records; the count derives from the adjacent named allocation.</summary>
    internal static int NormalFrameCount => FrameExtent(SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose);
    /// <summary>$92:DD18/DD20/DD28/DD30/DD48/DD50/DEB0/DEB8: the shared jump/fall sequences contain two records; the count derives from the adjacent named allocation.</summary>
    internal static int JumpFrameCount => FrameExtent(SamusPoseId.NormalJumpAimDownRightPose, SamusPoseId.NormalJumpAimDownLeftPose);
    /// <summary>The selector byte at which <paramref name="pose"/>'s default frame list begins.</summary>
    internal static int ListStart(SamusPoseId pose) => SamusBodyPoseFrames.DefaultFirstFrame(pose) * 4;

    /// <summary>Derives a four-byte frame-list length from adjacent native pose allocations.</summary>
    internal static int FrameExtent(SamusPoseId first, SamusPoseId next) => (ListStart(next) - ListStart(first)) / 4;

    /// <summary>$92:D928 -> CF6E, native top set5, right grapple outer angular allocation.</summary>
    internal const int GrappleRightOuterSet = 5;
    /// <summary>$92:D924 -> CE80, native top set3, right grapple middle half-turn allocation.</summary>
    internal const int GrappleRightMiddleSet = 3;
    /// <summary>$92:D92A -> CFE5, native top set6, left grapple outer angular allocation.</summary>
    internal const int GrappleLeftOuterSet = 6;
    /// <summary>$92:D926 -> CEF7, native top set4, left grapple middle half-turn allocation.</summary>
    internal const int GrappleLeftMiddleSet = 4;
}
