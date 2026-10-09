using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Calculated native pose-half sharing and allocation geometry.</summary>
/// <remarks>The exact remaining 813 drawing choices, one observable zero-WRAM identity and named sequence memberships are selected composition content. They cannot be derived from physical pose quantities without restating the chosen pictures. Parts, pixels and timing have separate owners; see Stream 1 Batch 103 for exact native membership and consumer evidence.</remarks>
internal static class SamusSpritemapFrameDefinitions
{
    /// <summary>$92:8091/8151/81D1/8291: appearance lists contain96 indexed phases, including the initial body and mutable-memory slot.</summary>
    private static int AppearancePhases => (SamusAnimationDelayDefinitions.DelayStreamsEndExclusive & ushort.MaxValue) - SamusAnimationDelayDefinitions.PointerForPose((byte)SamusPoseId.ForwardFacingPowerSuitPose) - 2;
    /// <summary>$92:8095..812F: the initial electricity sequence alternates three distinct discharges with its shared empty composition.</summary>
    private const int AppearanceDischargeEnd = 80;
    /// <summary>$92:86F3/8777: upper grapple composition repeats after one32-position turn; the two final entries select its center orientation.</summary>
    private const int GrappleTurnPhases = GrappleSwingFrameDefinitions.FrameCount;
    /// <summary>$92:8EAD/8EC1: preserved right/left morph sequences; their first eight spin pictures reverse, followed by memory and bounce slots.</summary>
    private const int FirstRightMorph = 0x710, FirstLeftMorph = 0x71a;

    /// <summary>$92:D7D3/$92:D858, adjacent bubble and diving OAM allocations in native phase order.</summary>
    private const ushort BubbleAllocation = 0xd7d3, DivingAllocation = 0xd858;
    /// <summary>$92:C8B7/$92:C580, facing-specific nine-phase suit-explosion OAM allocations.</summary>
    private const ushort DeathRightAllocation = 0xc8b7, DeathLeftAllocation = 0xc580;
    /// <summary>$92:A8D4/A46C/A981: three packed right-facing grapple angular allocation runs, selected in reverse record order.</summary>
    private const ushort GrappleRightFirstQuadrant = 0xa8d4, GrappleRightHalfTurn = 0xa46c, GrappleRightLastQuadrant = 0xa981;
    /// <summary>$92:AB8B/A5B0/AA18: three packed left-facing grapple angular runs, selected in forward record order.</summary>
    private const ushort GrappleLeftFirstQuadrant = 0xab8b, GrappleLeftHalfTurn = 0xa5b0, GrappleLeftLastQuadrant = 0xaa18;
    /// <summary>$92:8399/83AB and90C5/90D7 identify bubbles, diving splash and the two death-OAM sequence domains.</summary>
    private const int Bubbles = 0x186, Diving = 0x18f, DeathRight = 0x81c, DeathLeft = 0x825;

    /// <summary>Resolves an allocated pose-half slot to its native OAM pointer or identifies a control/omitted slot.</summary>
    /// <param name="index">Index in the combined upper- and lower-half pose allocation.</param>
    /// <param name="definitions">Compiled spritemap records used to advance through variable-length allocation runs.</param>
    /// <param name="pointer">Receives the resolved pointer, or zero for a control or intentionally omitted half.</param>
    /// <returns><see langword="true"/> when the slot is a control/omitted entry or its native pointer can be resolved.</returns>
    internal static bool TryPointer(int index, IReadOnlyDictionary<ushort, SamusSpritemapDefinition> definitions, out ushort pointer)
    {
        if (IsControlOrOmittedHalf(index)) { pointer = 0; return true; }
        int phase = index - Bubbles;
        if ((uint)phase < Diving - Bubbles) return Advance(BubbleAllocation, phase, definitions, out pointer);
        phase = index - Diving;
        if ((uint)phase < 9) return Advance(DivingAllocation, phase, definitions, out pointer);
        phase = index - DeathRight;
        if ((uint)phase < DeathLeft - DeathRight) return Advance(DeathRightAllocation, phase, definitions, out pointer);
        phase = index - DeathLeft;
        if ((uint)phase < SamusDeathExplosionTimingDefinitions.RecordCount) return Advance(DeathLeftAllocation, phase, definitions, out pointer);
        phase = index - SamusSpritemapPoseDefinitions.TopBase((byte)SamusPoseId.GrappleSwingRightPose);
        if ((uint)phase < GrappleTurnPhases)
            return phase <= 8 ? Advance(GrappleRightFirstQuadrant, 8 - phase, definitions, out pointer)
                : phase <= 24 ? Advance(GrappleRightHalfTurn, 24 - phase, definitions, out pointer)
                : Advance(GrappleRightLastQuadrant, 31 - phase, definitions, out pointer);
        phase = index - SamusSpritemapPoseDefinitions.TopBase((byte)SamusPoseId.GrappleSwingLeftPose);
        if ((uint)phase < GrappleTurnPhases)
            return phase < 8 ? Advance(GrappleLeftFirstQuadrant, phase, definitions, out pointer)
                : phase < 24 ? Advance(GrappleLeftHalfTurn, phase - 8, definitions, out pointer)
                : Advance(GrappleLeftLastQuadrant, phase - 24, definitions, out pointer);
        pointer = 0;
        return false;
    }

    /// <summary>
    /// Native animation commands do not consume a drawn-frame composition. These
    /// exact allocated holes retain the bank-local zero identity (mutable WRAM),
    /// not an invented empty OAM record. Suppressed halves mirror the existing
    /// $90:8686/86EE/870C..874B/8762..8790 render policy. No wider slot is inferred.
    /// </summary>
    private static bool IsControlOrOmittedHalf(int index)
    {
        int Phase(bool upper, SamusPoseId pose) => index - (upper
            ? SamusSpritemapPoseDefinitions.TopBase((byte)pose) : SamusSpritemapPoseDefinitions.BottomBase((byte)pose));
        bool Command(bool upper, SamusPoseId pose, int extent, int firstPhase = 0)
        {
            int phase = Phase(upper, pose);
            if ((uint)phase >= extent || phase < firstPhase) return false;
            int pointer = SamusAnimationDelayDefinitions.PointerForPose((byte)pose);
            for (int cursor = 0; cursor <= phase;)
            {
                int address = SamusMovementRomData.Banks.Pose | (pointer + cursor);
                if (address >= SamusAnimationDelayDefinitions.DelayStreamsEndExclusive) return false;
                byte value = SamusAnimationDelayDefinitions.ReadCompiledByte(address);
                int size = value switch { 0xf8 or 0xfd or 0xfe => 2, 0xf9 => 7, 0xfa => 3, 0xfc => 5, _ => 1 };
                if (phase < cursor + size) return value is 0xf6 or 0xfb or 0xfd or 0xfe or 0xff;
                cursor += size;
            }
            return false;
        }
        foreach (bool upper in (ReadOnlySpan<bool>)[ true, false ])
        {
            if (Command(upper, SamusPoseId.ForwardFacingPowerSuitPose, AppearancePhases) ||
                Command(upper, SamusPoseId.ForwardFacingSuitedPose, AppearancePhases) ||
                Command(upper, SamusPoseId.FacingRightNormalPose, 9) ||
                Command(upper, SamusPoseId.FacingLeftNormalPose, 9) ||
                Command(upper, SamusPoseId.CrouchingRightPose, 9) ||
                Command(upper, SamusPoseId.CrouchingLeftPose, 9) ||
                Command(upper, SamusPoseId.FallingRightPose, 7) ||
                Command(upper, SamusPoseId.FallingLeftPose, 7) ||
                Command(upper, SamusPoseId.FallingGunExtendedRightPose, 7) ||
                Command(upper, SamusPoseId.FallingGunExtendedLeftPose, 7) ||
                Command(upper, SamusPoseId.WallJumpRightPose, 47) ||
                Command(upper, SamusPoseId.WallJumpLeftPose, 47) ||
                Command(upper, SamusPoseId.CrystalFlashRightPose, 15) ||
                Command(upper, SamusPoseId.CrystalFlashLeftPose, 15) ||
                // The earlier fall-controller loop retains its standing drawing
                // even in command slots. Only the subsequent rocking blocks use
                // the null-control convention; do not generalize to all bytecode.
                Command(upper, SamusPoseId.DrainedCrouchingRightPose, 15, 8) ||
                Command(upper, SamusPoseId.DrainedCrouchingLeftPose, 32, 7) ||
                Command(upper, SamusPoseId.DrainedStandingRightPose, 6) ||
                Command(upper, SamusPoseId.DrainedStandingLeftPose, 6)) return true;
        }
        if (Command(false, SamusPoseId.StandingTransitionLeftPose, 3)) return true;
        int morphPhase = index - FirstRightMorph;
        if ((uint)morphPhase < 8 + 1 && Command(true, SamusPoseId.MorphBallGroundRightPose, 9)) return true;
        foreach (bool left in (ReadOnlySpan<bool>)[ false, true ])
        {
            int phase = Phase(false, left ? SamusPoseId.DamageBoostLeftPose : SamusPoseId.DamageBoostRightPose);
            if (phase is >= 2 and < 9) return true;
            phase = Phase(false, left ? SamusPoseId.WallJumpLeftPose : SamusPoseId.WallJumpRightPose);
            if (phase is >= 3 and < 13) return true;
            phase = Phase(false, left ? SamusPoseId.DeathSequenceLeftPose : SamusPoseId.DeathSequenceRightPose);
            if ((uint)phase < 3) return true;
            phase = Phase(false, left ? SamusPoseId.DrainedCrouchingLeftPose : SamusPoseId.DrainedCrouchingRightPose);
            if ((uint)phase < 2) return true;
        }
        int entering = Phase(false, SamusPoseId.UnusedPoseDb);
        if (entering is 1 or 2) return true;
        int leaving = Phase(false, SamusPoseId.UnusedPoseDd);
        return leaving is 0 or 1 || Phase(false, SamusPoseId.UnusedPoseDe) is 0 or 1;
    }
    /// <summary>Advances by ordinal through consecutive native spritemap records using their compiled part counts.</summary>
    /// <param name="start">Pointer to the first record in the allocation run.</param>
    /// <param name="ordinal">Zero-based record position to resolve from the run start.</param>
    /// <param name="definitions">Definitions providing each preceding record's variable byte length.</param>
    /// <param name="pointer">Receives the resulting pointer or the first pointer whose definition is unavailable.</param>
    /// <returns><see langword="true"/> when every record needed to reach the ordinal is present.</returns>
    private static bool Advance(ushort start, int ordinal, IReadOnlyDictionary<ushort, SamusSpritemapDefinition> definitions, out ushort pointer)
    {
        pointer = start;
        for (int part = 0; part < ordinal; part++)
        {
            if (!definitions.TryGetValue(pointer, out var definition)) return false;
            pointer = unchecked((ushort)(pointer + sizeof(ushort) + definition.Parts.Length * 5));
        }
        return true;
    }
    /// <summary>Maps a pose-half allocation slot to the selected artwork source, accounting for shared and reversed sequences.</summary>
    /// <param name="index">Index in the complete Samus spritemap pointer allocation.</param>
    /// <returns>The allocation index whose artwork supplies this slot, or the same index when it has no source alias.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the allocated spritemap pointers.</exception>
    internal static int SourceIndex(int index)
    {
        if ((uint)index >= SamusSpritemapArtworkCatalog.PointerCount) throw new ArgumentOutOfRangeException(nameof(index));
        int Top(SamusPoseId pose) => SamusSpritemapPoseDefinitions.TopBase((byte)pose);
        int Bottom(SamusPoseId pose) => SamusSpritemapPoseDefinitions.BottomBase((byte)pose);
        int Alias(bool upper, SamusPoseId target, SamusPoseId source, int count)
        {
            int targetBase = upper ? Top(target) : Bottom(target);
            int phase = index - targetBase;
            return (uint)phase < count ? (upper ? Top(source) : Bottom(source)) + phase : index;
        }
        int Reverse(bool upper, SamusPoseId target, SamusPoseId source, int count)
        {
            int targetBase = upper ? Top(target) : Bottom(target);
            int phase = index - targetBase;
            return (uint)phase < count ? (upper ? Top(source) : Bottom(source)) + count - 1 - phase : index;
        }
        int phase = index - Top(SamusPoseId.ForwardFacingSuitedPose);
        if (phase > 0 && phase < AppearancePhases) return Top(SamusPoseId.ForwardFacingPowerSuitPose) + phase;
        phase = index - Top(SamusPoseId.ForwardFacingPowerSuitPose);
        if (phase >= 2 && phase < AppearancePhases)
        {
            if ((phase & 1) != 0) return Top(SamusPoseId.ForwardFacingPowerSuitPose) + 3;
            if (phase < AppearanceDischargeEnd) return Top(SamusPoseId.ForwardFacingPowerSuitPose) + 2 + (phase - 2) % 6;
            if (phase >= 90) return index - 6;
        }
        foreach (SamusPoseId pose in (ReadOnlySpan<SamusPoseId>)[ SamusPoseId.ForwardFacingPowerSuitPose, SamusPoseId.ForwardFacingSuitedPose ])
        {
            phase = index - Bottom(pose);
            if (phase >= 2 && phase < AppearancePhases) return Bottom(pose) + 2;
            if (phase == 1) return Top(SamusPoseId.ForwardFacingPowerSuitPose) + 1;
        }
        foreach (SamusPoseId pose in (ReadOnlySpan<SamusPoseId>)[ SamusPoseId.MoonwalkFacingLeftPose, SamusPoseId.MoonwalkFacingRightPose,
            SamusPoseId.MoonwalkAimUpLeftPose, SamusPoseId.MoonwalkAimUpRightPose,
            SamusPoseId.MoonwalkAimDownLeftPose, SamusPoseId.MoonwalkAimDownRightPose ])
        {
            phase = index - Top(pose);
            if ((uint)phase < 6) return Top(pose) + (phase % 3 == 0 ? 0 : 1);
        }
        foreach (SamusPoseId pose in (ReadOnlySpan<SamusPoseId>)[ SamusPoseId.RunningAimUpRightPose, SamusPoseId.RunningAimUpLeftPose,
            SamusPoseId.RunningAimDiagonalUpRightPose, SamusPoseId.RunningAimDiagonalUpLeftPose,
            SamusPoseId.RunningAimDiagonalDownRightPose, SamusPoseId.RunningAimDiagonalDownLeftPose,
            SamusPoseId.UnusedPose45, SamusPoseId.UnusedPose46 ])
        {
            phase = index - Top(pose);
            if ((uint)phase < 10)
            {
                int gait = phase % 5;
                return Top(pose) + (gait <= 1 ? 0 : gait == 4 ? 2 : gait);
            }
        }
        foreach (SamusPoseId pose in (ReadOnlySpan<SamusPoseId>)[ SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose ])
        {
            phase = index - Top(pose);
            if ((uint)phase < 9 && phase is 3 or 5 or 6 or 8) return Top(pose) + (phase == 5 ? 0 : 1);
        }
        foreach (SamusPoseId pose in (ReadOnlySpan<SamusPoseId>)[ SamusPoseId.GrappleSwingRightPose, SamusPoseId.GrappleSwingLeftPose ])
        {
            phase = index - Top(pose);
            if ((uint)phase < GrappleTurnPhases * 2 + 2)
                return Top(pose) + (phase < GrappleTurnPhases * 2 ? phase % GrappleTurnPhases : GrappleTurnPhases / 2);
        }
        int morphOffset = index - FirstRightMorph;
        int morphLength = FirstLeftMorph - FirstRightMorph;
        if ((uint)morphOffset < morphLength * 6)
        {
            int rotation = morphOffset % morphLength;
            bool reverse = ((morphOffset / morphLength) & 1) != 0;
            return FirstRightMorph + (reverse && rotation < morphLength - 2 ? morphLength - 3 - rotation : rotation);
        }
        foreach (bool left in (ReadOnlySpan<bool>)[ false, true ])
        {
            int wall = Top(left ? SamusPoseId.WallJumpLeftPose : SamusPoseId.WallJumpRightPose);
            int spin = Top(left ? SamusPoseId.SpinJumpLeftPose : SamusPoseId.SpinJumpRightPose);
            int space = Top(left ? SamusPoseId.SpaceJumpLeftPose : SamusPoseId.SpaceJumpRightPose);
            int screw = Top(left ? SamusPoseId.ScrewAttackLeftPose : SamusPoseId.ScrewAttackRightPose);
            phase = index - spin;
            if ((uint)phase < 11) return wall + (phase == 0 ? 1 : phase < 9 ? phase + 2 : 2);
            phase = index - space;
            if ((uint)phase < 11) return wall + (phase == 0 ? 1 : phase < 9 ? 13 : 2);
            phase = index - screw;
            if ((uint)phase < 27) return wall + (phase == 0 ? 1 : phase < 25 ? 23 + (phase - 1) / 3 * 3 : 2);
            int lowerWall = Bottom(left ? SamusPoseId.WallJumpLeftPose : SamusPoseId.WallJumpRightPose);
            phase = index - Bottom(left ? SamusPoseId.SpinJumpLeftPose : SamusPoseId.SpinJumpRightPose);
            if ((uint)phase < 11) return lowerWall + (phase == 0 ? 1 : 2);
            phase = index - Bottom(left ? SamusPoseId.SpaceJumpLeftPose : SamusPoseId.SpaceJumpRightPose);
            if ((uint)phase < 11) return lowerWall + (phase == 0 ? 1 : phase < 9 ? phase + 12 : 2);
            phase = index - Bottom(left ? SamusPoseId.ScrewAttackLeftPose : SamusPoseId.ScrewAttackRightPose);
            if ((uint)phase < 27) return lowerWall + (phase == 0 ? 1 : phase < 25 ? 13 + (phase - 1) % 8 : 2);
        }
        foreach (SamusPoseId pose in (ReadOnlySpan<SamusPoseId>)[ SamusPoseId.GrappleSwingRightPose, SamusPoseId.GrappleSwingLeftPose ])
        {
            phase = index - Bottom(pose);
            if ((uint)phase < 64)
            {
                int angle = phase % 16;
                int sharedAngle = angle switch { 4 => 3, 6 => 5, 8 or 9 => 7, 11 => 10, 13 => 12, _ => angle };
                return index - angle + sharedAngle;
            }
        }
        foreach (SamusPoseId pose in (ReadOnlySpan<SamusPoseId>)[ SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose ])
        {
            phase = index - Bottom(pose);
            if ((uint)phase < 9 && phase != 4)
            {
                int breath = phase > 4 ? phase - 5 : phase;
                return Bottom(pose) + (breath == 3 ? 1 : breath);
            }
        }
        foreach (bool left in (ReadOnlySpan<bool>)[ false, true ])
        {
            int normalLanding = Top(left ? SamusPoseId.NormalLandingLeftPose : SamusPoseId.NormalLandingRightPose);
            phase = index - Top(left ? SamusPoseId.SpinLandingLeftPose : SamusPoseId.SpinLandingRightPose);
            if ((uint)phase < 3) return normalLanding + 1 - phase % 2;
            SamusPoseId flash = left ? SamusPoseId.CrystalFlashLeftPose : SamusPoseId.CrystalFlashRightPose;
            phase = index - Top(flash);
            if ((uint)phase < 14)
            {
                if (phase is 3 or 12 or 13) return Top(flash) + 1;
                if (phase is 5 or 10 or 11) return Top(flash) + 4;
                if (phase == 6) return Top(flash) + 2;
                if (phase == 9) return Top(flash) + 7;
            }
            phase = index - Bottom(flash);
            if ((uint)phase < 14)
            {
                if (phase == 3 || phase is >= 6 and <= 9) return Bottom(flash) + 2;
                if (phase is 5 or 10 or 11) return Bottom(flash) + 4;
                if (phase == 12) return Bottom(flash) + 1;
                if (phase == 13) return Bottom(flash);
            }
        }
        foreach (bool left in (ReadOnlySpan<bool>)[ false, true ])
        {
            SamusPoseId crouch = left ? SamusPoseId.DrainedCrouchingLeftPose : SamusPoseId.DrainedCrouchingRightPose;
            SamusPoseId standing = left ? SamusPoseId.DrainedStandingLeftPose : SamusPoseId.DrainedStandingRightPose;
            phase = index - Top(crouch);
            if (left)
            {
                if (phase is >= 2 and <= 6) return Top(crouch) + 2;
                if (phase is 9 or 11 or 14) return Top(crouch) + 7;
                if (phase is >= 19 and <= 23) return Top(crouch) + 14 + Math.Min(phase - 19, 23 - phase);
                if (phase is 26 or 29) return Top(crouch) + 8;
            }
            else
            {
                if (phase is >= 3 and <= 7) return Top(crouch) + 3;
                if (phase == 11) return Top(crouch) + 9;
            }
            phase = index - Bottom(crouch);
            if (left)
            {
                if (phase is >= 2 and <= 6) return Bottom(crouch) + 2;
                if (phase is >= 8 and <= 11 || phase is 26 or 29) return Bottom(crouch) + 8;
                if (phase == 15) return Bottom(crouch) + 7;
                if (phase is >= 19 and <= 23) return Bottom(crouch) + 14 + Math.Min(phase - 19, 23 - phase);
            }
            else
            {
                if (phase is >= 3 and <= 7) return Bottom(crouch) + 3;
                if (phase is >= 8 and <= 11) return Bottom(crouch) + 8;
            }
            phase = index - Top(standing);
            if (phase == 3) return Top(standing) + 1;
            phase = index - Bottom(standing);
            if ((uint)phase < 4) return Bottom(crouch) + 8;
            if (phase == 5) return Bottom(crouch) + (left ? 7 : 14);
        }
        // Every source below precedes its target; independent edits therefore form
        // an acyclic alias graph rather than changing the selected basis itself.
        int source;
        source = Alias(true, SamusPoseId.GrappleCrouchingRightPose, SamusPoseId.FacingRightNormalPose, 9); if (source != index) return source;
        source = Alias(true, SamusPoseId.GrappleCrouchingLeftPose, SamusPoseId.FacingLeftNormalPose, 9); if (source != index) return source;
        source = Alias(true, SamusPoseId.NormalJumpAimUpRightPose, SamusPoseId.StandingAimUpRightPose, 2); if (source != index) return source;
        source = Alias(true, SamusPoseId.NormalJumpAimUpLeftPose, SamusPoseId.StandingAimUpLeftPose, 2); if (source != index) return source;
        source = Alias(true, SamusPoseId.CrouchingAimUpRightPose, SamusPoseId.StandingAimUpRightPose, 2); if (source != index) return source;
        source = Alias(true, SamusPoseId.CrouchingAimUpLeftPose, SamusPoseId.StandingAimUpLeftPose, 2); if (source != index) return source;
        source = Alias(true, SamusPoseId.UnusedPoseAC, SamusPoseId.FiringLandingRightPose, 2); if (source != index) return source;
        source = Alias(true, SamusPoseId.UnusedPoseAD, SamusPoseId.FiringLandingLeftPose, 2); if (source != index) return source;
        source = Alias(true, SamusPoseId.NormalJumpForwardRightPose, SamusPoseId.FiringLandingRightPose, 2); if (source != index) return source;
        source = Alias(true, SamusPoseId.NormalJumpForwardLeftPose, SamusPoseId.FiringLandingLeftPose, 2); if (source != index) return source;
        source = Alias(true, SamusPoseId.FallingAimDownRightPose, SamusPoseId.UnusedPoseAE, 2); if (source != index) return source;
        source = Alias(true, SamusPoseId.FallingAimDownLeftPose, SamusPoseId.UnusedPoseAF, 2); if (source != index) return source;
        source = Alias(true, SamusPoseId.DraygonGrabbedFiringRightPose, SamusPoseId.StandingTransitionRightPose, 1); if (source != index) return source;
        source = Alias(true, SamusPoseId.DraygonGrabbedFiringLeftPose, SamusPoseId.StandingTransitionLeftPose, 1); if (source != index) return source;
        source = Alias(true, SamusPoseId.ShinesparkDiagonalRightPose, SamusPoseId.ShinesparkHorizontalRightPose, 1); if (source != index) return source;
        source = Alias(true, SamusPoseId.ShinesparkDiagonalLeftPose, SamusPoseId.ShinesparkHorizontalLeftPose, 1); if (source != index) return source;
        source = Alias(false, SamusPoseId.FallingAimDownRightPose, SamusPoseId.UnusedPoseAE, 2); if (source != index) return source;
        source = Alias(false, SamusPoseId.FallingAimDownLeftPose, SamusPoseId.UnusedPoseAF, 2); if (source != index) return source;
        source = Alias(false, SamusPoseId.NormalJumpForwardRightPose, SamusPoseId.UnusedPoseAC, 2); if (source != index) return source;
        source = Alias(false, SamusPoseId.NormalJumpForwardLeftPose, SamusPoseId.UnusedPoseAD, 2); if (source != index) return source;
        source = Alias(false, SamusPoseId.UnusedPoseB0, SamusPoseId.UnusedPoseAC, 2); if (source != index) return source;
        source = Alias(false, SamusPoseId.UnusedPoseB1, SamusPoseId.UnusedPoseAD, 2); if (source != index) return source;
        source = Alias(false, SamusPoseId.NormalJumpAimDiagonalUpRightPose, SamusPoseId.NormalJumpAimUpRightPose, 2); if (source != index) return source;
        source = Alias(false, SamusPoseId.NormalJumpAimDiagonalUpLeftPose, SamusPoseId.NormalJumpAimUpLeftPose, 2); if (source != index) return source;
        source = Alias(false, SamusPoseId.FallingAimDiagonalUpRightPose, SamusPoseId.FallingAimUpRightPose, 3); if (source != index) return source;
        source = Alias(false, SamusPoseId.FallingAimDiagonalUpLeftPose, SamusPoseId.FallingAimUpLeftPose, 3); if (source != index) return source;
        source = Alias(false, SamusPoseId.FallingAimDiagonalDownRightPose, SamusPoseId.FallingAimUpRightPose, 3); if (source != index) return source;
        source = Alias(false, SamusPoseId.FallingAimDiagonalDownLeftPose, SamusPoseId.FallingAimUpLeftPose, 3); if (source != index) return source;
        source = Alias(false, SamusPoseId.GrappleCrouchingDownRightPose, SamusPoseId.CrouchingAimDiagonalUpRightPose, 1); if (source != index) return source;
        source = Alias(false, SamusPoseId.GrappleCrouchingDownLeftPose, SamusPoseId.CrouchingAimDiagonalUpLeftPose, 1); if (source != index) return source;
        source = Alias(false, SamusPoseId.ShinesparkDiagonalRightPose, SamusPoseId.ShinesparkHorizontalRightPose, 1); if (source != index) return source;
        source = Alias(false, SamusPoseId.ShinesparkDiagonalLeftPose, SamusPoseId.ShinesparkHorizontalLeftPose, 1); if (source != index) return source;

        source = Reverse(true, SamusPoseId.MoonwalkTurnJumpRightPose, SamusPoseId.MoonwalkTurnJumpLeftPose, 3); if (source != index) return source;
        source = Reverse(true, SamusPoseId.MoonwalkTurnJumpAimUpRightPose, SamusPoseId.MoonwalkTurnJumpAimUpLeftPose, 3); if (source != index) return source;
        source = Reverse(true, SamusPoseId.MoonwalkTurnJumpAimDownRightPose, SamusPoseId.MoonwalkTurnJumpAimDownLeftPose, 3); if (source != index) return source;
        source = Reverse(true, SamusPoseId.UnmorphingTransitionRightPose, SamusPoseId.MorphingTransitionRightPose, 2); if (source != index) return source;
        source = Reverse(true, SamusPoseId.UnmorphingTransitionLeftPose, SamusPoseId.MorphingTransitionLeftPose, 2); if (source != index) return source;
        source = Reverse(true, SamusPoseId.UnusedPoseDd, SamusPoseId.UnusedPoseDb, 3); if (source != index) return source;
        source = Reverse(true, SamusPoseId.UnusedPoseDe, SamusPoseId.UnusedPoseDc, 3); if (source != index) return source;
        source = Reverse(false, SamusPoseId.MoonwalkTurnJumpRightPose, SamusPoseId.MoonwalkTurnJumpLeftPose, 3); if (source != index) return source;
        source = Reverse(false, SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose, SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose, 3); if (source != index) return source;
        // These body halves remain stationary throughout their named aiming/X-ray phases.
        phase = index - Top(SamusPoseId.StandingAimDiagonalUpRightPose); if ((uint)phase < 2) return index - phase;
        phase = index - Top(SamusPoseId.StandingAimDiagonalUpLeftPose); if ((uint)phase < 2) return index - phase;
        phase = index - Top(SamusPoseId.StandingAimDiagonalDownRightPose); if ((uint)phase < 2) return index - phase;
        phase = index - Top(SamusPoseId.StandingAimDiagonalDownLeftPose); if ((uint)phase < 2) return index - phase;
        phase = index - Top(SamusPoseId.StandingTransitionAimUpRightPose); if ((uint)phase < 2) return index - phase;
        phase = index - Top(SamusPoseId.StandingTransitionAimUpLeftPose); if ((uint)phase < 2) return index - phase;
        phase = index - Top(SamusPoseId.FiringLandingRightPose); if ((uint)phase < 2) return index - phase;
        phase = index - Top(SamusPoseId.FiringLandingLeftPose); if ((uint)phase < 2) return index - phase;
        phase = index - Top(SamusPoseId.NormalJumpAimDiagonalUpRightPose); if ((uint)phase < 2) return index - phase;
        phase = index - Top(SamusPoseId.NormalJumpAimDiagonalUpLeftPose); if ((uint)phase < 2) return index - phase;
        phase = index - Top(SamusPoseId.UnusedPoseB0); if ((uint)phase < 2) return index - phase;
        phase = index - Top(SamusPoseId.UnusedPoseB1); if ((uint)phase < 2) return index - phase;
        phase = index - Top(SamusPoseId.KnockbackRightPose); if ((uint)phase < 2) return index - phase;
        phase = index - Top(SamusPoseId.FallingAimDiagonalUpRightPose); if ((uint)phase < 3) return index - phase;
        phase = index - Top(SamusPoseId.FallingAimDiagonalUpLeftPose); if ((uint)phase < 3) return index - phase;
        phase = index - Top(SamusPoseId.FallingAimDiagonalDownRightPose); if ((uint)phase < 3) return index - phase;
        phase = index - Top(SamusPoseId.FallingAimDiagonalDownLeftPose); if ((uint)phase < 3) return index - phase;
        phase = index - Bottom(SamusPoseId.RanIntoWallAimDownRightPose); if ((uint)phase < 2) return index - phase;
        phase = index - Bottom(SamusPoseId.XrayingStandingRightPose); if ((uint)phase < 5) return index - phase;
        phase = index - Bottom(SamusPoseId.XrayingStandingLeftPose); if ((uint)phase < 5) return index - phase;
        phase = index - Bottom(SamusPoseId.XrayingCrouchingRightPose); if ((uint)phase < 5) return index - phase;
        phase = index - Bottom(SamusPoseId.XrayingCrouchingLeftPose); if ((uint)phase < 5) return index - phase;
        phase = index - Bottom(SamusPoseId.CrouchingAimUpRightPose); if ((uint)phase < 2) return index - phase;
        phase = index - Bottom(SamusPoseId.CrouchingAimUpLeftPose); if ((uint)phase < 2) return index - phase;
        return index;
    }
}
