using System.Collections.Frozen;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Where each room character sheet, block table and palette is stored, read from the cartridge's tileset table, and the
/// content identities the installation records for them, which hash each sheet with its set.
/// </summary>
internal static class RoomTilesetSources
{
    /// <summary>
    /// Reads every graphics set's tileset entry. Sets several graphics sets share must name one source; the Tourian
    /// statue ghost, which no tileset names, is the library background's.
    /// </summary>
    /// <exception cref="InvalidDataException">Two graphics sets that share a set name different sources for it.</exception>
    internal static RoomTilesetSourceMap Read(IImportCartridgeSource cartridge)
    {
        Ensure.NotNull(cartridge);
        var characters = new Dictionary<RoomCharacterSet, SnesAddress>();
        var blocks = new Dictionary<RoomBlockSet, SnesAddress>();
        var palettes = new Dictionary<RoomPaletteSet, SnesAddress>();
        SnesAddress table = RoomTilesetSourceDefinitions.TilesetTable;
        for (int graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            TilesetDefinition sets = RoomTilesetDefinitions.Get(graphicsSet);
            var entry = new SnesAddress(table.Bank,
                RomDataReader.ReadWordFixedBank(cartridge, table.AddWithinBank(graphicsSet * sizeof(ushort))));
            Add(blocks, sets.Blocks, Source(cartridge, entry, RoomTilesetSourceDefinitions.BlocksPointerOffset));
            Add(characters, sets.Characters, Source(cartridge, entry, RoomTilesetSourceDefinitions.CharactersPointerOffset));
            Add(palettes, sets.Palette, Source(cartridge, entry, RoomTilesetSourceDefinitions.PalettePointerOffset));
        }
        characters.Add(RoomCharacterSet.TourianStatueGhost,
            SnesAddress.FromBusAddress(RoomBackgroundSourceDefinitions.TourianStatueGhostCharacters));
        return new(characters.ToFrozenDictionary(), blocks.ToFrozenDictionary(), palettes.ToFrozenDictionary());
    }

    /// <summary>The installed content identity of the selected character sheets.</summary>
    internal static string CharacterIdentity(RoomCharacterAtlasCatalog catalog) => SelectedPresentationHash.FromNamedTransfers(
        nameof(RoomCharacterAtlasCatalog), catalog.Sheets.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value),
        atlas => atlas.Characters, catalog.Cre.Characters);

    /// <summary>The installed content identity of the selected block tables.</summary>
    internal static string BlockIdentity(RoomMetatileCatalog catalog) => SelectedPresentationHash.FromNamedTransfers(
        nameof(RoomMetatileCatalog), catalog.Atlases.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value),
        atlas => BackgroundTileWords.ToBytes(atlas.BlockTiles.Span), BackgroundTileWords.ToBytes(catalog.Cre.BlockTiles.Span));

    /// <summary>The installed content identity of the selected room palettes.</summary>
    internal static string PaletteIdentity(RoomStaticPaletteCatalog catalog) => SelectedPresentationHash.FromNamedTransfers(
        nameof(RoomStaticPaletteCatalog), catalog.Palettes.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value),
        palette => palette.PackedColours);

    /// <summary>The long pointer <paramref name="offset"/> bytes into the tileset entry at <paramref name="entry"/>.</summary>
    private static SnesAddress Source(IImportCartridgeSource cartridge, SnesAddress entry, int offset) =>
        SnesAddress.FromBusAddress(RomDataReader.ReadLongFixedBank(cartridge, entry.AddWithinBank(offset)));

    /// <summary>Records <paramref name="set"/>'s source, which every graphics set that shares it must agree on.</summary>
    private static void Add<TSet>(Dictionary<TSet, SnesAddress> sources, TSet set, SnesAddress source) where TSet : struct, Enum
    {
        if (sources.TryGetValue(set, out SnesAddress named) && named != source)
            throw new InvalidDataException($"Graphics sets name both {named} and {source} for {set}.");
        sources[set] = source;
    }
}
