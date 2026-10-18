using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>One Ceres cinematic actor definition plus its fixed initializer result.</summary>
internal readonly record struct CeresFlightActorDefinition(
    CinematicSpritePreInstruction ActivePreInstruction,
    CinematicSpriteProgram Program,
    Position16<ScreenPixels> Position,
    ushort Attributes,
    ushort InitialTimer,
    int HorizontalDelta,
    bool WrapX);
