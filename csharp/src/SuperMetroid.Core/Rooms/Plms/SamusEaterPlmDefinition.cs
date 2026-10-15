namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One Samus Eater PLM header paired with its first program and mounting direction.</summary>
internal readonly record struct SamusEaterPlmDefinition(
    SamusEaterProgram Program,
    bool Ceiling);
