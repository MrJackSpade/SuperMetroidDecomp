using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Required title artwork identities selected by compiled title-card behavior.</summary>
public static class TitleSpriteDefinitions
{
    /// <summary>
    /// Enumerates the 31 distinct bank-$8C identities in native address order from the
    /// calculated title-card layouts. Blank timed entries are not artwork.
    /// </summary>
    public static IEnumerable<ushort> NativePointers
    {
        get
        {
            yield return TitleSequenceRomData.Sprites.NintendoCopyright;
            for (int frame = 3; frame < 9; frame++)
                yield return Frame(TitleSequenceRomData.TextSequences.MetroidThree, frame);
            yield return TitleSequenceRomData.Sprites.SuperMetroidLogo;
            for (int frame = 1; frame <= 4; frame++)
                yield return Frame(TitleSequenceRomData.TextSequences.Year, frame);
            for (int frame = 0; frame < 8; frame++)
                yield return Frame(TitleSequenceRomData.TextSequences.Nintendo, frame);
            for (int frame = 0; frame < 8; frame++)
                yield return Frame(TitleSequenceRomData.TextSequences.Presents, frame);
            for (int frame = 0; frame < 3; frame++)
                yield return Frame(TitleSequenceRomData.TextSequences.MetroidThree, frame);
        }
    }

    /// <summary>Reads the bank-$8C spritemap pointer stored in one timed text-list entry.</summary>
    /// <param name="sequence">Text-list definition whose instruction address begins the timed entries.</param>
    /// <param name="frame">Zero-based timed-entry index in that list.</param>
    /// <returns>The spritemap pointer from the entry, following its duration word.</returns>
    private static ushort Frame(TitleTextSequenceDefinition sequence, int frame) =>
        TitleSequenceInstructionDefinitions.ReadWord(sequence.InstructionAddress +
            frame * TitleSequenceRomData.TextSequences.TimedEntryByteCount + sizeof(ushort));
}
