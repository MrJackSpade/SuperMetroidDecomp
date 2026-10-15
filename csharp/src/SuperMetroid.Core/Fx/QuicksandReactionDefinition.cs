namespace SuperMetroid.Core.Fx;

/// <summary>One quicksand PLM header paired with its setup routine and initial list.</summary>
public readonly record struct QuicksandReactionDefinition(
    SpecialAirReactionSetup SetupPointer,
    ushort InstructionListPointer);
