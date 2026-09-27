namespace SuperMetroid.Core.Assets;

/// <summary>One contiguous source sheet used by a fixed Mother Brain OBJ transfer list.</summary>
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
    /// <summary>$B1:8800-$8FFF, four Baby Metroid pages loaded by $A9:8FE5.</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition BabyMetroid =
        new("mother-brain-baby-tiles.png", 0xb18800, 4, 0x7c00);

    /// <summary>$B7:A000-$A7FF, four Mother Brain attack pages restored by $A9:8FC7.</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition Attack =
        new("mother-brain-attack-tiles.png", 0xb7a000, 4, 0x7c00);

    /// <summary>$AB:F400-$F7FF, the two exploded escape-door pages from $A9:902F.</summary>
    public static readonly MotherBrainSpecialSpriteSheetDefinition ExplodedDoor =
        new("mother-brain-exploded-door-tiles.png", 0xabf400, 2, 0x7000);

    /// <summary>All three separate editable source sheets.</summary>
    public static IReadOnlyList<MotherBrainSpecialSpriteSheetDefinition> All { get; } =
        Array.AsReadOnly([BabyMetroid, Attack, ExplodedDoor]);

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
}
