using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>One cinematic-object definition, initializer result, and translated motion policy.</summary>
[IndependentFlag("independent authored actor facts: horizontal wrap and whether the actor completes the scene")]
internal readonly record struct CeresDestructionActorDefinition(
    CinematicSpritePreInstruction ActivePreInstruction,
    CinematicSpriteProgram Program,
    Position16<ScreenPixels> Position,
    int Attributes,
    int HorizontalDelta,
    bool WrapX,
    int SlideAcceleration,
    CinematicSpritePreInstruction? SlidePreInstruction,
    bool CompletesScene);
