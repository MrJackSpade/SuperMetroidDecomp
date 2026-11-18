using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The poses of a ninja Space Pirate's claw throw facing one way (#2451): its wind-up and its follow-through. Each throw is
/// written once over a facing's poses, as a Torizo's animations are written once over a side (#2411).
/// </summary>
/// <param name="Facing">The way the pirate faces, and the claw flies.</param>
/// <param name="Crouch">Crouching.</param>
/// <param name="ClawsReady">Its claws ready.</param>
/// <param name="ClawDrawnBack">A claw drawn back.</param>
/// <param name="ClawRaisedHigh">The claw raised high.</param>
/// <param name="ClawRaisedOverhead">The claw raised overhead.</param>
/// <param name="ClawSwungForward">The claw swung forward, as it lets go.</param>
/// <param name="ClawsSweptDown">Its claws swept down.</param>
/// <param name="UprightClawsLowered">Upright, its claws lowered.</param>
internal sealed record NinjaSpacePirateClawPoses(
    HorizontalDirection Facing, F Crouch, F ClawsReady, F ClawDrawnBack, F ClawRaisedHigh, F ClawRaisedOverhead, F ClawSwungForward,
    F ClawsSweptDown, F UprightClawsLowered)
{
    /// <summary>The poses facing left.</summary>
    internal static NinjaSpacePirateClawPoses Left { get; } = new(
        HorizontalDirection.Left, F.NinjaPirateCrouchFacingLeft, F.NinjaPirateClawsReadyFacingLeft, F.NinjaPirateClawDrawnBackFacingLeft,
        F.NinjaPirateClawRaisedHighFacingLeft, F.NinjaPirateClawRaisedOverheadFacingLeft, F.NinjaPirateClawSwungForwardFacingLeft,
        F.NinjaPirateClawsSweptDownFacingLeft, F.NinjaPirateUprightClawsLoweredFacingLeft);

    /// <summary>The poses facing right.</summary>
    internal static NinjaSpacePirateClawPoses Right { get; } = new(
        HorizontalDirection.Right, F.NinjaPirateCrouchFacingRight, F.NinjaPirateClawsReadyFacingRight, F.NinjaPirateClawDrawnBackFacingRight,
        F.NinjaPirateClawRaisedHighFacingRight, F.NinjaPirateClawRaisedOverheadFacingRight, F.NinjaPirateClawSwungForwardFacingRight,
        F.NinjaPirateClawsSweptDownFacingRight, F.NinjaPirateUprightClawsLoweredFacingRight);
}
