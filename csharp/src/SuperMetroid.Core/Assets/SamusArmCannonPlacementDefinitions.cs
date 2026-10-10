using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Body-sprite attachment geometry for the native arm-cannon overlay.</summary>
/// <remarks>
/// Named joins and phase choices are narrowly retained overlay composition. Body
/// coordinates remain owned by the independently editable spritemaps. A missing
/// body source leaves the supplied cover coordinate intact at installation.
/// </remarks>
internal static class SamusArmCannonPlacementDefinitions
{
    /// <summary>$90:C9DF/C9F3: the horizontal cover begins one pixel beyond the closed cannon-tip OBJ origin.</summary>
    private const int OutwardJoin = 1;
    /// <summary>$90:CA17/CA1B: stationary diagonal-up covers move one pixel inward from the same body tip used by the upward transition.</summary>
    private const int DiagonalInwardJoin = 1;
    /// <summary>$90:CA20 and CABA/CABC: selected diagonal-down and airborne/crouched upward joins sit one pixel above the body tip.</summary>
    private const int RaisedJoin = 1;
    /// <summary>$90:CB34/CB3A: falling downward covers sit four pixels above the terminal downward tip.</summary>
    private const int FallingDownRaise = 4;
    /// <summary>$90:CB2E/CB30: the left falling-up terminal overlay sits three pixels below its body-tip origin.</summary>
    private const int FallingLeftUpDrop = 3;
    /// <summary>$90:CBA7/CBB5: reversed horizontal moonwalk covers move three pixels inward from the body tip.</summary>
    private const int MoonwalkInwardJoin = 3;

    /// <summary>$90:CAD5/CAD6: first unused retracted descriptor tail, repeated at CBFF/CC00 and CC0D/CC0E.</summary>
    private static (int X, int Y) UnusedFirstTail => (6, 2);
    /// <summary>$90:CAD7/CAD8: later unused retracted descriptor tail, repeated at CC01..CC04 and CC0F..CC14.</summary>
    private static (int X, int Y) UnusedLaterTail => (-19, -2);

    internal static bool TryCoordinate(SamusBodyArtworkCatalog body, ushort address, out byte value)
    {
        value = 0;
        if (!TryOwner(address, out SamusPoseId pose, out int phase, out bool yComponent)) return false;
        int sourcePhase = phase;
        int direction;
        int joinX = 0, joinY = 0;
        bool left = false;
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
            default:
                throw new InvalidOperationException($"{pose} does not own an arm-cannon drawing allocation.");
        }
        int index = body.Spritemaps.TopBase((byte)pose) + sourcePhase;
        if ((uint)index >= SamusSpritemapArtworkCatalog.PointerCount ||
            !body.Spritemaps.TryGet((ushort)index, out SamusSpritemapDefinition? map) ||
            map!.Parts.Length == 0) return false;
        int best = int.MinValue, selectedX = 0, selectedY = 0;
        foreach (SamusSpritePart part in map.Parts)
        {
            int x = part.X & 511;
            if (x >= 256) x -= 512;
            int y = unchecked((sbyte)part.Y);
            int score = Math.Abs(direction) == 1 ? x * direction : y * Math.Sign(direction);
            if (score <= best) continue;
            best = score;
            selectedX = x;
            selectedY = y;
        }
        value = unchecked((byte)(yComponent ? selectedY + joinY : selectedX + joinX));
        return true;
    }

    private static bool TryOwner(ushort address, out SamusPoseId pose, out int phase, out bool yComponent)
    {
        pose = 0; phase = 0; yComponent = false;
        if (address < SamusArmCannonArtworkFormat.DrawingDataStart ||
            address >= (SamusComboRomData.Costs & 0xffff) ||
            SamusArmCannonArtworkFormat.TryStockDrawingByte(address, out _)) return false;
        int start = -1;
        // Resolve allocation identity through the existing semantic pose dispatch.
        // No second descriptor-address or pose-selection table is introduced.
        for (int candidate = 0; candidate < SamusBodyArtworkCatalog.PoseCount; candidate++)
        {
            int pointer = SamusArmCannonArtworkFormat.StockPoseDrawingData(candidate);
            if (pointer > address || pointer <= start) continue;
            start = pointer;
            pose = (SamusPoseId)candidate;
        }
        if (start < 0 || !SamusArmCannonArtworkFormat.TryStockDrawingByte((ushort)start, out byte selector)) return false;
        int relative = address - start - ((selector & 128) != 0 ? 4 : 2);
        if (relative < 0) return false;
        phase = relative / 2;
        yComponent = (relative & 1) != 0;
        return true;
    }
}
