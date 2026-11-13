using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>One cinematic-object definition, initializer result, and motion policy; where it starts is installed (#1615).</summary>
[IndependentFlag("independent authored actor facts: horizontal wrap and whether the actor completes the scene")]
internal readonly record struct CeresDestructionActorDefinition(
    CinematicSpriteFrameBehavior ActiveFrameBehavior,
    CinematicSpriteSegment Animation,
    CeresSpriteColouring Colouring,
    SignedSixteenSixteen HorizontalDelta,
    bool WrapX,
    SignedEightEight SlideAcceleration,
    CinematicSpriteFrameBehavior? SlideFrameBehavior,
    bool CompletesScene);
