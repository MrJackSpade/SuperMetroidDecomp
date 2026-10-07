using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Calculated native body-frame sharing, cleared slots and angular allocation selection.</summary>
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
    private static int FrameExtent(SamusPoseId first, SamusPoseId next) => (SamusBodyPoseDefinitions.DefaultFrameList((byte)next) - SamusBodyPoseDefinitions.DefaultFrameList((byte)first)) / 4;

    /// <summary>
    /// Crouching X-ray upper set/position pairs at $92:DC20/DC34 share standing
    /// $92:DBF8/DC0C frame selections, respectively. Lower set/position pairs remain
    /// independent. Moving left and either gun-extended list share right-moving lower
    /// components at $92:DC48, preserving their independent upper selections.
    /// The six aiming-moving lists at $92:DF28/DF50/DF78/DFA0/DFC8/DFF0
    /// use the same lower gait. Normal crouching shares standing upper components;
    /// forward jumps share stationary gun-extended frames, and downward-aimed falls
    /// share their jump frames. Remaining source components are the exact selected drawing identities documented in Batch 104.
    /// </summary>
    /// <summary>$92:D928 -> CF6E, native top set5, right grapple outer angular allocation.</summary>
    private const int GrappleRightOuterSet = 5;
    /// <summary>$92:D924 -> CE80, native top set3, right grapple middle half-turn allocation.</summary>
    private const int GrappleRightMiddleSet = 3;
    /// <summary>$92:D92A -> CFE5, native top set6, left grapple outer angular allocation.</summary>
    private const int GrappleLeftOuterSet = 6;
    /// <summary>$92:D926 -> CEF7, native top set4, left grapple middle half-turn allocation.</summary>
    private const int GrappleLeftMiddleSet = 4;
    /// <summary>$92:E050/E158: upper grapple characters follow the same three angular allocation runs as their OAM compositions. Set identities select the native glyph allocation; positions advance or reverse within it.</summary>
    internal static bool TryComponent(int index, out byte value)
    {
        int address = SamusBodyArtworkCatalog.FirstFrameOffset + index;
        bool ClearSlot(SamusPoseId pose, int phase) => (uint)(address - SamusBodyPoseDefinitions.DefaultFrameList((byte)pose) - phase * 4) < 4;
        // Explicit cleared transfer slots belonging to native animation control
        // or omitted-body phases. Do not generalize to every bytecode operand.
        if (ClearSlot(SamusPoseId.FacingRightNormalPose, 4) || ClearSlot(SamusPoseId.FacingLeftNormalPose, 4) ||
            ClearSlot(SamusPoseId.CrouchingRightPose, 4) || ClearSlot(SamusPoseId.CrouchingLeftPose, 4) ||
            ClearSlot(SamusPoseId.WallJumpRightPose, 2) || ClearSlot(SamusPoseId.WallJumpRightPose, 11) ||
            ClearSlot(SamusPoseId.WallJumpRightPose, 12) || ClearSlot(SamusPoseId.WallJumpRightPose, 21) || ClearSlot(SamusPoseId.WallJumpRightPose, 22) ||
            ClearSlot(SamusPoseId.CrystalFlashRightPose, 4) || ClearSlot(SamusPoseId.CrystalFlashLeftPose, 4) ||
            ClearSlot(SamusPoseId.DrainedCrouchingRightPose, 12) || ClearSlot(SamusPoseId.DrainedCrouchingLeftPose, 12) ||
            ClearSlot(SamusPoseId.DrainedStandingRightPose, 4) || ClearSlot(SamusPoseId.DrainedStandingLeftPose, 4) ||
            ClearSlot(SamusPoseId.MorphBallGroundRightPose, 8) ||
            ClearSlot(SamusPoseId.ForwardFacingPowerSuitPose, 1) || ClearSlot(SamusPoseId.ForwardFacingSuitedPose, 1))
        { value = 0; return true; }
        foreach (bool left in (ReadOnlySpan<bool>)[ false, true ])
        {
            int offset = address - SamusBodyPoseDefinitions.DefaultFrameList((byte)(left
                ? SamusPoseId.GrappleSwingLeftPose : SamusPoseId.GrappleSwingRightPose));
            if ((uint)offset >= GrappleSwingFrameDefinitions.FrameCount * 4 || offset % 4 >= 2) continue;
            int phase = offset / 4;
            value = (byte)(offset % 4 == 0
                ? left ? phase < 8 || phase >= 24 ? GrappleLeftOuterSet : GrappleLeftMiddleSet : phase <= 8 || phase > 24 ? GrappleRightOuterSet : GrappleRightMiddleSet
                : left ? phase < 8 ? phase + 8 : phase < 24 ? phase - 8 : phase - 24
                    : phase <= 8 ? 8 - phase : phase <= 24 ? 24 - phase : 40 - phase);
            return true;
        }
        value = 0;
        return false;
    }
    internal static int SourceComponent(int index)
    {
        int source = SourceStep(index);
        while (source != index)
        {
            if (source >= index) throw new InvalidOperationException("Body-frame aliases must reference an earlier native component.");
            index = source;
            source = SourceStep(index);
        }
        return source;
    }

    private static int SourceStep(int index)
    {
        if ((uint)index >= SamusBodyArtworkCatalog.FrameCount * 4) throw new ArgumentOutOfRangeException(nameof(index));
        int address = SamusBodyArtworkCatalog.FirstFrameOffset + index;
        int Source(SamusPoseId target, SamusPoseId basis, int frameCount, int half)
        {
            int offset = address - SamusBodyPoseDefinitions.DefaultFrameList((byte)target);
            return (uint)offset < frameCount * 4 && (half < 0 || offset % 4 / 2 == half)
                ? SamusBodyPoseDefinitions.DefaultFrameList((byte)basis) + offset - SamusBodyArtworkCatalog.FirstFrameOffset : index;
        }
        int Range(SamusPoseId target, SamusPoseId basis, int count, int targetPhase = 0, int basisPhase = 0, bool reverse = false, int half = -1)
        {
            int offset = address - SamusBodyPoseDefinitions.DefaultFrameList((byte)target) - targetPhase * 4;
            return (uint)offset < count * 4 && (half < 0 || offset % 4 / 2 == half)
                ? SamusBodyPoseDefinitions.DefaultFrameList((byte)basis) - SamusBodyArtworkCatalog.FirstFrameOffset +
                    (basisPhase + (reverse ? count - 1 - offset / 4 : offset / 4)) * 4 + offset % 4 : index;
        }
        foreach (SamusPoseId grapple in (ReadOnlySpan<SamusPoseId>)[ SamusPoseId.GrappleSwingRightPose, SamusPoseId.GrappleSwingLeftPose ])
        {
            int start = SamusBodyPoseDefinitions.DefaultFrameList((byte)grapple);
            int offset = address - start, phase = offset / 4;
            if ((uint)offset < 66 * 4 && offset % 4 < 2 && phase >= GrappleSwingFrameDefinitions.FrameCount)
                return start - SamusBodyArtworkCatalog.FirstFrameOffset +
                    (phase < GrappleSwingFrameDefinitions.FrameCount * 2 ? phase % GrappleSwingFrameDefinitions.FrameCount : GrappleSwingFrameDefinitions.FrameCount / 2) * 4 + offset % 4;
        }
        foreach (SamusPoseId pose in (ReadOnlySpan<SamusPoseId>)[ SamusPoseId.RunningAimUpRightPose, SamusPoseId.RunningAimUpLeftPose,
            SamusPoseId.RunningAimDiagonalUpRightPose, SamusPoseId.RunningAimDiagonalUpLeftPose,
            SamusPoseId.RunningAimDiagonalDownRightPose, SamusPoseId.RunningAimDiagonalDownLeftPose ])
        {
            int start = SamusBodyPoseDefinitions.DefaultFrameList((byte)pose), offset = address - start;
            if ((uint)offset < 40 && offset % 4 < 2)
            {
                int phase = offset / 4 % 5, basis = phase is 0 or 1 ? 0 : phase == 4 ? 2 : phase;
                return start - SamusBodyArtworkCatalog.FirstFrameOffset + basis * 4 + offset % 4;
            }
        }
        foreach (SamusPoseId pose in (ReadOnlySpan<SamusPoseId>)[ SamusPoseId.MoonwalkFacingLeftPose, SamusPoseId.MoonwalkFacingRightPose,
            SamusPoseId.MoonwalkAimUpLeftPose, SamusPoseId.MoonwalkAimUpRightPose,
            SamusPoseId.MoonwalkAimDownLeftPose, SamusPoseId.MoonwalkAimDownRightPose ])
        {
            int start = SamusBodyPoseDefinitions.DefaultFrameList((byte)pose), offset = address - start;
            if ((uint)offset >= 24) continue;
            if (offset % 4 < 2)
                return start - SamusBodyArtworkCatalog.FirstFrameOffset + (offset / 4 % 3 == 0 ? 0 : 1) * 4 + offset % 4;
            if (pose != SamusPoseId.MoonwalkFacingLeftPose && pose != SamusPoseId.MoonwalkFacingRightPose)
                return SamusBodyPoseDefinitions.DefaultFrameList((byte)SamusPoseId.MoonwalkFacingRightPose) - SamusBodyArtworkCatalog.FirstFrameOffset + offset;
        }
        foreach (bool left in (ReadOnlySpan<bool>)[ false, true ])
        {
            SamusPoseId crystal = left ? SamusPoseId.CrystalFlashLeftPose : SamusPoseId.CrystalFlashRightPose;
            int start = SamusBodyPoseDefinitions.DefaultFrameList((byte)crystal), offset = address - start;
            if ((uint)offset < 60)
            {
                int phase = offset / 4, component = offset % 4;
                int basis = phase switch { 5 or 10 or 11 => 4, 6 => 2, 9 => 7, 12 => 1, _ => phase };
                if (component >= 2 && (phase == 3 || phase is >= 6 and <= 9)) basis = 2;
                if (phase == 13) basis = component < 2 ? 1 : 0;
                if (basis != phase) return start - SamusBodyArtworkCatalog.FirstFrameOffset + basis * 4 + component;
            }
            SamusPoseId drained = left ? SamusPoseId.DrainedCrouchingLeftPose : SamusPoseId.DrainedCrouchingRightPose;
            start = SamusBodyPoseDefinitions.DefaultFrameList((byte)drained); offset = address - start;
            if ((uint)offset < (left ? 32 : 15) * 4)
            {
                int phase = offset / 4, component = offset % 4;
                int basis = left ? phase switch
                {
                    >= 3 and <= 6 => 2, 11 => 9, 13 or 17 or 18 or 24 or 25 or 27 or 28 or 30 or 31 => 12,
                    >= 19 and <= 23 => 14 + Math.Min(phase - 19, 23 - phase), 26 or 29 => 8, _ => phase
                } : phase switch { >= 4 and <= 7 => 3, 11 => 9, 13 => 12, _ => phase };
                if (!left && component >= 2 && phase is >= 9 and <= 11) basis = 8;
                if (left && component >= 2 && phase is >= 9 and <= 11) basis = 8;
                if (basis != phase) return start - SamusBodyArtworkCatalog.FirstFrameOffset + basis * 4 + component;
            }
            start = SamusBodyPoseDefinitions.DefaultFrameList((byte)(left ? SamusPoseId.DrainedStandingLeftPose : SamusPoseId.DrainedStandingRightPose));
            offset = address - start;
            if ((uint)offset < 16)
            {
                int phase = offset / 4, basis = offset % 4 >= 2 ? 0 : phase == 3 ? 1 : phase;
                return start - SamusBodyArtworkCatalog.FirstFrameOffset + basis * 4 + offset % 4;
            }
        }
        int appearance = SamusBodyPoseDefinitions.DefaultFrameList((byte)SamusPoseId.ForwardFacingPowerSuitPose);
        int suited = SamusBodyPoseDefinitions.DefaultFrameList((byte)SamusPoseId.ForwardFacingSuitedPose);
        int appearanceCount = (suited - appearance) / 4;
        int appearanceOffset = address - appearance;
        if ((uint)appearanceOffset < appearanceCount * 8)
        {
            bool suit = address >= suited;
            int start = suit ? suited : appearance;
            int offset = address - start, phase = offset / 4, component = offset % 4;
            if (component < 2 && suit && phase >= 1)
                return appearance - SamusBodyArtworkCatalog.FirstFrameOffset + offset;
            if (component >= 2 && phase >= 2)
                return start - SamusBodyArtworkCatalog.FirstFrameOffset + 8 + component;
            if (suit && phase == 1)
                return appearance - SamusBodyArtworkCatalog.FirstFrameOffset + offset;
            if (!suit && component < 2 && phase >= 3)
            {
                int basis = (phase & 1) != 0 ? 3 : phase < 80 ? 2 + (phase - 2) % 6 : phase >= 90 ? phase - 6 : phase;
                return appearance - SamusBodyArtworkCatalog.FirstFrameOffset + basis * 4 + component;
            }
        }
        int shared = Range(SamusPoseId.ShinesparkDiagonalRightPose, SamusPoseId.ShinesparkHorizontalRightPose, 1);
        if (shared != index) return shared;
        shared = Range(SamusPoseId.ShinesparkDiagonalLeftPose, SamusPoseId.ShinesparkHorizontalLeftPose, 1);
        if (shared != index) return shared;
        shared = Range(SamusPoseId.UnusedPose66, SamusPoseId.UnusedPose65, 8, 1, 1);
        if (shared != index) return shared;
        int wallOffset = address - SamusBodyPoseDefinitions.DefaultFrameList((byte)SamusPoseId.WallJumpRightPose);
        if ((uint)wallOffset < 47 * 4)
        {
            int phase = wallOffset / 4, component = wallOffset % 4;
            int basis = phase;
            if (component < 2 && phase is >= 13 and <= 20) basis = 13;
            if (phase >= 23)
            {
                int offset = phase - 23;
                basis = component >= 2 ? 23 + offset % 8 : offset / 3 % 2 == 1 ? 13 : 23 + offset / 3 * 3;
            }
            if (basis != phase) return index - (phase - basis) * 4;
        }
        foreach (SamusPoseId grapple in (ReadOnlySpan<SamusPoseId>)[ SamusPoseId.GrappleSwingRightPose, SamusPoseId.GrappleSwingLeftPose ])
        {
            int start = SamusBodyPoseDefinitions.DefaultFrameList((byte)grapple), offset = address - start;
            if ((uint)offset < 64 * 4 && offset % 4 >= 2)
            {
                int phase = offset / 4, withinQuarter = phase % 16;
                int basis = withinQuarter switch { 4 => 3, 6 => 5, 8 or 9 => 7, 11 => 10, 13 => 12, _ => withinQuarter };
                return start - SamusBodyArtworkCatalog.FirstFrameOffset + (phase - withinQuarter + basis) * 4 + offset % 4;
            }
        }
        int turn = Range(SamusPoseId.TurningLeftToRightPose, SamusPoseId.TurningRightToLeftPose, 3, reverse: true);
        if (turn != index) return turn;
        turn = Range(SamusPoseId.TurningLeftToRightAimUpPose, SamusPoseId.TurningRightToLeftAimUpPose, 3, reverse: true);
        if (turn != index) return turn;
        turn = Range(SamusPoseId.TurningLeftToRightAimDiagonalDownPose, SamusPoseId.TurningRightToLeftAimDiagonalDownPose, 3, reverse: true);
        if (turn != index) return turn;
        turn = Range(SamusPoseId.TurningLeftToRightJumpPose, SamusPoseId.TurningRightToLeftJumpPose, 3, reverse: true);
        if (turn != index) return turn;
        turn = Range(SamusPoseId.TurningLeftToRightJumpAimUpPose, SamusPoseId.TurningRightToLeftJumpAimUpPose, 3, reverse: true);
        if (turn != index) return turn;
        turn = Range(SamusPoseId.TurningLeftToRightJumpAimDownPose, SamusPoseId.TurningRightToLeftJumpAimDownPose, 3, reverse: true);
        if (turn != index) return turn;
        turn = Range(SamusPoseId.TurningRightToLeftAimUpPose, SamusPoseId.TurningRightToLeftPose, 3, half: 1);
        if (turn != index) return turn;
        turn = Range(SamusPoseId.TurningRightToLeftAimDiagonalDownPose, SamusPoseId.TurningRightToLeftPose, 3, half: 1);
        if (turn != index) return turn;
        turn = Range(SamusPoseId.TurningRightToLeftJumpPose, SamusPoseId.TurningRightToLeftPose, 3, half: 0);
        if (turn != index) return turn;
        turn = Range(SamusPoseId.TurningRightToLeftJumpAimUpPose, SamusPoseId.TurningRightToLeftAimUpPose, 3, half: 0);
        if (turn != index) return turn;
        turn = Range(SamusPoseId.TurningRightToLeftJumpAimDownPose, SamusPoseId.TurningRightToLeftAimDiagonalDownPose, 3, half: 0);
        if (turn != index) return turn;
        turn = Range(SamusPoseId.TurningRightToLeftJumpAimUpPose, SamusPoseId.TurningRightToLeftJumpPose, 3, half: 1);
        if (turn != index) return turn;
        turn = Range(SamusPoseId.TurningRightToLeftJumpAimDownPose, SamusPoseId.TurningRightToLeftJumpPose, 3, half: 1);
        if (turn != index) return turn;
        int alias = Range(SamusPoseId.WallJumpLeftPose, SamusPoseId.WallJumpRightPose, 45, 2, 2);
        if (alias != index) return alias;
        alias = Range(SamusPoseId.MorphBallGroundLeftPose, SamusPoseId.MorphBallGroundRightPose, 8, reverse: true);
        if (alias != index) return alias;
        alias = Range(SamusPoseId.MorphBallGroundLeftPose, SamusPoseId.MorphBallGroundRightPose, 2, 8, 8);
        if (alias != index) return alias;
        alias = Range(SamusPoseId.MorphBallMovingRightPose, SamusPoseId.MorphBallGroundRightPose, 10);
        if (alias != index) return alias;
        alias = Range(SamusPoseId.MorphBallMovingLeftPose, SamusPoseId.MorphBallGroundLeftPose, 10);
        if (alias != index) return alias;
        alias = Range(SamusPoseId.SpringBallGroundRightPose, SamusPoseId.MorphBallGroundRightPose, 10);
        if (alias != index) return alias;
        alias = Range(SamusPoseId.SpringBallGroundLeftPose, SamusPoseId.MorphBallGroundLeftPose, 10);
        if (alias != index) return alias;
        alias = Range(SamusPoseId.UnmorphingTransitionRightPose, SamusPoseId.MorphingTransitionRightPose, 2, reverse: true);
        if (alias != index) return alias;
        alias = Range(SamusPoseId.UnmorphingTransitionLeftPose, SamusPoseId.MorphingTransitionLeftPose, 2, reverse: true);
        if (alias != index) return alias;
        alias = Range(SamusPoseId.UnusedPoseDd, SamusPoseId.UnusedPoseDb, 3, reverse: true);
        if (alias != index) return alias;
        alias = Range(SamusPoseId.UnusedPoseDe, SamusPoseId.UnusedPoseDc, 3, reverse: true);
        if (alias != index) return alias;
        foreach (bool left in (ReadOnlySpan<bool>)[ false, true ])
        {
            SamusPoseId wall = left ? SamusPoseId.WallJumpLeftPose : SamusPoseId.WallJumpRightPose;
            SamusPoseId spin = left ? SamusPoseId.SpinJumpLeftPose : SamusPoseId.SpinJumpRightPose;
            SamusPoseId space = left ? SamusPoseId.SpaceJumpLeftPose : SamusPoseId.SpaceJumpRightPose;
            SamusPoseId screw = left ? SamusPoseId.ScrewAttackLeftPose : SamusPoseId.ScrewAttackRightPose;
            alias = Range(spin, wall, 1, 0, 1);
            if (alias != index) return alias;
            alias = Range(spin, wall, 10, 1, 3);
            if (alias != index) return alias;
            alias = Range(space, wall, 1, 0, 1);
            if (alias != index) return alias;
            alias = Range(space, wall, 10, 1, 13);
            if (alias != index) return alias;
            alias = Range(space, spin, 1, 11, 11);
            if (alias != index) return alias;
            alias = Range(screw, wall, 1, 0, 1);
            if (alias != index) return alias;
            alias = Range(screw, wall, 24, 1, 23);
            if (alias != index) return alias;
            alias = Range(screw, wall, 2, 25, 21);
            if (alias != index) return alias;
            alias = Range(screw, spin, 1, 27, 11);
            if (alias != index) return alias;
        }
        // Native breathing repeats earlier pictures. Lower phase seven repeats
        // its first raised-leg composition while the upper keeps a distinct one.
        foreach (bool left in (ReadOnlySpan<bool>)[ false, true ])
        {
            int start = SamusBodyPoseDefinitions.DefaultFrameList((byte)(left
                ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose));
            int offset = address - start;
            if ((uint)offset < NormalFrameCount * 4)
            {
                int phase = offset / 4;
                int basis = phase switch { 3 or 6 or 8 => 1, 5 => 0, 7 when offset % 4 >= 2 => 2, _ => phase };
                return start - SamusBodyArtworkCatalog.FirstFrameOffset + basis * 4 + offset % 4;
            }
            int normal = SamusBodyPoseDefinitions.DefaultFrameList((byte)(left
                ? SamusPoseId.NormalLandingLeftPose : SamusPoseId.NormalLandingRightPose));
            start = SamusBodyPoseDefinitions.DefaultFrameList((byte)(left
                ? SamusPoseId.SpinLandingLeftPose : SamusPoseId.SpinLandingRightPose));
            offset = address - start;
            if ((uint)offset < 3 * 4 && offset % 4 < 2)
                return normal - SamusBodyArtworkCatalog.FirstFrameOffset + (1 - offset / 4 % 2) * 4 + offset % 4;
            foreach (SamusPoseId pose in left
                ? new[] { SamusPoseId.LandingAimUpLeftPose, SamusPoseId.LandingAimDiagonalUpLeftPose, SamusPoseId.LandingAimDiagonalDownLeftPose, SamusPoseId.FiringLandingLeftPose }
                : new[] { SamusPoseId.LandingAimUpRightPose, SamusPoseId.LandingAimDiagonalUpRightPose, SamusPoseId.LandingAimDiagonalDownRightPose, SamusPoseId.FiringLandingRightPose })
            {
                start = SamusBodyPoseDefinitions.DefaultFrameList((byte)pose);
                offset = address - start;
                if ((uint)offset < 2 * 4 && offset % 4 >= 2)
                    return normal - SamusBodyArtworkCatalog.FirstFrameOffset + offset;
            }
            foreach (SamusPoseId pose in left
                ? new[] { SamusPoseId.XrayingStandingLeftPose, SamusPoseId.XrayingCrouchingLeftPose }
                : new[] { SamusPoseId.XrayingStandingRightPose, SamusPoseId.XrayingCrouchingRightPose })
            {
                start = SamusBodyPoseDefinitions.DefaultFrameList((byte)pose);
                offset = address - start;
                if ((uint)offset < XrayFrameCount * 4 && offset % 4 >= 2)
                    return start - SamusBodyArtworkCatalog.FirstFrameOffset + offset % 4;
            }
        }
        int source = Source(SamusPoseId.XrayingCrouchingRightPose, SamusPoseId.XrayingStandingRightPose, XrayFrameCount, 0);
        if (source != index) return source;
        source = Source(SamusPoseId.XrayingCrouchingLeftPose, SamusPoseId.XrayingStandingLeftPose, XrayFrameCount, 0);
        if (source != index) return source;
        source = Source(SamusPoseId.MovingLeftNormalPose, SamusPoseId.MovingRightNormalPose, MovingFrameCount, 1);
        if (source != index) return source;
        source = Source(SamusPoseId.MovingRightGunExtendedPose, SamusPoseId.MovingRightNormalPose, MovingFrameCount, 1);
        if (source != index) return source;
        source = Source(SamusPoseId.MovingLeftGunExtendedPose, SamusPoseId.MovingRightNormalPose, MovingFrameCount, 1);
        if (source != index) return source;
        source = Source(SamusPoseId.RunningAimUpRightPose, SamusPoseId.MovingRightNormalPose, MovingFrameCount, 1);
        if (source != index) return source;
        source = Source(SamusPoseId.RunningAimUpLeftPose, SamusPoseId.MovingRightNormalPose, MovingFrameCount, 1);
        if (source != index) return source;
        source = Source(SamusPoseId.RunningAimDiagonalUpRightPose, SamusPoseId.MovingRightNormalPose, MovingFrameCount, 1);
        if (source != index) return source;
        source = Source(SamusPoseId.RunningAimDiagonalUpLeftPose, SamusPoseId.MovingRightNormalPose, MovingFrameCount, 1);
        if (source != index) return source;
        source = Source(SamusPoseId.RunningAimDiagonalDownRightPose, SamusPoseId.MovingRightNormalPose, MovingFrameCount, 1);
        if (source != index) return source;
        source = Source(SamusPoseId.RunningAimDiagonalDownLeftPose, SamusPoseId.MovingRightNormalPose, MovingFrameCount, 1);
        if (source != index) return source;
        source = Source(SamusPoseId.CrouchingRightPose, SamusPoseId.FacingRightNormalPose, NormalFrameCount, 0);
        if (source != index) return source;
        source = Source(SamusPoseId.CrouchingLeftPose, SamusPoseId.FacingLeftNormalPose, NormalFrameCount, 0);
        if (source != index) return source;
        source = Source(SamusPoseId.NormalJumpForwardRightPose, SamusPoseId.NormalJumpGunExtendedRightPose, JumpFrameCount, -1);
        if (source != index) return source;
        source = Source(SamusPoseId.NormalJumpForwardLeftPose, SamusPoseId.NormalJumpGunExtendedLeftPose, JumpFrameCount, -1);
        if (source != index) return source;
        source = Source(SamusPoseId.FallingAimDownRightPose, SamusPoseId.NormalJumpAimDownRightPose, JumpFrameCount, -1);
        return source != index ? source : Source(SamusPoseId.FallingAimDownLeftPose, SamusPoseId.NormalJumpAimDownLeftPose, JumpFrameCount, -1);
    }
}
