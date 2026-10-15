using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>One cinematic-object definition, initializer result, and translated motion policy.</summary>
internal readonly record struct CeresDestructionActorDefinition(
    ushort ActivePreInstruction,
    CinematicSpriteProgram Program,
    Position16<ScreenPixels> Position,
    ushort Attributes,
    int HorizontalDelta,
    bool WrapX,
    ushort SlideAcceleration,
    ushort SlidePreInstruction,
    bool CompletesScene);
