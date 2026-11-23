using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// A ninja Space Pirate's poses facing one way (#2557), beside its claw throw's (<see cref="NinjaSpacePirateClawPoses"/>):
/// each of its animations is written once over a side, as a Torizo's are (#2411), and the offsets a side's claws fly from
/// are mirrored by the way it faces.
/// </summary>
/// <param name="Claw">The poses of its claw throw, which name the way it faces.</param>
/// <param name="Spinning">Its spin, an eighth of a turn a pose.</param>
/// <param name="ReadyFeetClose">Ready, its feet close.</param>
/// <param name="ReadyFeetApart">Ready, its feet apart.</param>
/// <param name="ReadyFeetWide">Ready, its feet wide.</param>
/// <param name="Flinch">Flinching.</param>
/// <param name="LeapCrouch">Crouched to leap.</param>
/// <param name="TuckedJump">Tucked in its jump.</param>
/// <param name="DiveKick">Diving into its kick.</param>
/// <param name="Running">Its run to its post, a stride of each leg.</param>
/// <param name="LookingAtViewer">Looking out at the viewer.</param>
/// <param name="Standing">Standing.</param>
/// <param name="LookingAway">Looking away.</param>
/// <param name="KickChambered">Its kick chambered.</param>
/// <param name="KickExtending">Its kick extending.</param>
/// <param name="KickExtended">Its kick extended.</param>
internal sealed partial record NinjaSpacePirateSide(
    NinjaSpacePirateClawPoses Claw, F[] Spinning, F ReadyFeetClose, F ReadyFeetApart, F ReadyFeetWide, F Flinch, F LeapCrouch,
    F TuckedJump, F DiveKick, F[] Running, F LookingAtViewer, F Standing, F LookingAway, F KickChambered, F KickExtending,
    F KickExtended)
{
    /// <summary>The way the pirate faces.</summary>
    internal HorizontalDirection Facing => Claw.Facing;

    /// <summary><paramref name="offset"/>, given for a pirate facing right, mirrored to the way this side faces.</summary>
    /// <param name="offset">The offset ahead of a right-facing pirate.</param>
    internal Offset16<RoomPixels> Ahead(Offset16<RoomPixels> offset) =>
        Facing == HorizontalDirection.Left ? new(-offset.X, offset.Y) : offset;

    /// <summary>The poses facing left.</summary>
    internal static NinjaSpacePirateSide Left { get; } = new(
        NinjaSpacePirateClawPoses.Left,
        [
            F.NinjaPirateSpinningFacingLeftTurned0, F.NinjaPirateSpinningFacingLeftTurned45, F.NinjaPirateSpinningFacingLeftTurned90,
            F.NinjaPirateSpinningFacingLeftTurned135, F.NinjaPirateSpinningFacingLeftTurned180, F.NinjaPirateSpinningFacingLeftTurned225,
            F.NinjaPirateSpinningFacingLeftTurned270, F.NinjaPirateSpinningFacingLeftTurned315,
        ],
        F.NinjaPirateReadyFacingLeftFeetClose, F.NinjaPirateReadyFacingLeftFeetApart, F.NinjaPirateReadyFacingLeftFeetWide,
        F.NinjaPirateFlinchFacingLeft, F.NinjaPirateLeapCrouchFacingLeft, F.NinjaPirateTuckedJumpFacingLeft, F.NinjaPirateDiveKickFacingLeft,
        [
            F.NinjaPirateRunningLeftFarLegBack, F.NinjaPirateRunningLeftFarLegLifting, F.NinjaPirateRunningLeftFarLegPassing,
            F.NinjaPirateRunningLeftFarLegPlanting, F.NinjaPirateRunningLeftNearLegBack, F.NinjaPirateRunningLeftNearLegLifting,
            F.NinjaPirateRunningLeftNearLegPassing, F.NinjaPirateRunningLeftNearLegPlanting,
        ],
        F.NinjaPirateLookingAtViewerFacingLeft, F.NinjaPirateStandingFacingLeft, F.NinjaPirateLookingAwayFacingLeft,
        F.NinjaPirateKickChamberedFacingLeft, F.NinjaPirateKickExtendingFacingLeft, F.NinjaPirateKickExtendedFacingLeft);

    /// <summary>The poses facing right.</summary>
    internal static NinjaSpacePirateSide Right { get; } = new(
        NinjaSpacePirateClawPoses.Right,
        [
            F.NinjaPirateSpinningFacingRightTurned0, F.NinjaPirateSpinningFacingRightTurned45, F.NinjaPirateSpinningFacingRightTurned90,
            F.NinjaPirateSpinningFacingRightTurned135, F.NinjaPirateSpinningFacingRightTurned180, F.NinjaPirateSpinningFacingRightTurned225,
            F.NinjaPirateSpinningFacingRightTurned270, F.NinjaPirateSpinningFacingRightTurned315,
        ],
        F.NinjaPirateReadyFacingRightFeetClose, F.NinjaPirateReadyFacingRightFeetApart, F.NinjaPirateReadyFacingRightFeetWide,
        F.NinjaPirateFlinchFacingRight, F.NinjaPirateLeapCrouchFacingRight, F.NinjaPirateTuckedJumpFacingRight, F.NinjaPirateDiveKickFacingRight,
        [
            F.NinjaPirateRunningRightFarLegBack, F.NinjaPirateRunningRightFarLegLifting, F.NinjaPirateRunningRightFarLegPassing,
            F.NinjaPirateRunningRightFarLegPlanting, F.NinjaPirateRunningRightNearLegBack, F.NinjaPirateRunningRightNearLegLifting,
            F.NinjaPirateRunningRightNearLegPassing, F.NinjaPirateRunningRightNearLegPlanting,
        ],
        F.NinjaPirateLookingAtViewerFacingRight, F.NinjaPirateStandingFacingRight, F.NinjaPirateLookingAwayFacingRight,
        F.NinjaPirateKickChamberedFacingRight, F.NinjaPirateKickExtendingFacingRight, F.NinjaPirateKickExtendedFacingRight);
}
