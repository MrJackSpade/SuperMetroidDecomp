using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>One Ceres cinematic actor definition plus its fixed initializer result; where it starts is installed (#1615).</summary>
[IndependentFlag("whether the authored actor wraps horizontally, a fact of the actor data")]
internal readonly record struct CeresFlightActorDefinition(
    CinematicSpriteFrameBehavior ActiveFrameBehavior,
    CinematicSpriteSegment Animation,
    CeresSpriteColouring Colouring,
    SignedEightEight InitialVelocity,
    SignedSixteenSixteen HorizontalDelta,
    bool WrapX);
