namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One Ceres cinematic actor definition plus its fixed initializer result.</summary>
internal readonly record struct CeresFlightActorDefinition(
    ushort ActivePreInstruction,
    ushort InstructionList,
    Position16<ScreenPixels> Position,
    ushort Attributes,
    ushort InitialTimer,
    int HorizontalDelta,
    bool WrapX);
