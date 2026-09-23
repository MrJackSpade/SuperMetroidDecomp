using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Writes a room-ID lookup for artists. This is an import-time guide, not a runtime
/// source of room mechanics: shared source artwork retains one installed file and
/// the native room-state selector and background transfer lists remain code-owned.
/// </summary>
public static class RoomArtIndexFiles
{
    public const string FileName = "room-art-index.json";
    private const int Version = 1;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public static RoomArtIndex Extract(string contentDirectory)
    {
        Directory.CreateDirectory(contentDirectory);
        RoomArtEntry[] rooms = RoomHeaderDefinitions.All.Select(header =>
        {
            RoomArtStateEntry[] states = RoomStateSelectionDefinitions
                .GetStatePointers(header.Pointer)
                .Select((pointer, index) => CreateState(index, RoomStateDefinitions.Get(pointer)))
                .ToArray();
            return new RoomArtEntry($"{(byte)header.AreaIndex:X2}/{header.RoomIndex:X2}",
                header.AreaIndex.ToString(), states);
        }).ToArray();
        var index = new RoomArtIndex(Version, SupportedCartridge.Sha256,
            $"{GameInstallationLayout.RoomCharacterDirectoryName}/{RoomCharacterAtlasFormat.CreFileName}",
            $"{GameInstallationLayout.RoomMetatileDirectoryName}/{RoomMetatileFormat.CreFileName}",
            Enumerable.Range(0, RoomSkyTilemapFormat.PageCount).Select(page =>
                $"{GameInstallationLayout.RoomBackgroundTilemapDirectoryName}/" +
                RoomSkyTilemapFormat.FileName(page)).ToArray(),
            rooms);
        File.WriteAllText(Path.Combine(contentDirectory, FileName),
            JsonSerializer.Serialize(index, JsonOptions));
        return index;
    }

    public static RoomArtIndex Load(string contentDirectory)
    {
        string path = Path.Combine(contentDirectory, FileName);
        RoomArtIndex index = JsonSerializer.Deserialize<RoomArtIndex>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidDataException("Installed room-art index is empty.");
        if (index.Version != Version || index.SourceCartridgeSha256 != SupportedCartridge.Sha256 ||
            index.Rooms is null || index.Rooms.Length != RoomHeaderDefinitions.RetailRoomCount)
            throw new InvalidDataException("Installed room-art index is incomplete or incompatible.");
        return index;
    }

    private static RoomArtStateEntry CreateState(int index, CartridgeRoomState state)
    {
        TilesetDefinition tileset = RoomTilesetDefinitions.Get(state.GraphicsSet);
        string[] backgroundFiles = unchecked((short)state.BackgroundDataPointer) < 0
            ? LibraryBackgroundProgramDefinitions.Get(state.BackgroundDataPointer)
                .Instructions.SelectMany(BackgroundFiles).Distinct()
                .Order(StringComparer.Ordinal).ToArray()
            : [];
        return new RoomArtStateEntry(index == 0 ? "default" : $"alternate-{index:D2}",
            $"{GameInstallationLayout.RoomCharacterDirectoryName}/" +
                RoomCharacterAtlasFormat.SourceFileName(tileset.CharacterAddress),
            $"{GameInstallationLayout.RoomMetatileDirectoryName}/" +
                RoomMetatileFormat.SourceFileName(tileset.BlockDefinitionsAddress),
            $"{GameInstallationLayout.RoomPaletteDirectoryName}/" +
                RoomStaticPaletteFormat.SourceFileName(tileset.PaletteAddress),
            backgroundFiles);
    }

    private static IEnumerable<string> BackgroundFiles(LibraryBackgroundInstruction source)
    {
        if (source.Command is LibraryBackgroundCommand.ClearFxTilemap or
            LibraryBackgroundCommand.ClearBg2 or LibraryBackgroundCommand.ClearBg2ForKraid)
            yield break;
        if (source.Command == LibraryBackgroundCommand.DecompressToWorkRam)
        {
            if (!RoomBackgroundTilemapSources.Contains(source.SourceAddress))
                throw new InvalidDataException($"Uncatalogued room background ${source.SourceAddress:X6}.");
            yield return $"{GameInstallationLayout.RoomBackgroundTilemapDirectoryName}/" +
                RoomBackgroundTilemapFormat.SourceFileName(source.SourceAddress);
            yield break;
        }
        if (source.Command is not (LibraryBackgroundCommand.TransferToVram or
            LibraryBackgroundCommand.TransferToVramForKraid or
            LibraryBackgroundCommand.TransferForDoor))
            throw new InvalidDataException(
                $"Uncatalogued library-background command {source.Command}.");

        int offset = source.SourceAddress - RoomSkyTilemapFormat.FirstSourceAddress;
        int byteCount = source.ByteCount;
        // WRAM uploads consume the decompressed tilemap named by the preceding
        // command; they are not a second editable source file.
        if ((source.SourceAddress >> 16) is 0x7e or 0x7f)
            yield break;
        if (offset >= 0 && byteCount > 0 &&
            offset + byteCount <= RoomSkyTilemapFormat.TotalByteCount)
        {
            int firstPage = offset / RoomSkyTilemapFormat.PageByteCount;
            int lastPage = (offset + byteCount - 1) / RoomSkyTilemapFormat.PageByteCount;
            for (int page = firstPage; page <= lastPage; page++)
                yield return $"{GameInstallationLayout.RoomBackgroundTilemapDirectoryName}/" +
                    RoomSkyTilemapFormat.FileName(page);
        }
        else if (source.SourceAddress == HudTileAtlasFormat.SourceAddress)
            yield return $"{GameInstallationLayout.MapDirectoryName}/{HudTileAtlasFormat.FileName}";
        else if (source.SourceAddress ==
                 RoomAssetRomData.LibraryBackground.TourianStatueGhost.SourceAddress)
            yield return $"{GameInstallationLayout.RoomCharacterDirectoryName}/" +
                RoomCharacterAtlasFormat.SourceFileName(source.SourceAddress);
        else
            throw new InvalidDataException($"Uncatalogued direct room artwork ${source.SourceAddress:X6}.");
    }
}

/// <summary>Installed, read-only room-to-artwork guide, keyed by logical area/room ID.</summary>
public sealed record RoomArtIndex(int Version, string SourceCartridgeSha256,
    string SharedCharacters, string SharedBlocks, string[] ScrollingSkyArtwork,
    RoomArtEntry[] Rooms);

/// <summary>A logical room and all its cartridge-selectable presentation variants.</summary>
public sealed record RoomArtEntry(string RoomId, string Area, RoomArtStateEntry[] States);

/// <summary>Relative installed-art paths; background files include door-dependent alternatives.</summary>
public sealed record RoomArtStateEntry(string Variant, string Characters, string Blocks,
    string Palette, string[] BackgroundArtwork);
