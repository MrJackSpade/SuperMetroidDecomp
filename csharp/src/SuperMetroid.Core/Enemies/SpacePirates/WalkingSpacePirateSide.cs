using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// A walking Space Pirate's poses facing one way (#2557): each of its animations is written once over a side, as a
/// Torizo's are (#2411).
/// </summary>
/// <param name="Facing">The way the pirate faces, walks and fires.</param>
/// <param name="Flinch">Flinching.</param>
/// <param name="Striding">Its stride, a step of each leg.</param>
/// <param name="Standing">Standing.</param>
/// <param name="ClawsLifting">Lifting its claws.</param>
/// <param name="ClawsForward">Its claws forward.</param>
/// <param name="ClawsRaised">Its claws raised.</param>
/// <param name="ClawsRaisedLeaning">Leaning in, its claws raised.</param>
/// <param name="Firing">Firing.</param>
/// <param name="LookingAtViewer">Looking out at the viewer.</param>
/// <param name="LookingAway">Looking away.</param>
internal sealed partial record WalkingSpacePirateSide(
    HorizontalDirection Facing, F Flinch, F[] Striding, F Standing, F ClawsLifting, F ClawsForward, F ClawsRaised, F ClawsRaisedLeaning,
    F Firing, F LookingAtViewer, F LookingAway)
{
    /// <summary>The poses facing left.</summary>
    internal static WalkingSpacePirateSide Left { get; } = new(
        HorizontalDirection.Left, F.WalkingPirateFlinchLeft,
        [
            F.WalkingPirateWalkingLeftFarLegBack, F.WalkingPirateWalkingLeftFarLegLifting, F.WalkingPirateWalkingLeftFarLegPassing,
            F.WalkingPirateWalkingLeftFarLegPlanting, F.WalkingPirateWalkingLeftNearLegBack, F.WalkingPirateWalkingLeftNearLegLifting,
            F.WalkingPirateWalkingLeftNearLegPassing, F.WalkingPirateWalkingLeftNearLegPlanting,
        ],
        F.WalkingPirateStandingFacingLeft, F.WalkingPirateClawsLiftingFacingLeft, F.WalkingPirateClawsForwardFacingLeft,
        F.WalkingPirateClawsRaisedFacingLeft, F.WalkingPirateClawsRaisedLeaningFacingLeft, F.WalkingPirateFiringFacingLeft,
        F.WalkingPirateLookingAtViewerFacingLeft, F.WalkingPirateLookingAwayFacingLeft);

    /// <summary>The poses facing right.</summary>
    internal static WalkingSpacePirateSide Right { get; } = new(
        HorizontalDirection.Right, F.WalkingPirateFlinchRight,
        [
            F.WalkingPirateWalkingRightFarLegBack, F.WalkingPirateWalkingRightFarLegLifting, F.WalkingPirateWalkingRightFarLegPassing,
            F.WalkingPirateWalkingRightFarLegPlanting, F.WalkingPirateWalkingRightNearLegBack, F.WalkingPirateWalkingRightNearLegLifting,
            F.WalkingPirateWalkingRightNearLegPassing, F.WalkingPirateWalkingRightNearLegPlanting,
        ],
        F.WalkingPirateStandingFacingRight, F.WalkingPirateClawsLiftingFacingRight, F.WalkingPirateClawsForwardFacingRight,
        F.WalkingPirateClawsRaisedFacingRight, F.WalkingPirateClawsRaisedLeaningFacingRight, F.WalkingPirateFiringFacingRight,
        F.WalkingPirateLookingAtViewerFacingRight, F.WalkingPirateLookingAwayFacingRight);
}
