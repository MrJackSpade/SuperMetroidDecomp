namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One Samus Eater PLM header paired with its initial list and mounting direction.</summary>
internal readonly record struct SamusEaterPlmDefinition(
    ushort InstructionListPointer,
    bool Ceiling);
