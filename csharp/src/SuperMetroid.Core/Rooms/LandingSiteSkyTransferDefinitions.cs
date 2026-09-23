using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The six command-E door transfers in Landing Site's bank-$8F:B76A library-
/// background list. This is fixed transfer selection, not editable sky artwork;
/// the seven referenced sky pages are installed presentation assets.
/// </summary>
public static class LandingSiteSkyTransferDefinitions
{
    /// <summary>VRAM word destination used by five of the six native transfers.</summary>
    private const ushort MainVramDestination = 0x4800;

    /// <summary>VRAM word destination of the door-$89B2 native transfer.</summary>
    private const ushort AlternateVramDestination = 0x4c00;

    private static readonly LandingSiteSkyTransferDefinition[] definitions =
    [
        new(0x8946, RoomSkyTilemapFormat.SourceAddress(2), MainVramDestination),
        new(0x896a, RoomSkyTilemapFormat.SourceAddress(4), MainVramDestination),
        new(0x89b2, RoomSkyTilemapFormat.SourceAddress(1), AlternateVramDestination),
        new(0x8ac6, RoomSkyTilemapFormat.SourceAddress(4), MainVramDestination),
        new(LandingSiteRomData.LandingCutsceneDoorPointer,
            RoomSkyTilemapFormat.SourceAddress(0), MainVramDestination),
        new(0x890a, RoomSkyTilemapFormat.SourceAddress(2), MainVramDestination),
    ];

    /// <summary>All authored command-E transfers in native list order.</summary>
    public static IReadOnlyList<LandingSiteSkyTransferDefinition> All { get; } =
        Array.AsReadOnly(definitions);

    /// <summary>Native byte span: six eleven-byte command-E records and a two-byte terminator.</summary>
    public static int NativeListByteCount => definitions.Length * 11 + sizeof(ushort);

    /// <summary>Selects the transfer for an entry door; unmatched doors did not occur in the list.</summary>
    public static LandingSiteSkyTransferDefinition Get(ushort doorPointer)
    {
        foreach (LandingSiteSkyTransferDefinition definition in definitions)
        {
            if (definition.DoorPointer == doorPointer)
                return definition;
        }

        throw new InvalidDataException(
            $"Landing Site library background has no command-E record for door $83:{doorPointer:X4}.");
    }
}

/// <summary>One fixed door-selected sky page upload from Landing Site's native list.</summary>
public readonly record struct LandingSiteSkyTransferDefinition(
    ushort DoorPointer,
    int SourceAddress,
    ushort VramDestination)
{
    /// <summary>The native command transfers one complete 32x32 sky tilemap page.</summary>
    public ushort ByteCount => RoomSkyTilemapFormat.PageByteCount;
}
