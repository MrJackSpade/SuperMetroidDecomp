using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>One Ceres cinematic actor's motion, animation, colouring and starting speed; where it starts is installed (#1615).</summary>
internal readonly partial record struct CeresFlightActorDefinition(
    CeresAnimation Animation,
    CeresSpriteColouring Colouring,
    SignedEightEight InitialVelocity,
    SignedSixteenSixteen HorizontalDelta,
    bool DriftsRoundLoop);
