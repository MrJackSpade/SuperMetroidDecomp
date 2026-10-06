using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Source-identified shared body-frame components; chosen source artwork and frame extents remain REQUIRED.</summary>
internal static class SamusBodyFrameDefinitions
{
    /// <summary>$92:DC20..DC33 and DC34..DC47: each crouching X-ray list has five four-byte records. This selected sequence extent remains REQUIRED.</summary>
    internal const int RequiredXrayFrameCount = 5;

    /// <summary>$92:DC48/DC70/DC98/DCC0: the shared moving lower-body gait has ten records; selected order/count remain REQUIRED.</summary>
    internal const int RequiredMovingFrameCount = 10;

    /// <summary>$92:DB48/DB6C and DE18/DE3C: selected normal standing/crouching lists contain nine records; count remains REQUIRED.</summary>
    internal const int RequiredNormalFrameCount = 9;
    /// <summary>$92:DD18/DD20/DD28/DD30/DD48/DD50/DEB0/DEB8: the shared jump/fall sequences contain two records; count remains REQUIRED.</summary>
    internal const int RequiredJumpFrameCount = 2;

    /// <summary>
    /// Crouching X-ray upper set/position pairs at $92:DC20/DC34 share standing
    /// $92:DBF8/DC0C frame selections, respectively. Lower set/position pairs remain
    /// independent. Moving left and either gun-extended list share right-moving lower
    /// components at $92:DC48, preserving their independent upper selections.
    /// The six aiming-moving lists at $92:DF28/DF50/DF78/DFA0/DFC8/DFF0
    /// use the same lower gait. Normal crouching shares standing upper components;
    /// forward jumps share stationary gun-extended frames, and downward-aimed falls
    /// share their jump frames. All selected source components remain REQUIRED.
    /// </summary>
    internal static int SourceComponent(int index)
    {
        if ((uint)index >= SamusBodyArtworkCatalog.FrameCount * 4) throw new ArgumentOutOfRangeException(nameof(index));
        int address = SamusBodyArtworkCatalog.FirstFrameOffset + index;
        int Source(SamusPoseId target, SamusPoseId basis, int frameCount, int half)
        {
            int offset = address - SamusBodyPoseDefinitions.DefaultFrameList((byte)target);
            return (uint)offset < frameCount * 4 && (half < 0 || offset % 4 / 2 == half)
                ? SamusBodyPoseDefinitions.DefaultFrameList((byte)basis) + offset - SamusBodyArtworkCatalog.FirstFrameOffset : index;
        }
        int source = Source(SamusPoseId.XrayingCrouchingRightPose, SamusPoseId.XrayingStandingRightPose, RequiredXrayFrameCount, 0);
        if (source != index) return source;
        source = Source(SamusPoseId.XrayingCrouchingLeftPose, SamusPoseId.XrayingStandingLeftPose, RequiredXrayFrameCount, 0);
        if (source != index) return source;
        source = Source(SamusPoseId.MovingLeftNormalPose, SamusPoseId.MovingRightNormalPose, RequiredMovingFrameCount, 1);
        if (source != index) return source;
        source = Source(SamusPoseId.MovingRightGunExtendedPose, SamusPoseId.MovingRightNormalPose, RequiredMovingFrameCount, 1);
        if (source != index) return source;
        source = Source(SamusPoseId.MovingLeftGunExtendedPose, SamusPoseId.MovingRightNormalPose, RequiredMovingFrameCount, 1);
        if (source != index) return source;
        source = Source(SamusPoseId.RunningAimUpRightPose, SamusPoseId.MovingRightNormalPose, RequiredMovingFrameCount, 1);
        if (source != index) return source;
        source = Source(SamusPoseId.RunningAimUpLeftPose, SamusPoseId.MovingRightNormalPose, RequiredMovingFrameCount, 1);
        if (source != index) return source;
        source = Source(SamusPoseId.RunningAimDiagonalUpRightPose, SamusPoseId.MovingRightNormalPose, RequiredMovingFrameCount, 1);
        if (source != index) return source;
        source = Source(SamusPoseId.RunningAimDiagonalUpLeftPose, SamusPoseId.MovingRightNormalPose, RequiredMovingFrameCount, 1);
        if (source != index) return source;
        source = Source(SamusPoseId.RunningAimDiagonalDownRightPose, SamusPoseId.MovingRightNormalPose, RequiredMovingFrameCount, 1);
        if (source != index) return source;
        source = Source(SamusPoseId.RunningAimDiagonalDownLeftPose, SamusPoseId.MovingRightNormalPose, RequiredMovingFrameCount, 1);
        if (source != index) return source;
        source = Source(SamusPoseId.CrouchingRightPose, SamusPoseId.FacingRightNormalPose, RequiredNormalFrameCount, 0);
        if (source != index) return source;
        source = Source(SamusPoseId.CrouchingLeftPose, SamusPoseId.FacingLeftNormalPose, RequiredNormalFrameCount, 0);
        if (source != index) return source;
        source = Source(SamusPoseId.NormalJumpForwardRightPose, SamusPoseId.NormalJumpGunExtendedRightPose, RequiredJumpFrameCount, -1);
        if (source != index) return source;
        source = Source(SamusPoseId.NormalJumpForwardLeftPose, SamusPoseId.NormalJumpGunExtendedLeftPose, RequiredJumpFrameCount, -1);
        if (source != index) return source;
        source = Source(SamusPoseId.FallingAimDownRightPose, SamusPoseId.NormalJumpAimDownRightPose, RequiredJumpFrameCount, -1);
        return source != index ? source : Source(SamusPoseId.FallingAimDownLeftPose, SamusPoseId.NormalJumpAimDownLeftPose, RequiredJumpFrameCount, -1);
    }
}
