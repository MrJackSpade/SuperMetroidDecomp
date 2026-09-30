using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Required title artwork identities selected by compiled title-card behavior.</summary>
public static class TitleSpriteDefinitions
{
    private static readonly ushort[] Pointers = CollectPointers();

    /// <summary>
    /// The 31 bank-$8C spritemaps selected by the four bank-$8B title-card lists,
    /// the logo and the Nintendo copyright. Blank timed entries are not artwork.
    /// These identities come from compiled selectors, never from a cartridge reader
    /// or a replacement's own manifest.
    /// </summary>
    public static ReadOnlySpan<ushort> NativePointers => Pointers;

    private static ushort[] CollectPointers()
    {
        var pointers = new HashSet<ushort>
        {
            TitleSequenceRomData.Sprites.SuperMetroidLogo,
            TitleSequenceRomData.Sprites.NintendoCopyright,
        };
        foreach (TitleTextSequenceDefinition sequence in new[]
        {
            TitleSequenceRomData.TextSequences.Year,
            TitleSequenceRomData.TextSequences.Nintendo,
            TitleSequenceRomData.TextSequences.Presents,
            TitleSequenceRomData.TextSequences.MetroidThree,
        })
        {
            int entry = sequence.InstructionAddress;
            while ((TitleSequenceInstructionDefinitions.ReadWord(entry) &
                    TitleSequenceRomData.TextSequences.CommandBit) == 0)
            {
                ushort pointer = TitleSequenceInstructionDefinitions.ReadWord(entry + sizeof(ushort));
                if (pointer != TitleSequenceRomData.Sprites.Blank)
                    pointers.Add(pointer);
                entry += TitleSequenceRomData.TextSequences.TimedEntryByteCount;
            }
        }
        return pointers.Order().ToArray();
    }
}
