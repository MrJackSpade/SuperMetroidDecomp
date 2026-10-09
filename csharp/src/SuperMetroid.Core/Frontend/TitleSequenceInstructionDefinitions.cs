namespace SuperMetroid.Core.Frontend;

/// <summary>Bank-$8B title-card timing, scene triggers and calculated progressive text selectors.</summary>
internal static class TitleSequenceInstructionDefinitions
{
    /// <summary>$8B:A03D, first Year-card duration word.</summary>
    internal const int StartAddress = 0x8ba03d;
    /// <summary>$8B:A0C9, exclusive end after the title-logo pointer.</summary>
    internal const int EndAddress = 0x8ba0c9;
    /// <summary><c>TitleSequenceSpritemaps_1</c> at $8C:8862; first year letter.</summary>
    private const ushort YearFirstLetter = 0x8862;
    /// <summary><c>TitleSequenceSpritemaps_N</c> at $8C:88CE; first Nintendo letter.</summary>
    private const ushort NintendoFirstLetter = 0x88ce;
    /// <summary><c>TitleSequenceSpritemaps_P</c> at $8C:8A46; first Presents letter.</summary>
    private const ushort PresentsFirstLetter = 0x8a46;
    /// <summary><c>TitleSequenceSpritemaps_M</c> at $8C:8BBE; first three Metroid letters.</summary>
    private const ushort MetroidFirstLetter = 0x8bbe;
    /// <summary><c>TitleSequenceSpritemaps_METR</c> at $8C:85C8; four/five-letter lists precede unused debug copyright artwork.</summary>
    private const ushort MetroidFourLetters = 0x85c8;
    /// <summary><c>TitleSequenceSpritemaps_METROI</c> at $8C:867D; final lists follow the debug copyright.</summary>
    private const ushort MetroidSixLetters = 0x867d;

    internal static byte ReadByte(int address)
    {
        if (address is < StartAddress or >= EndAddress)
            throw new InvalidDataException($"Title-card byte ${address:X6} leaves its compiled program.");
        int offset = address - StartAddress;
        return (byte)(WordAt(offset & ~1) >> ((offset & 1) * 8));
    }

    internal static ushort ReadWord(int address)
    {
        if (address is < StartAddress or >= (EndAddress - 1))
            throw new InvalidDataException($"Title-card word ${address:X6} leaves its compiled program.");
        return (ushort)(ReadByte(address) | ReadByte(address + 1) << 8);
    }

    // Initial/reveal/final/logo holds 60/8/45/120/32 are the title card's authored timing (see residualScalarInputsReview).
    private static ushort WordAt(int offset)
    {
        if (offset < 4) return (ushort)(offset == 0 ? 60 : 0);
        if (offset < 24) return CardWord(offset - 4, 4, 45, YearFirstLetter,
            CinematicCodePointers.Instruction_TriggerTitleSequenceScene0);
        if (offset < 60) return CardWord(offset - 24, 8, 45, NintendoFirstLetter,
            CinematicCodePointers.Instruction_TriggerTitleSequenceScene1);
        if (offset < 96) return CardWord(offset - 60, 8, 45, PresentsFirstLetter,
            CinematicCodePointers.Instruction_TriggerTitleSequenceScene2);
        if (offset < 136) return CardWord(offset - 96, 9, 120, MetroidFirstLetter,
            CinematicCodePointers.Instruction_TriggerTitleSequenceScene3);
        return offset == 136 ? (ushort)32 : TitleSequenceRomData.Sprites.SuperMetroidLogo;
    }

    private static ushort CardWord(int offset, int frames, ushort finalHold, ushort firstLetter, ushort sceneCommand)
    {
        if (offset == frames * 4) return sceneCommand;
        if (offset == frames * 4 + 2) return CinematicCodePointers.CinematicSpriteObject_Instruction_Delete;
        int frame = offset / 4;
        if ((offset & 2) == 0) return frame == frames - 1 ? finalHold : (ushort)8;
        if (firstLetter != MetroidFirstLetter || frame < 3)
            return ProgressiveFrame(firstLetter, frame, firstLetterCount: 1);
        if (frame < 5) return ProgressiveFrame(MetroidFourLetters, frame - 3, firstLetterCount: 4);
        // The penultimate reveal adds a blank space, so its map has no extra letter objects.
        return (ushort)(ProgressiveFrame(MetroidSixLetters, frame - 5, firstLetterCount: 6) - (frame == 8 ? 10 : 0));
    }

    private static ushort ProgressiveFrame(ushort start, int frame, int firstLetterCount) =>
        // Each list has a two-byte count and two five-byte OAM objects per visible letter.
        (ushort)(start + frame * 2 + 10 * (frame * firstLetterCount + frame * (frame - 1) / 2));
}
