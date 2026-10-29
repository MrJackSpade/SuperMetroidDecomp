using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.AssetExtraction.Samus.SamusArmCannonPlacementDefinitions;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Where each pose places the arm cannon's cover and which pose owns each placement.</summary>
internal static class SamusArmCannonPlacements
{
    /// <summary>Finds a pose's extreme body-sprite tip and applies its authored cannon join for a drawing-coordinate byte.</summary>
    /// <remarks>Returns false for unowned coordinates or missing editable body compositions, leaving the installed value intact.</remarks>
    internal static bool TryCoordinate(SamusSpritemapSources spritemaps, int position, out int value)
    {
        value = 0;
        if (!TryOwner(position, out SamusPoseId pose, out int phase, out bool yComponent)) return false;
        int sourcePhase = phase;
        int direction;
        int joinX = 0, joinY = 0;
        bool left;
        switch (pose)
        {
            // Constant horizontal profiles use their first body picture, including
            // descriptor slots whose corresponding OAM pointer is mutable zero.
            case SamusPoseId.FacingRightNormalPose:
            case SamusPoseId.FacingLeftNormalPose:
            case SamusPoseId.NormalJumpGunExtendedRightPose:
            case SamusPoseId.NormalJumpGunExtendedLeftPose:
            case SamusPoseId.CrouchingRightPose:
            case SamusPoseId.CrouchingLeftPose:
            case SamusPoseId.NormalJumpForwardRightPose:
            case SamusPoseId.NormalJumpForwardLeftPose:
            case SamusPoseId.FallingGunExtendedRightPose:
            case SamusPoseId.FallingGunExtendedLeftPose:
                left = pose is SamusPoseId.FacingLeftNormalPose or SamusPoseId.NormalJumpGunExtendedLeftPose
                    or SamusPoseId.CrouchingLeftPose or SamusPoseId.NormalJumpForwardLeftPose
                    or SamusPoseId.FallingGunExtendedLeftPose;
                direction = left ? -1 : 1;
                sourcePhase = 0;
                joinX = left ? -OutwardJoin : OutwardJoin;
                break;
            case SamusPoseId.StandingAimUpRightPose:
            case SamusPoseId.StandingAimUpLeftPose:
            case SamusPoseId.NormalJumpAimUpRightPose:
            case SamusPoseId.NormalJumpAimUpLeftPose:
            case SamusPoseId.FallingAimUpRightPose:
            case SamusPoseId.FallingAimUpLeftPose:
            case SamusPoseId.CrouchingAimUpRightPose:
            case SamusPoseId.CrouchingAimUpLeftPose:
                direction = -2;
                sourcePhase = Math.Min(phase, 1);
                if (pose is not (SamusPoseId.StandingAimUpRightPose or SamusPoseId.StandingAimUpLeftPose)) joinY = -RaisedJoin;
                if (pose == SamusPoseId.FallingAimUpLeftPose && phase != 0) joinY = FallingLeftUpDrop;
                break;
            case SamusPoseId.StandingAimDiagonalUpRightPose:
            case SamusPoseId.StandingAimDiagonalUpLeftPose:
                direction = -2;
                sourcePhase = 0;
                joinX = pose == SamusPoseId.StandingAimDiagonalUpRightPose ? -DiagonalInwardJoin : DiagonalInwardJoin;
                break;
            case SamusPoseId.StandingAimDiagonalDownRightPose:
            case SamusPoseId.StandingAimDiagonalDownLeftPose:
            case SamusPoseId.CrouchingAimDiagonalDownRightPose:
            case SamusPoseId.CrouchingAimDiagonalDownLeftPose:
                direction = 2;
                sourcePhase = 0;
                joinY = -RaisedJoin;
                break;
            case SamusPoseId.MovingRightGunExtendedPose:
            case SamusPoseId.MovingLeftGunExtendedPose:
                direction = pose == SamusPoseId.MovingRightGunExtendedPose ? 1 : -1;
                break;
            case SamusPoseId.RunningAimDiagonalUpRightPose:
            case SamusPoseId.RunningAimDiagonalUpLeftPose:
            case SamusPoseId.MoonwalkAimUpLeftPose:
            case SamusPoseId.MoonwalkAimUpRightPose:
                direction = -2;
                break;
            case SamusPoseId.RunningAimDiagonalDownRightPose:
            case SamusPoseId.RunningAimDiagonalDownLeftPose:
            case SamusPoseId.MoonwalkAimDownLeftPose:
            case SamusPoseId.MoonwalkAimDownRightPose:
                direction = 2;
                break;
            case SamusPoseId.NormalJumpAimDownRightPose:
            case SamusPoseId.NormalJumpAimDownLeftPose:
            case SamusPoseId.FallingAimDownRightPose:
            case SamusPoseId.FallingAimDownLeftPose:
                direction = 2;
                sourcePhase = 1;
                joinX = pose is SamusPoseId.NormalJumpAimDownLeftPose or SamusPoseId.FallingAimDownLeftPose ? -OutwardJoin : OutwardJoin;
                if (pose is SamusPoseId.FallingAimDownRightPose or SamusPoseId.FallingAimDownLeftPose) joinY = -FallingDownRaise;
                break;
            case SamusPoseId.NormalJumpAimDiagonalUpRightPose:
            case SamusPoseId.NormalJumpAimDiagonalUpLeftPose:
            case SamusPoseId.FallingAimDiagonalUpRightPose:
            case SamusPoseId.FallingAimDiagonalUpLeftPose:
                direction = -2;
                sourcePhase = 0;
                break;
            case SamusPoseId.NormalJumpAimDiagonalDownRightPose:
            case SamusPoseId.NormalJumpAimDiagonalDownLeftPose:
            case SamusPoseId.FallingAimDiagonalDownRightPose:
            case SamusPoseId.FallingAimDiagonalDownLeftPose:
                direction = 2;
                sourcePhase = 0;
                break;
            case SamusPoseId.CrouchingAimDiagonalUpRightPose:
            case SamusPoseId.CrouchingAimDiagonalUpLeftPose:
                direction = -2;
                sourcePhase = 0;
                joinY = -RaisedJoin;
                break;
            case SamusPoseId.MoonwalkFacingLeftPose:
            case SamusPoseId.MoonwalkFacingRightPose:
                direction = pose == SamusPoseId.MoonwalkFacingLeftPose ? -1 : 1;
                joinX = pose == SamusPoseId.MoonwalkFacingLeftPose ? MoonwalkInwardJoin : -MoonwalkInwardJoin;
                break;
            case SamusPoseId.NormalJumpTransitionAimUpRightPose:
            case SamusPoseId.NormalJumpTransitionAimUpLeftPose:
                direction = -2;
                sourcePhase = 0;
                break;
            case SamusPoseId.NeutralJumpTransitionRightPose:
            case SamusPoseId.NormalLandingRightPose:
            case SamusPoseId.SpinLandingRightPose:
                // Native B308/B22D/B235 display one/two/three frames respectively.
                // Their extra descriptor slots remain observable through installed
                // arbitrary pointers, but are not additional native animation phases.
                int displayed = pose == SamusPoseId.NeutralJumpTransitionRightPose ? 1 : pose == SamusPoseId.NormalLandingRightPose ? 2 : 3;
                if (phase >= displayed)
                {
                    var tail = phase == displayed ? UnusedFirstTail : UnusedLaterTail;
                    value = unchecked((byte)(yComponent ? tail.Y : tail.X));
                    return true;
                }
                pose = SamusPoseId.SpinLandingRightPose;
                sourcePhase = 0;
                direction = 2;
                break;
            // The forward-facing and default allocations keep their stored coordinates.
            case SamusPoseId.ForwardFacingPowerSuitPose:
            case SamusPoseId.MovingRightNormalPose:
                return false;
            case SamusPoseId.AdjacentCodePoseFD:
            case SamusPoseId.AdjacentCodePoseFE:
            case SamusPoseId.AdjacentCodePoseFF:
            case SamusPoseId.ForwardFacingSuitedPose:
            case SamusPoseId.MovingLeftNormalPose:
            case SamusPoseId.UnusedPose65:
            case SamusPoseId.UnusedPose66:
            case SamusPoseId.MoonwalkTurnJumpLeftPose:
            case SamusPoseId.MoonwalkTurnJumpRightPose:
            case SamusPoseId.MoonwalkTurnJumpAimUpLeftPose:
            case SamusPoseId.MoonwalkTurnJumpAimUpRightPose:
            case SamusPoseId.MoonwalkTurnJumpAimDownLeftPose:
            case SamusPoseId.MoonwalkTurnJumpAimDownRightPose:
            case SamusPoseId.ShinesparkWindupRightPose:
            case SamusPoseId.ShinesparkWindupLeftPose:
            case SamusPoseId.ShinesparkHorizontalRightPose:
            case SamusPoseId.ShinesparkHorizontalLeftPose:
            case SamusPoseId.ShinesparkVerticalRightPose:
            case SamusPoseId.ShinesparkVerticalLeftPose:
            case SamusPoseId.ShinesparkDiagonalRightPose:
            case SamusPoseId.ShinesparkDiagonalLeftPose:
            case SamusPoseId.CrystalFlashRightPose:
            case SamusPoseId.CrystalFlashLeftPose:
            case SamusPoseId.XrayingStandingRightPose:
            case SamusPoseId.XrayingStandingLeftPose:
            case SamusPoseId.DeathSequenceRightPose:
            case SamusPoseId.DeathSequenceLeftPose:
            case SamusPoseId.XrayingCrouchingRightPose:
            case SamusPoseId.XrayingCrouchingLeftPose:
            case SamusPoseId.UnusedPoseDb:
            case SamusPoseId.UnusedPoseDc:
            case SamusPoseId.UnusedPoseDd:
            case SamusPoseId.UnusedPoseDe:
            case SamusPoseId.UnusedPoseDf:
            case SamusPoseId.DrainedCrouchingRightPose:
            case SamusPoseId.DrainedCrouchingLeftPose:
            case SamusPoseId.DrainedStandingRightPose:
            case SamusPoseId.DrainedStandingLeftPose:
            case SamusPoseId.RanIntoWallRightPose:
            case SamusPoseId.RanIntoWallLeftPose:
            case SamusPoseId.RanIntoWallAimUpRightPose:
            case SamusPoseId.RanIntoWallAimUpLeftPose:
            case SamusPoseId.RanIntoWallAimDownRightPose:
            case SamusPoseId.RanIntoWallAimDownLeftPose:
            case SamusPoseId.TurningRightToLeftPose:
            case SamusPoseId.TurningLeftToRightPose:
            case SamusPoseId.SpinJumpRightPose:
            case SamusPoseId.SpinJumpLeftPose:
            case SamusPoseId.SpaceJumpRightPose:
            case SamusPoseId.SpaceJumpLeftPose:
            case SamusPoseId.ScrewAttackRightPose:
            case SamusPoseId.ScrewAttackLeftPose:
            case SamusPoseId.TurningRightToLeftJumpPose:
            case SamusPoseId.TurningLeftToRightJumpPose:
            case SamusPoseId.WallJumpRightPose:
            case SamusPoseId.WallJumpLeftPose:
            case SamusPoseId.GrappleSwingRightPose:
            case SamusPoseId.GrappleSwingLeftPose:
            case SamusPoseId.GrappleStandingRightPose:
            case SamusPoseId.GrappleStandingLeftPose:
            case SamusPoseId.GrappleStandingDownRightPose:
            case SamusPoseId.GrappleStandingDownLeftPose:
            case SamusPoseId.GrappleCrouchingRightPose:
            case SamusPoseId.GrappleCrouchingLeftPose:
            case SamusPoseId.GrappleCrouchingDownRightPose:
            case SamusPoseId.GrappleCrouchingDownLeftPose:
            case SamusPoseId.GrappleWallContactLeftPose:
            case SamusPoseId.GrappleWallContactRightPose:
            case SamusPoseId.DraygonGrabbedNeutralLeftPose:
            case SamusPoseId.DraygonGrabbedAimUpLeftPose:
            case SamusPoseId.DraygonGrabbedFiringLeftPose:
            case SamusPoseId.DraygonGrabbedAimDownLeftPose:
            case SamusPoseId.DraygonGrabbedMovingLeftPose:
            case SamusPoseId.DraygonGrabbedNeutralRightPose:
            case SamusPoseId.DraygonGrabbedAimUpRightPose:
            case SamusPoseId.DraygonGrabbedFiringRightPose:
            case SamusPoseId.DraygonGrabbedAimDownRightPose:
            case SamusPoseId.DraygonGrabbedMovingRightPose:
            case SamusPoseId.DamageBoostLeftPose:
            case SamusPoseId.DamageBoostRightPose:
            case SamusPoseId.KnockbackRightPose:
            case SamusPoseId.KnockbackLeftPose:
            case SamusPoseId.TurningRightToLeftFallingPose:
            case SamusPoseId.TurningLeftToRightFallingPose:
            case SamusPoseId.TurningRightToLeftJumpAimUpPose:
            case SamusPoseId.TurningLeftToRightJumpAimUpPose:
            case SamusPoseId.TurningRightToLeftJumpAimDownPose:
            case SamusPoseId.TurningLeftToRightJumpAimDownPose:
            case SamusPoseId.TurningRightToLeftFallingAimUpPose:
            case SamusPoseId.TurningLeftToRightFallingAimUpPose:
            case SamusPoseId.TurningRightToLeftFallingAimDownPose:
            case SamusPoseId.TurningLeftToRightFallingAimDownPose:
            case SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose:
            case SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose:
            case SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose:
            case SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose:
            case SamusPoseId.MorphBallGroundRightPose:
            case SamusPoseId.MorphBallMovingRightPose:
            case SamusPoseId.MorphBallMovingLeftPose:
            case SamusPoseId.MorphBallFallingRightPose:
            case SamusPoseId.MorphBallFallingLeftPose:
            case SamusPoseId.UnusedKnockbackRightPose:
            case SamusPoseId.UnusedKnockbackLeftPose:
            case SamusPoseId.MorphingTransitionRightPose:
            case SamusPoseId.MorphingTransitionLeftPose:
            case SamusPoseId.UnmorphingTransitionRightPose:
            case SamusPoseId.UnmorphingTransitionLeftPose:
            case SamusPoseId.MorphBallGroundLeftPose:
            case SamusPoseId.SpringBallGroundRightPose:
            case SamusPoseId.SpringBallGroundLeftPose:
            case SamusPoseId.SpringBallMovingRightPose:
            case SamusPoseId.SpringBallMovingLeftPose:
            case SamusPoseId.SpringBallFallingRightPose:
            case SamusPoseId.SpringBallFallingLeftPose:
            case SamusPoseId.SpringBallJumpRightPose:
            case SamusPoseId.SpringBallJumpLeftPose:
            case SamusPoseId.FallingRightPose:
            case SamusPoseId.FallingLeftPose:
            case SamusPoseId.RunningAimUpRightPose:
            case SamusPoseId.RunningAimUpLeftPose:
            case SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose:
            case SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose:
            case SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose:
            case SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose:
            case SamusPoseId.TurningRightToLeftCrouchingPose:
            case SamusPoseId.TurningLeftToRightCrouchingPose:
            case SamusPoseId.TurningRightToLeftAimUpPose:
            case SamusPoseId.TurningLeftToRightAimUpPose:
            case SamusPoseId.TurningRightToLeftAimDiagonalDownPose:
            case SamusPoseId.TurningLeftToRightAimDiagonalDownPose:
            case SamusPoseId.TurningRightToLeftAimDiagonalUpPose:
            case SamusPoseId.TurningLeftToRightAimDiagonalUpPose:
            case SamusPoseId.TurningRightToLeftCrouchingAimUpPose:
            case SamusPoseId.TurningLeftToRightCrouchingAimUpPose:
            case SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose:
            case SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose:
            case SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose:
            case SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose:
            case SamusPoseId.LandingAimUpRightPose:
            case SamusPoseId.LandingAimUpLeftPose:
            case SamusPoseId.LandingAimDiagonalUpRightPose:
            case SamusPoseId.LandingAimDiagonalUpLeftPose:
            case SamusPoseId.LandingAimDiagonalDownRightPose:
            case SamusPoseId.LandingAimDiagonalDownLeftPose:
            case SamusPoseId.FiringLandingRightPose:
            case SamusPoseId.FiringLandingLeftPose:
            case SamusPoseId.CrouchingTransitionAimUpRightPose:
            case SamusPoseId.CrouchingTransitionAimUpLeftPose:
            case SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose:
            case SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose:
            case SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose:
            case SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose:
            case SamusPoseId.StandingTransitionAimUpRightPose:
            case SamusPoseId.StandingTransitionAimUpLeftPose:
            case SamusPoseId.StandingTransitionAimDiagonalUpRightPose:
            case SamusPoseId.StandingTransitionAimDiagonalUpLeftPose:
            case SamusPoseId.StandingTransitionAimDiagonalDownRightPose:
            case SamusPoseId.StandingTransitionAimDiagonalDownLeftPose:
            case SamusPoseId.CrouchingTransitionRightPose:
            case SamusPoseId.CrouchingTransitionLeftPose:
            case SamusPoseId.StandingTransitionRightPose:
            case SamusPoseId.StandingTransitionLeftPose:
            case SamusPoseId.NeutralJumpTransitionLeftPose:
            case SamusPoseId.NeutralJumpRightPose:
            case SamusPoseId.NeutralJumpLeftPose:
            case SamusPoseId.NormalLandingLeftPose:
            case SamusPoseId.SpinLandingLeftPose:
            case SamusPoseId.UnusedPose20:
            case SamusPoseId.UnusedPose21:
            case SamusPoseId.UnusedPose22:
            case SamusPoseId.UnusedPose23:
            case SamusPoseId.UnusedPose24:
            case SamusPoseId.UnusedPose39:
            case SamusPoseId.UnusedPose3A:
            case SamusPoseId.UnusedPose3F:
            case SamusPoseId.UnusedPose40:
            case SamusPoseId.UnusedPose42:
            case SamusPoseId.UnusedPose45:
            case SamusPoseId.UnusedPose46:
            case SamusPoseId.UnusedPose47:
            case SamusPoseId.UnusedPose48:
            case SamusPoseId.UnusedPose5B:
            case SamusPoseId.UnusedPose5C:
            case SamusPoseId.UnusedPose5D:
            case SamusPoseId.UnusedPose5E:
            case SamusPoseId.UnusedPose5F:
            case SamusPoseId.UnusedPose60:
            case SamusPoseId.UnusedPose61:
            case SamusPoseId.UnusedPose62:
            case SamusPoseId.UnusedPose63:
            case SamusPoseId.UnusedPose64:
            case SamusPoseId.UnusedPoseAC:
            case SamusPoseId.UnusedPoseAD:
            case SamusPoseId.UnusedPoseAE:
            case SamusPoseId.UnusedPoseAF:
            case SamusPoseId.UnusedPoseB0:
            case SamusPoseId.UnusedPoseB1:
            case SamusPoseId.UnusedPoseC5:
            case SamusPoseId.UnusedPoseC6:
            default:
                throw new InvalidOperationException($"{pose} does not own an arm-cannon drawing allocation.");
        }
        int index = spritemaps.TopBase(pose) + sourcePhase;
        if ((uint)index >= InstalledSamusSpritemaps.EntryCount ||
            !spritemaps.TryGet((ushort)index, out SamusSpritemapDefinition? map) ||
            map.Parts.Length == 0) return false;
        int best = int.MinValue, selectedX = 0, selectedY = 0;
        foreach (SpritePart part in map.Parts)
        {
            int x = part.Offset.X;
            int y = part.Offset.Y;
            int score = Math.Abs(direction) == 1 ? x * direction : y * Math.Sign(direction);
            if (score <= best) continue;
            best = score;
            selectedX = x;
            selectedY = y;
        }
        value = unchecked((byte)(yComponent ? selectedY + joinY : selectedX + joinX));
        return true;
    }

    /// <summary>Locates the semantic pose, animation phase, and X/Y component owning a native cannon descriptor byte.</summary>
    private static bool TryOwner(int position, out SamusPoseId pose, out int phase, out bool yComponent)
    {
        pose = 0; phase = 0; yComponent = false;
        if (position < 0 ||
            position >= SamusArmCannonArtworkFormat.AdjacentCostStart ||
            SamusArmCannonArtworkFormat.TryStockDrawingByte(position, out _)) return false;
        int start = -1;
        // Resolve allocation identity through the existing semantic pose dispatch.
        // No second descriptor-position or pose-selection table is introduced.
        for (int candidate = 0; candidate < InstalledSamusBodyArtwork.PoseCount; candidate++)
        {
            int descriptorStart = SamusArmCannonArtworkFormat.StockPoseDescriptor(candidate);
            if (descriptorStart > position || descriptorStart <= start) continue;
            start = descriptorStart;
            pose = SamusPoseNumbering.FromNumber(candidate);
        }
        if (start < 0 || !SamusArmCannonArtworkFormat.TryStockDrawingByte(start, out int selector)) return false;
        int relative = position - start - ((selector & 128) != 0 ? 4 : 2);
        if (relative < 0) return false;
        phase = relative / 2;
        yComponent = (relative & 1) != 0;
        return true;
    }
}
