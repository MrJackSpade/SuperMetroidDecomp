using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>One Ceres cinematic actor definition plus its fixed initializer result.</summary>
[IndependentFlag("whether the authored actor wraps horizontally, a fact of the actor data")]
internal readonly record struct CeresFlightActorDefinition(
    CinematicSpriteFrameBehavior ActiveFrameBehavior,
    CinematicSpriteProgram Program,
    Position16<ScreenPixels> Position,
    CeresSpriteColouring Colouring,
    SignedEightEight InitialVelocity,
    int HorizontalDelta,
    bool WrapX);
