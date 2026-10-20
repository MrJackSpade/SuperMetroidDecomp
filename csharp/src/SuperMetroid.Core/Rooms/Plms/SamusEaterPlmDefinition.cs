namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One Samus Eater PLM header paired with its first program and mounting direction.</summary>
[IndependentFlag("whether the eater hangs from the ceiling, a fact of the PLM definition")]
internal readonly record struct SamusEaterPlmDefinition(
    SamusEaterProgram Program,
    bool Ceiling);
