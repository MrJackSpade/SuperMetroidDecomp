namespace SuperMetroid.Core.Game;

/// <summary>One compiled command from Kraid's four private bank-$A7 head programs.</summary>
internal readonly record struct KraidHeadInstructionDefinition(
    ushort Pointer,
    KraidHeadInstructionKind Kind,
    ushort Duration,
    ushort Tilemap,
    ushort VulnerableHitbox,
    ushort InvulnerableHitbox,
    ushort SoundId);
