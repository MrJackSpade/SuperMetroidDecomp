using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Death;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Calculated native pose-half sharing and allocation geometry.</summary>
/// <remarks>The exact remaining 813 drawing choices, one observable zero-WRAM identity and named sequence memberships are selected composition content. They cannot be derived from physical pose quantities without restating the chosen pictures. Parts, pixels and timing have separate owners; see Stream 1 Batch 103 for exact native membership and consumer evidence.</remarks>
internal static class SamusSpritemapFrameDefinitions
{
    /// <summary>$92:8091/8151/81D1/8291: appearance lists contain96 indexed phases, including the initial body and mutable-memory slot.</summary>
    private static int AppearancePhases =>
        SamusAnimationDelayDefinitions.BytesToStreamEnd(SamusAnimationDelayDefinitions.ListForPose(SamusPoseId.ForwardFacingPowerSuitPose)) - 2;
    /// <summary>$92:8095..812F: the initial electricity sequence alternates three distinct discharges with its shared empty composition.</summary>
    private const int AppearanceDischargeEnd = 80;
    /// <summary>$92:86F3/8777: upper grapple composition repeats after one32-position turn; the two final entries select its center orientation.</summary>
    private const int GrappleTurnPhases = GrappleSwingFrameDefinitions.FrameCount;
    /// <summary>$92:8EAD begins the right morph sequence's spin, memory, and bounce slots.</summary>
    private const int FirstRightMorph = 0x710;
    /// <summary>$92:8EC1 begins the mirrored left morph sequence used to derive its phase span.</summary>
    private const int FirstLeftMorph = 0x71a;

    /// <summary>The first of the consecutive bubble compositions.</summary>
    private static readonly SamusSpritemapId FirstBubble = new(449);
    /// <summary>The first of the consecutive diving-splash compositions.</summary>
    private static readonly SamusSpritemapId FirstDiving = new(458);
    /// <summary>The first of the right-facing suit-explosion compositions.</summary>
    private static readonly SamusSpritemapId FirstDeathRight = new(440);
    /// <summary>The first of the left-facing suit-explosion compositions.</summary>
    private static readonly SamusSpritemapId FirstDeathLeft = new(431);
    /// <summary>Starts right-facing grapple angles zero through eight, selected in reverse record order.</summary>
    private static readonly SamusSpritemapId GrappleRightFirstQuadrant = new(168);
    /// <summary>Starts the middle right-facing grapple arc, selected in reverse order.</summary>
    private static readonly SamusSpritemapId GrappleRightHalfTurn = new(136);
    /// <summary>Starts the final right-facing grapple quadrant.</summary>
    private static readonly SamusSpritemapId GrappleRightLastQuadrant = new(177);
    /// <summary>Starts left-facing grapple angles in forward record order.</summary>
    private static readonly SamusSpritemapId GrappleLeftFirstQuadrant = new(192);
    /// <summary>Starts the middle left-facing grapple arc.</summary>
    private static readonly SamusSpritemapId GrappleLeftHalfTurn = new(152);
    /// <summary>Starts the final left-facing grapple quadrant.</summary>
    private static readonly SamusSpritemapId GrappleLeftLastQuadrant = new(184);
    /// <summary>Top-half pointer index $186 begins the bubble OAM sequence.</summary>
    private const int Bubbles = 0x186;
    /// <summary>Top-half pointer index $18F begins the diving OAM sequence.</summary>
    private const int Diving = 0x18f;
    /// <summary>Pointer index $81C begins right-facing suit-explosion maps.</summary>
    private const int DeathRight = 0x81c;
    /// <summary>Pointer index $825 begins left-facing suit-explosion maps.</summary>
    private const int DeathLeft = 0x825;

    /// <summary>
    /// Calculates the composition of entries in consecutive composition runs and recognizes non-drawing control
    /// slots, which select no composition.
    /// </summary>
    /// <param name="index">The pose-half table entry.</param>
    /// <param name="compositionCount">How many compositions are installed.</param>
    /// <param name="composition">The calculated composition, or null for a slot that draws nothing.</param>
    internal static bool TryComposition(int index, int compositionCount, out SamusSpritemapId? composition)
    {
        if (IsControlOrOmittedHalf(index)) { composition = null; return true; }
        int phase = index - Bubbles;
        if ((uint)phase < Diving - Bubbles) return Advance(FirstBubble, phase, compositionCount, out composition);
        phase = index - Diving;
        if ((uint)phase < 9) return Advance(FirstDiving, phase, compositionCount, out composition);
        phase = index - DeathRight;
        if ((uint)phase < DeathLeft - DeathRight) return Advance(FirstDeathRight, phase, compositionCount, out composition);
        phase = index - DeathLeft;
        if ((uint)phase < SamusDeathExplosionTimingDefinitions.RecordCount) return Advance(FirstDeathLeft, phase, compositionCount, out composition);
        phase = index - SamusSpritemapPoseDefinitions.TopBase(SamusPoseId.GrappleSwingRightPose);
        if ((uint)phase < GrappleTurnPhases)
            return phase <= 8 ? Advance(GrappleRightFirstQuadrant, 8 - phase, compositionCount, out composition)
                : phase <= 24 ? Advance(GrappleRightHalfTurn, 24 - phase, compositionCount, out composition)
                : Advance(GrappleRightLastQuadrant, 31 - phase, compositionCount, out composition);
        phase = index - SamusSpritemapPoseDefinitions.TopBase(SamusPoseId.GrappleSwingLeftPose);
        if ((uint)phase < GrappleTurnPhases)
            return phase < 8 ? Advance(GrappleLeftFirstQuadrant, phase, compositionCount, out composition)
                : phase < 24 ? Advance(GrappleLeftHalfTurn, phase - 8, compositionCount, out composition)
                : Advance(GrappleLeftLastQuadrant, phase - 24, compositionCount, out composition);
        composition = null;
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
            ? SamusSpritemapPoseDefinitions.TopBase(pose) : SamusSpritemapPoseDefinitions.BottomBase(pose));
        bool Command(bool upper, SamusPoseId pose, int extent, int firstPhase = 0)
        {
            int phase = Phase(upper, pose);
            if ((uint)phase >= extent || phase < firstPhase) return false;
            SamusAnimationDelayList list = SamusAnimationDelayDefinitions.ListForPose(pose);
            int remaining = SamusAnimationDelayDefinitions.BytesToStreamEnd(list);
            for (int cursor = 0; cursor <= phase;)
            {
                if (cursor >= remaining) return false;
                byte value = SamusAnimationDelayDefinitions.ReadAnimationByte(null, list, cursor, windows: null);
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
    /// <summary>
    /// The composition <paramref name="ordinal"/> places after <paramref name="start"/> in a run; a run's trailing
    /// entries past its last step (a negative ordinal) repeat its first composition.
    /// </summary>
    private static bool Advance(SamusSpritemapId start, int ordinal, int compositionCount, out SamusSpritemapId? composition)
    {
        SamusSpritemapId selected = start.After(Math.Max(ordinal, 0));
        composition = selected.Ordinal < compositionCount ? selected : null;
        return composition is not null;
    }
    /// <summary>Resolves authored pose-half image sharing to an earlier physical composition slot.</summary>
    internal static int SourceIndex(int index)
    {
        if ((uint)index >= SamusSpritemapArtworkCatalog.PointerCount) throw new ArgumentOutOfRangeException(nameof(index));
        static int Top(SamusPoseId pose) => SamusSpritemapPoseDefinitions.TopBase(pose);
        static int Bottom(SamusPoseId pose) => SamusSpritemapPoseDefinitions.BottomBase(pose);
        int Alias(Func<SamusPoseId, int> halfBase, SamusPoseId target, SamusPoseId source, int count)
        {
            int phase = index - halfBase(target);
            return (uint)phase < count ? halfBase(source) + phase : index;
        }
        int Reverse(Func<SamusPoseId, int> halfBase, SamusPoseId target, SamusPoseId source, int count)
        {
            int phase = index - halfBase(target);
            return (uint)phase < count ? halfBase(source) + count - 1 - phase : index;
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
                if (phase is 3 or >= 6 and <= 9) return Bottom(flash) + 2;
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
                if (phase is >= 8 and <= 11 or 26 or 29) return Bottom(crouch) + 8;
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
        source = Alias(Top, SamusPoseId.GrappleCrouchingRightPose, SamusPoseId.FacingRightNormalPose, 9); if (source != index) return source;
        source = Alias(Top, SamusPoseId.GrappleCrouchingLeftPose, SamusPoseId.FacingLeftNormalPose, 9); if (source != index) return source;
        source = Alias(Top, SamusPoseId.NormalJumpAimUpRightPose, SamusPoseId.StandingAimUpRightPose, 2); if (source != index) return source;
        source = Alias(Top, SamusPoseId.NormalJumpAimUpLeftPose, SamusPoseId.StandingAimUpLeftPose, 2); if (source != index) return source;
        source = Alias(Top, SamusPoseId.CrouchingAimUpRightPose, SamusPoseId.StandingAimUpRightPose, 2); if (source != index) return source;
        source = Alias(Top, SamusPoseId.CrouchingAimUpLeftPose, SamusPoseId.StandingAimUpLeftPose, 2); if (source != index) return source;
        source = Alias(Top, SamusPoseId.UnusedPoseAC, SamusPoseId.FiringLandingRightPose, 2); if (source != index) return source;
        source = Alias(Top, SamusPoseId.UnusedPoseAD, SamusPoseId.FiringLandingLeftPose, 2); if (source != index) return source;
        source = Alias(Top, SamusPoseId.NormalJumpForwardRightPose, SamusPoseId.FiringLandingRightPose, 2); if (source != index) return source;
        source = Alias(Top, SamusPoseId.NormalJumpForwardLeftPose, SamusPoseId.FiringLandingLeftPose, 2); if (source != index) return source;
        source = Alias(Top, SamusPoseId.FallingAimDownRightPose, SamusPoseId.UnusedPoseAE, 2); if (source != index) return source;
        source = Alias(Top, SamusPoseId.FallingAimDownLeftPose, SamusPoseId.UnusedPoseAF, 2); if (source != index) return source;
        source = Alias(Top, SamusPoseId.DraygonGrabbedFiringRightPose, SamusPoseId.StandingTransitionRightPose, 1); if (source != index) return source;
        source = Alias(Top, SamusPoseId.DraygonGrabbedFiringLeftPose, SamusPoseId.StandingTransitionLeftPose, 1); if (source != index) return source;
        source = Alias(Top, SamusPoseId.ShinesparkDiagonalRightPose, SamusPoseId.ShinesparkHorizontalRightPose, 1); if (source != index) return source;
        source = Alias(Top, SamusPoseId.ShinesparkDiagonalLeftPose, SamusPoseId.ShinesparkHorizontalLeftPose, 1); if (source != index) return source;
        source = Alias(Bottom, SamusPoseId.FallingAimDownRightPose, SamusPoseId.UnusedPoseAE, 2); if (source != index) return source;
        source = Alias(Bottom, SamusPoseId.FallingAimDownLeftPose, SamusPoseId.UnusedPoseAF, 2); if (source != index) return source;
        source = Alias(Bottom, SamusPoseId.NormalJumpForwardRightPose, SamusPoseId.UnusedPoseAC, 2); if (source != index) return source;
        source = Alias(Bottom, SamusPoseId.NormalJumpForwardLeftPose, SamusPoseId.UnusedPoseAD, 2); if (source != index) return source;
        source = Alias(Bottom, SamusPoseId.UnusedPoseB0, SamusPoseId.UnusedPoseAC, 2); if (source != index) return source;
        source = Alias(Bottom, SamusPoseId.UnusedPoseB1, SamusPoseId.UnusedPoseAD, 2); if (source != index) return source;
        source = Alias(Bottom, SamusPoseId.NormalJumpAimDiagonalUpRightPose, SamusPoseId.NormalJumpAimUpRightPose, 2); if (source != index) return source;
        source = Alias(Bottom, SamusPoseId.NormalJumpAimDiagonalUpLeftPose, SamusPoseId.NormalJumpAimUpLeftPose, 2); if (source != index) return source;
        source = Alias(Bottom, SamusPoseId.FallingAimDiagonalUpRightPose, SamusPoseId.FallingAimUpRightPose, 3); if (source != index) return source;
        source = Alias(Bottom, SamusPoseId.FallingAimDiagonalUpLeftPose, SamusPoseId.FallingAimUpLeftPose, 3); if (source != index) return source;
        source = Alias(Bottom, SamusPoseId.FallingAimDiagonalDownRightPose, SamusPoseId.FallingAimUpRightPose, 3); if (source != index) return source;
        source = Alias(Bottom, SamusPoseId.FallingAimDiagonalDownLeftPose, SamusPoseId.FallingAimUpLeftPose, 3); if (source != index) return source;
        source = Alias(Bottom, SamusPoseId.GrappleCrouchingDownRightPose, SamusPoseId.CrouchingAimDiagonalUpRightPose, 1); if (source != index) return source;
        source = Alias(Bottom, SamusPoseId.GrappleCrouchingDownLeftPose, SamusPoseId.CrouchingAimDiagonalUpLeftPose, 1); if (source != index) return source;
        source = Alias(Bottom, SamusPoseId.ShinesparkDiagonalRightPose, SamusPoseId.ShinesparkHorizontalRightPose, 1); if (source != index) return source;
        source = Alias(Bottom, SamusPoseId.ShinesparkDiagonalLeftPose, SamusPoseId.ShinesparkHorizontalLeftPose, 1); if (source != index) return source;

        source = Reverse(Top, SamusPoseId.MoonwalkTurnJumpRightPose, SamusPoseId.MoonwalkTurnJumpLeftPose, 3); if (source != index) return source;
        source = Reverse(Top, SamusPoseId.MoonwalkTurnJumpAimUpRightPose, SamusPoseId.MoonwalkTurnJumpAimUpLeftPose, 3); if (source != index) return source;
        source = Reverse(Top, SamusPoseId.MoonwalkTurnJumpAimDownRightPose, SamusPoseId.MoonwalkTurnJumpAimDownLeftPose, 3); if (source != index) return source;
        source = Reverse(Top, SamusPoseId.UnmorphingTransitionRightPose, SamusPoseId.MorphingTransitionRightPose, 2); if (source != index) return source;
        source = Reverse(Top, SamusPoseId.UnmorphingTransitionLeftPose, SamusPoseId.MorphingTransitionLeftPose, 2); if (source != index) return source;
        source = Reverse(Top, SamusPoseId.UnusedPoseDd, SamusPoseId.UnusedPoseDb, 3); if (source != index) return source;
        source = Reverse(Top, SamusPoseId.UnusedPoseDe, SamusPoseId.UnusedPoseDc, 3); if (source != index) return source;
        source = Reverse(Bottom, SamusPoseId.MoonwalkTurnJumpRightPose, SamusPoseId.MoonwalkTurnJumpLeftPose, 3); if (source != index) return source;
        source = Reverse(Bottom, SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose, SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose, 3); if (source != index) return source;
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
