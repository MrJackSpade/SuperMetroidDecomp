namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Fixed bank-$8B instruction programs for the four intro Rinkas and their
/// invisible two-wave spawner. The referenced bank-$8C art is separate.
/// </summary>
internal static class IntroRinkaInstructionDefinitions
{
    /// <summary>$8B:CDEB, first Rinka frame in the actor instruction program.</summary>
    internal const ushort StartPointer = CinematicCodePointers.Lists.IntroRinka;
    /// <summary>$8B:CE0D, first wait in the invisible spawn program.</summary>
    internal const ushort SpawnerPointer = CinematicCodePointers.Lists.IntroRinkaSpawner;
    /// <summary>$8B:CE1B, exclusive end after the spawner's delete opcode.</summary>
    internal const ushort EndPointer = 0xce1b;

    private static ReadOnlySpan<byte> Programs =>
    [
        0x0a, 0x00, 0x8d, 0x8c, 0x0a, 0x00, 0xa3, 0x8c,
        0x0a, 0x00, 0xb9, 0x8c, 0xc5, 0xb8, 0x0a, 0x00,
        0xa3, 0x8c, 0x0a, 0x00, 0x8d, 0x8c, 0x0a, 0x00,
        0xa3, 0x8c, 0x0a, 0x00, 0xb9, 0x8c, 0xbc, 0x94,
        0xf9, 0xcd, 0x4a, 0x00, 0x00, 0x00, 0x21, 0xba,
        0x80, 0x00, 0x00, 0x00, 0x36, 0xba, 0x38, 0x94,
    ];

    internal static byte ReadByte(ushort pointer)
    {
        if (pointer < StartPointer || pointer >= EndPointer)
            throw new ArgumentOutOfRangeException(nameof(pointer));
        return Programs[pointer - StartPointer];
    }

    internal static ushort ReadWord(ushort pointer)
    {
        if (pointer < StartPointer || pointer >= EndPointer - 1)
            throw new InvalidDataException(
                $"Intro Rinka instruction read $8B:{pointer:X4} leaves its compiled programs.");
        int offset = pointer - StartPointer;
        return (ushort)(Programs[offset] | Programs[offset + 1] << 8);
    }
}
