using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>One Ceres cinematic actor definition plus its fixed initializer result.</summary>
[IndependentFlag("whether the authored actor wraps horizontally, a fact of the actor data")]
internal readonly record struct CeresFlightActorDefinition(
    CinematicSpritePreInstruction ActivePreInstruction,
    CinematicSpriteProgram Program,
    Position16<ScreenPixels> Position,
    int Attributes,
    int InitialTimer,
    int HorizontalDelta,
    bool WrapX);
