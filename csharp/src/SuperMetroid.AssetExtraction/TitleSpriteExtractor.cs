using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Decodes the ordered bank-$8C parts selected by the four native title text lists.</summary>
internal static class TitleSpriteExtractor
{
    public static TitleSpriteFrame[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
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
            while (true)
            {
                ushort durationOrCommand = RomDataReader.ReadWordFixedBank(bus, entry);
                if ((durationOrCommand & TitleSequenceRomData.TextSequences.CommandBit) != 0)
                    break;
                ushort pointer = RomDataReader.ReadWordFixedBank(bus, entry + 2);
                if (pointer != TitleSequenceRomData.Sprites.Blank)
                    pointers.Add(pointer);
                entry += TitleSequenceRomData.TextSequences.TimedEntryByteCount;
                if ((entry & 0xffff) >= TitleSequenceRomData.Sprites.LogoPointerAddress % 0x10000)
                    throw new InvalidDataException("Title text list did not terminate before the logo pointer.");
            }
        }

        return pointers.Order().Select(pointer => new TitleSpriteFrame
        {
            Pointer = pointer,
            Parts = ReadParts(bus, pointer),
        }).ToArray();
    }

    private static SpriteVisualPart[] ReadParts(ISnesAddressSpace bus, ushort pointer)
    {
        int address = (TitleSequenceRomData.Sprites.Bank << 16) | pointer;
        int count = RomDataReader.ReadWordFixedBank(bus, address);
        if (count > TitleGraphicsFormat.MaximumSpriteParts)
            throw new InvalidDataException($"Title sprite $8C:{pointer:X4} exceeds OAM capacity.");
        var parts = new SpriteVisualPart[count];
        for (int index = 0; index < count; index++)
        {
            int source = address + 2 + index * 5;
            var x = new SnesSpritemapXWord(RomDataReader.ReadWordFixedBank(bus, source));
            var attributes = new SnesObjAttributeWord(RomDataReader.ReadWordFixedBank(bus, source + 3));
            parts[index] = new SpriteVisualPart
            {
                OffsetX = x.SignedOffset,
                OffsetY = unchecked((sbyte)bus.ReadByte(source + 2)),
                TileColumn = attributes.TileNumber % TitleGraphicsFormat.ObjectTileColumns,
                TileRow = attributes.TileNumber / TitleGraphicsFormat.ObjectTileColumns,
                Size = x.IsLarge ? 16 : 8,
                Priority = attributes.Priority,
                Palette = null,
                FlipX = attributes.FlipHorizontally,
                FlipY = attributes.FlipVertically,
            };
        }
        return parts;
    }
}
