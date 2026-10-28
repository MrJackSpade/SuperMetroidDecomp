using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.Core.Samus.Artwork.SamusBodyFrameDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Which earlier body-frame component an authored pose frame shares its tiles with.</summary>
internal static class SamusBodyFrameSharing
{
    /// <summary>$92:E050/E158: upper grapple characters follow the same three angular allocation runs as their OAM compositions. Set identities select the native glyph allocation; positions advance or reverse within it.</summary>
    internal static bool TryComponent(int index, out int value)
    {
        int entry = index;
        bool ClearSlot(SamusPoseId pose, int phase) => (uint)(entry - ListStart(pose) - phase * 4) < 4;
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
            int offset = entry - ListStart((left
                ? SamusPoseId.GrappleSwingLeftPose : SamusPoseId.GrappleSwingRightPose));
            if ((uint)offset >= GrappleSwingFrameDefinitions.FrameCount * 4 || offset % 4 >= 2) continue;
            int phase = offset / 4;
            value = (byte)(offset % 4 == 0
                ? left ? phase is < 8 or >= 24 ? GrappleLeftOuterSet : GrappleLeftMiddleSet : phase is <= 8 or > 24 ? GrappleRightOuterSet : GrappleRightMiddleSet
                : left ? phase < 8 ? phase + 8 : phase < 24 ? phase - 8 : phase - 24
                    : phase <= 8 ? 8 - phase : phase <= 24 ? 24 - phase : 40 - phase);
            return true;
        }
        value = 0;
        return false;
    }

    /// <summary>Precomputed roots of the native body-frame alias graph, avoiding pose-table walks during rendering.</summary>
    private static readonly int[] sourceComponents = ResolveSourceComponents();

    /// <summary>Maps a component to the earliest native body-frame entry containing its shared character selection.</summary>
    internal static int SourceComponent(int index)
    {
        if ((uint)index >= (uint)sourceComponents.Length) throw new ArgumentOutOfRangeException(nameof(index));
        return sourceComponents[index];
    }

    /// <summary>Follows every alias to its earlier source and rejects any forward reference in the native selection graph.</summary>
    private static int[] ResolveSourceComponents()
    {
        var components = new int[InstalledSamusBodyArtwork.FrameCount * 4];
        for (int component = 0; component < components.Length; component++)
        {
            int index = component;
            int source = SourceStep(index);
            while (source != index)
            {
                if (source >= index) throw new InvalidOperationException("Body-frame aliases must reference an earlier native component.");
                index = source;
                source = SourceStep(index);
            }
            components[component] = source;
        }
        return components;
    }

    /// <summary>
    /// Resolves one native component-sharing edge. Crouching X-ray frames share standing upper
    /// selections, aiming movement shares the lower gait, and selected jump and fall poses
    /// reuse their matching frame lists while retaining independent components elsewhere.
    /// </summary>
    private static int SourceStep(int index)
    {
        if ((uint)index >= InstalledSamusBodyArtwork.FrameCount * 4) throw new ArgumentOutOfRangeException(nameof(index));
        int entry = index;
        int Source(SamusPoseId target, SamusPoseId basis, int frameCount, int half)
        {
            int offset = entry - ListStart(target);
            return (uint)offset < frameCount * 4 && (half < 0 || offset % 4 / 2 == half)
                ? ListStart(basis) + offset : index;
        }
        int Range(SamusPoseId target, SamusPoseId basis, int count, int targetPhase = 0, int basisPhase = 0, int half = -1) =>
            Frames(target, basis, count, targetPhase, basisPhase, half, frame => frame);
        int ReversedRange(SamusPoseId target, SamusPoseId basis, int count) =>
            Frames(target, basis, count, 0, 0, -1, frame => count - 1 - frame);
        int Frames(SamusPoseId target, SamusPoseId basis, int count, int targetPhase, int basisPhase, int half, Func<int, int> basisFrame)
        {
            int offset = entry - ListStart(target) - targetPhase * 4;
            return (uint)offset < count * 4 && (half < 0 || offset % 4 / 2 == half)
                ? ListStart(basis) +
                    (basisPhase + basisFrame(offset / 4)) * 4 + offset % 4 : index;
        }
        foreach (SamusPoseId grapple in (ReadOnlySpan<SamusPoseId>)[ SamusPoseId.GrappleSwingRightPose, SamusPoseId.GrappleSwingLeftPose ])
        {
            int start = ListStart(grapple);
            int offset = entry - start, phase = offset / 4;
            if ((uint)offset < 66 * 4 && offset % 4 < 2 && phase >= GrappleSwingFrameDefinitions.FrameCount)
                return start +
                    (phase < GrappleSwingFrameDefinitions.FrameCount * 2 ? phase % GrappleSwingFrameDefinitions.FrameCount : GrappleSwingFrameDefinitions.FrameCount / 2) * 4 + offset % 4;
        }
        foreach (SamusPoseId pose in (ReadOnlySpan<SamusPoseId>)[ SamusPoseId.RunningAimUpRightPose, SamusPoseId.RunningAimUpLeftPose,
            SamusPoseId.RunningAimDiagonalUpRightPose, SamusPoseId.RunningAimDiagonalUpLeftPose,
            SamusPoseId.RunningAimDiagonalDownRightPose, SamusPoseId.RunningAimDiagonalDownLeftPose ])
        {
            int start = ListStart(pose), offset = entry - start;
            if ((uint)offset < 40 && offset % 4 < 2)
            {
                int phase = offset / 4 % 5, basis = phase is 0 or 1 ? 0 : phase == 4 ? 2 : phase;
                return start + basis * 4 + offset % 4;
            }
        }
        foreach (SamusPoseId pose in (ReadOnlySpan<SamusPoseId>)[ SamusPoseId.MoonwalkFacingLeftPose, SamusPoseId.MoonwalkFacingRightPose,
            SamusPoseId.MoonwalkAimUpLeftPose, SamusPoseId.MoonwalkAimUpRightPose,
            SamusPoseId.MoonwalkAimDownLeftPose, SamusPoseId.MoonwalkAimDownRightPose ])
        {
            int start = ListStart(pose), offset = entry - start;
            if ((uint)offset >= 24) continue;
            if (offset % 4 < 2)
                return start + (offset / 4 % 3 == 0 ? 0 : 1) * 4 + offset % 4;
            if (pose is not SamusPoseId.MoonwalkFacingLeftPose and not SamusPoseId.MoonwalkFacingRightPose)
                return ListStart(SamusPoseId.MoonwalkFacingRightPose) + offset;
        }
        foreach (bool left in (ReadOnlySpan<bool>)[ false, true ])
        {
            SamusPoseId crystal = left ? SamusPoseId.CrystalFlashLeftPose : SamusPoseId.CrystalFlashRightPose;
            int start = ListStart(crystal), offset = entry - start;
            if ((uint)offset < 60)
            {
                int phase = offset / 4, component = offset % 4;
                int basis = phase switch { 5 or 10 or 11 => 4, 6 => 2, 9 => 7, 12 => 1, _ => phase };
                if (component >= 2 && (phase == 3 || phase is >= 6 and <= 9)) basis = 2;
                if (phase == 13) basis = component < 2 ? 1 : 0;
                if (basis != phase) return start + basis * 4 + component;
            }
            SamusPoseId drained = left ? SamusPoseId.DrainedCrouchingLeftPose : SamusPoseId.DrainedCrouchingRightPose;
            start = ListStart(drained); offset = entry - start;
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
                if (basis != phase) return start + basis * 4 + component;
            }
            start = ListStart((left ? SamusPoseId.DrainedStandingLeftPose : SamusPoseId.DrainedStandingRightPose));
            offset = entry - start;
            if ((uint)offset < 16)
            {
                int phase = offset / 4, basis = offset % 4 >= 2 ? 0 : phase == 3 ? 1 : phase;
                return start + basis * 4 + offset % 4;
            }
        }
        int appearance = ListStart(SamusPoseId.ForwardFacingPowerSuitPose);
        int suited = ListStart(SamusPoseId.ForwardFacingSuitedPose);
        int appearanceCount = (suited - appearance) / 4;
        int appearanceOffset = entry - appearance;
        if ((uint)appearanceOffset < appearanceCount * 8)
        {
            bool suit = entry >= suited;
            int start = suit ? suited : appearance;
            int offset = entry - start, phase = offset / 4, component = offset % 4;
            if (component < 2 && suit && phase >= 1)
                return appearance + offset;
            if (component >= 2 && phase >= 2)
                return start + 8 + component;
            if (suit && phase == 1)
                return appearance + offset;
            if (!suit && component < 2 && phase >= 3)
            {
                int basis = (phase & 1) != 0 ? 3 : phase < 80 ? 2 + (phase - 2) % 6 : phase >= 90 ? phase - 6 : phase;
                return appearance + basis * 4 + component;
            }
        }
        int shared = Range(SamusPoseId.ShinesparkDiagonalRightPose, SamusPoseId.ShinesparkHorizontalRightPose, 1);
        if (shared != index) return shared;
        shared = Range(SamusPoseId.ShinesparkDiagonalLeftPose, SamusPoseId.ShinesparkHorizontalLeftPose, 1);
        if (shared != index) return shared;
        shared = Range(SamusPoseId.UnusedPose66, SamusPoseId.UnusedPose65, 8, 1, 1);
        if (shared != index) return shared;
        int wallOffset = entry - ListStart(SamusPoseId.WallJumpRightPose);
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
            int start = ListStart(grapple), offset = entry - start;
            if ((uint)offset < 64 * 4 && offset % 4 >= 2)
            {
                int phase = offset / 4, withinQuarter = phase % 16;
                int basis = withinQuarter switch { 4 => 3, 6 => 5, 8 or 9 => 7, 11 => 10, 13 => 12, _ => withinQuarter };
                return start + (phase - withinQuarter + basis) * 4 + offset % 4;
            }
        }
        int turn = ReversedRange(SamusPoseId.TurningLeftToRightPose, SamusPoseId.TurningRightToLeftPose, 3);
        if (turn != index) return turn;
        turn = ReversedRange(SamusPoseId.TurningLeftToRightAimUpPose, SamusPoseId.TurningRightToLeftAimUpPose, 3);
        if (turn != index) return turn;
        turn = ReversedRange(SamusPoseId.TurningLeftToRightAimDiagonalDownPose, SamusPoseId.TurningRightToLeftAimDiagonalDownPose, 3);
        if (turn != index) return turn;
        turn = ReversedRange(SamusPoseId.TurningLeftToRightJumpPose, SamusPoseId.TurningRightToLeftJumpPose, 3);
        if (turn != index) return turn;
        turn = ReversedRange(SamusPoseId.TurningLeftToRightJumpAimUpPose, SamusPoseId.TurningRightToLeftJumpAimUpPose, 3);
        if (turn != index) return turn;
        turn = ReversedRange(SamusPoseId.TurningLeftToRightJumpAimDownPose, SamusPoseId.TurningRightToLeftJumpAimDownPose, 3);
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
        alias = ReversedRange(SamusPoseId.MorphBallGroundLeftPose, SamusPoseId.MorphBallGroundRightPose, 8);
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
        alias = ReversedRange(SamusPoseId.UnmorphingTransitionRightPose, SamusPoseId.MorphingTransitionRightPose, 2);
        if (alias != index) return alias;
        alias = ReversedRange(SamusPoseId.UnmorphingTransitionLeftPose, SamusPoseId.MorphingTransitionLeftPose, 2);
        if (alias != index) return alias;
        alias = ReversedRange(SamusPoseId.UnusedPoseDd, SamusPoseId.UnusedPoseDb, 3);
        if (alias != index) return alias;
        alias = ReversedRange(SamusPoseId.UnusedPoseDe, SamusPoseId.UnusedPoseDc, 3);
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
            int start = ListStart((left
                ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose));
            int offset = entry - start;
            if ((uint)offset < NormalFrameCount * 4)
            {
                int phase = offset / 4;
                int basis = phase switch { 3 or 6 or 8 => 1, 5 => 0, 7 when offset % 4 >= 2 => 2, _ => phase };
                return start + basis * 4 + offset % 4;
            }
            int normal = ListStart((left
                ? SamusPoseId.NormalLandingLeftPose : SamusPoseId.NormalLandingRightPose));
            start = ListStart((left
                ? SamusPoseId.SpinLandingLeftPose : SamusPoseId.SpinLandingRightPose));
            offset = entry - start;
            if ((uint)offset < 3 * 4 && offset % 4 < 2)
                return normal + (1 - offset / 4 % 2) * 4 + offset % 4;
            foreach (SamusPoseId pose in left
                ? new[] { SamusPoseId.LandingAimUpLeftPose, SamusPoseId.LandingAimDiagonalUpLeftPose, SamusPoseId.LandingAimDiagonalDownLeftPose, SamusPoseId.FiringLandingLeftPose }
                : new[] { SamusPoseId.LandingAimUpRightPose, SamusPoseId.LandingAimDiagonalUpRightPose, SamusPoseId.LandingAimDiagonalDownRightPose, SamusPoseId.FiringLandingRightPose })
            {
                start = ListStart(pose);
                offset = entry - start;
                if ((uint)offset < 2 * 4 && offset % 4 >= 2)
                    return normal + offset;
            }
            foreach (SamusPoseId pose in left
                ? new[] { SamusPoseId.XrayingStandingLeftPose, SamusPoseId.XrayingCrouchingLeftPose }
                : new[] { SamusPoseId.XrayingStandingRightPose, SamusPoseId.XrayingCrouchingRightPose })
            {
                start = ListStart(pose);
                offset = entry - start;
                if ((uint)offset < XrayFrameCount * 4 && offset % 4 >= 2)
                    return start + offset % 4;
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
