using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>One contiguous source sheet used by a fixed Mother Brain OBJ transfer list.</summary>
/// <param name="FileName">Installed artwork filename that supplies the sheet's tile bytes.</param>
/// <param name="SourceAddress">First cartridge address occupied by this contiguous sheet.</param>
/// <param name="PageCount">Number of native $200-byte transfer pages in the sheet.</param>
/// <param name="FirstDestinationWord">OBJ VRAM word address receiving the first page.</param>
public readonly record struct MotherBrainSpecialSpriteSheetDefinition(
    string FileName, int SourceAddress, int PageCount, ushort FirstDestinationWord)
{
    /// <summary>Every native record copies one $200-byte, sixteen-tile page.</summary>
    public const ushort PageByteCount = 0x0200;

    /// <summary>Each page occupies $100 words in OBJ VRAM.</summary>
    public const ushort DestinationWordStride = 0x0100;

    /// <summary>Total tile-aligned source length encoded by this sheet.</summary>
    public int ByteCount => checked(PageCount * PageByteCount);

    /// <summary>Whether a transfer starts inside this source sheet.</summary>
    public bool ContainsSource(uint sourceAddress) =>
        sourceAddress >= SourceAddress && sourceAddress < SourceAddress + ByteCount;

    /// <summary>Calculates a page's native OBJ transfer, including its list-relative entry index.</summary>
    public MotherBrainSpriteTileTransferRequest Transfer(int page) => (uint)page < PageCount
        ? new((ushort)page, PageByteCount, (uint)(SourceAddress + page * PageByteCount),
            (ushort)(FirstDestinationWord + page * DestinationWordStride))
        : throw new IndexOutOfRangeException();

    /// <summary>Checks the cartridge's page alignment, size and destination.</summary>
    public int ResolvePage(uint sourceAddress, ushort byteCount, ushort destinationWord)
    {
        if (!ContainsSource(sourceAddress))
            throw new InvalidDataException(
                $"Source ${sourceAddress:X6} does not belong to {FileName}.");
        int offset = checked((int)sourceAddress - SourceAddress);
        int page = offset / PageByteCount;
        if (offset != page * PageByteCount || byteCount != PageByteCount ||
            destinationWord != FirstDestinationWord + page * DestinationWordStride)
            throw new InvalidDataException(
                $"Invalid Mother Brain sprite transfer for {FileName}: " +
                $"source ${sourceAddress:X6}, size ${byteCount:X4}, destination ${destinationWord:X4}.");
        return page;
    }
}

/// <summary>Native visual sources and destinations of the three fixed Mother Brain lists.</summary>
public static class MotherBrainSpecialSpriteArtworkDefinitions
{
    /// <summary>$B7:9000-$9FFF, eight phase-two leg pages loaded by $A9:8F8F.</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition Legs =
        new("mother-brain-leg-tiles.png", 0xb79000, 8, 0x7400);

    /// <summary>$B1:8800-$8FFF, four Baby Metroid pages loaded by $A9:8FE5.</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition BabyMetroid =
        new("mother-brain-baby-tiles.png", 0xb18800, 4, 0x7c00);

    /// <summary>$B7:A000-$A7FF, four Mother Brain attack pages restored by $A9:8FC7.</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition Attack =
        new("mother-brain-attack-tiles.png", 0xb7a000, 4, 0x7c00);

    /// <summary>$AB:F400-$F7FF, the two exploded escape-door pages from $A9:902F.</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition ExplodedDoor =
        new("mother-brain-exploded-door-tiles.png", 0xabf400, 2, 0x7000);

    /// <summary>All four separate editable source sheets.</summary>
    public static IReadOnlyList<MotherBrainSpecialSpriteSheetDefinition> All { get; } =
        new SheetList();

    /// <summary>Looks up the sheet containing one native source address.</summary>
    public static bool TryForSource(uint sourceAddress,
        out MotherBrainSpecialSpriteSheetDefinition definition)
    {
        foreach (MotherBrainSpecialSpriteSheetDefinition sheet in All)
        {
            if (!sheet.ContainsSource(sourceAddress)) continue;
            definition = sheet;
            return true;
        }
        definition = default;
        return false;
    }

    /// <summary>Provides indexed access to the four fixed transfer sheets in their catalog order.</summary>
    private sealed class SheetList : IReadOnlyList<MotherBrainSpecialSpriteSheetDefinition>
    {
        /// <summary>Number of fixed Mother Brain transfer sheets.</summary>
        public int Count => 4;

        /// <summary>Gets a sheet by its catalog position.</summary>
        /// <param name="index">Zero-based index ordered as legs, Baby Metroid, attack, then exploded door.</param>
        /// <returns>The sheet definition at that position.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The index is outside the four-sheet catalog.</exception>
        public MotherBrainSpecialSpriteSheetDefinition this[int index] => index switch
        {
            0 => Legs,
            1 => BabyMetroid,
            2 => Attack,
            3 => ExplodedDoor,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };

        /// <summary>Enumerates the four sheet definitions in catalog order.</summary>
        /// <returns>An iterator over the legs, Baby Metroid, attack, and exploded-door sheets.</returns>
        public IEnumerator<MotherBrainSpecialSpriteSheetDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Returns a non-generic enumerator over this sequence.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
