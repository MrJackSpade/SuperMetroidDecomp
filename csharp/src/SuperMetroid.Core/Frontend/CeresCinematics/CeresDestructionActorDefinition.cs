using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>One cinematic-object definition, initializer result, and translated motion policy.</summary>
[IndependentFlag("independent authored actor facts: horizontal wrap and whether the actor completes the scene")]
internal readonly record struct CeresDestructionActorDefinition(
    CinematicSpriteFrameBehavior ActiveFrameBehavior,
    CinematicSpriteProgram Program,
    Position16<ScreenPixels> Position,
    CeresSpriteColouring Colouring,
    int HorizontalDelta,
    bool WrapX,
    int SlideAcceleration,
    CinematicSpriteFrameBehavior? SlideFrameBehavior,
    bool CompletesScene);
