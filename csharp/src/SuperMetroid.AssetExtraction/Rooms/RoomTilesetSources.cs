using System.Collections.Frozen;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Where each room character sheet, block table and palette is stored on the cartridge, and the content identities the
/// installation records for them, which hash each sheet with its source address.
/// </summary>
internal static class RoomTilesetSources
{
    /// <summary>The compressed source of each room character sheet; the Tourian statue ghost is stored uncompressed.</summary>
    internal static readonly FrozenDictionary<RoomCharacterSet, int> Characters =
        new Dictionary<RoomCharacterSet, int>
        {
            [RoomCharacterSet.UpperCrateria] = 0xbac629,
            [RoomCharacterSet.LowerCrateria] = 0xbaf911,
            [RoomCharacterSet.WreckedShip] = 0xbbae9e,
            [RoomCharacterSet.GreenBlueBrinstar] = 0xbbe6b0,
            [RoomCharacterSet.RedBrinstarKraidStatuesHall] = 0xbca5aa,
            [RoomCharacterSet.Norfair] = 0xbdc3f9,
            [RoomCharacterSet.SandlessMaridia] = 0xbeb130,
            [RoomCharacterSet.SandyMaridia] = 0xbee78d,
            [RoomCharacterSet.Tourian] = 0xbfd414,
            [RoomCharacterSet.Ceres] = 0xc0b004,
            [RoomCharacterSet.CeresElevator] = 0xc0e22a,
            [RoomCharacterSet.CeresRidley] = 0xc18da9,
            [RoomCharacterSet.UtilityRoomStatues] = 0xc0860b,
            [RoomCharacterSet.Kraid] = 0xbcdff0,
            [RoomCharacterSet.Crocomire] = 0xbdfe2a,
            [RoomCharacterSet.Draygon] = 0xbf9dea,
            [RoomCharacterSet.TourianStatueGhost] = RoomAssetRomData.LibraryBackground.TourianStatueGhost.SourceAddress,
        }.ToFrozenDictionary();

    /// <summary>The compressed source of each room block table.</summary>
    internal static readonly FrozenDictionary<RoomBlockSet, int> Blocks =
        new Dictionary<RoomBlockSet, int>
        {
            [RoomBlockSet.UpperCrateria] = 0xc1b6f6,
            [RoomBlockSet.LowerCrateria] = 0xc1beee,
            [RoomBlockSet.WreckedShip] = 0xc1c5cf,
            [RoomBlockSet.GreenBlueBrinstar] = 0xc1cfa6,
            [RoomBlockSet.RedBrinstarKraidStatuesHall] = 0xc1d8dc,
            [RoomBlockSet.Norfair] = 0xc1e361,
            [RoomBlockSet.SandlessMaridia] = 0xc1f4b1,
            [RoomBlockSet.SandyMaridia] = 0xc2855f,
            [RoomBlockSet.Tourian] = 0xc29b01,
            [RoomBlockSet.Ceres] = 0xc2a75e,
            [RoomBlockSet.UtilityRoomStatues] = 0xc2a27b,
            [RoomBlockSet.Kraid] = 0xc1e189,
            [RoomBlockSet.Crocomire] = 0xc1f3af,
            [RoomBlockSet.Draygon] = 0xc2960d,
        }.ToFrozenDictionary();

    /// <summary>The compressed source of each room palette.</summary>
    internal static readonly FrozenDictionary<RoomPaletteSet, int> Palettes =
        new Dictionary<RoomPaletteSet, int>
        {
            [RoomPaletteSet.UpperCrateria] = 0xc2ad7c,
            [RoomPaletteSet.RedCrateria] = 0xc2ae5d,
            [RoomPaletteSet.LowerCrateria] = 0xc2af43,
            [RoomPaletteSet.OldTourian] = 0xc2b015,
            [RoomPaletteSet.WreckedShipPowerOn] = 0xc2b0e7,
            [RoomPaletteSet.WreckedShipPowerOff] = 0xc2b1a6,
            [RoomPaletteSet.GreenBlueBrinstar] = 0xc2b264,
            [RoomPaletteSet.RedBrinstarKraid] = 0xc2b35f,
            [RoomPaletteSet.StatuesHall] = 0xc2b447,
            [RoomPaletteSet.HeatedNorfair] = 0xc2b5e4,
            [RoomPaletteSet.UnheatedNorfair] = 0xc2b6bb,
            [RoomPaletteSet.SandlessMaridia] = 0xc2b83c,
            [RoomPaletteSet.SandyMaridia] = 0xc2b92e,
            [RoomPaletteSet.Tourian] = 0xc2baed,
            [RoomPaletteSet.MotherBrain] = 0xc2bbc1,
            [RoomPaletteSet.BlueCeres] = 0xc2c104,
            [RoomPaletteSet.WhiteCeres] = 0xc2c1e3,
            [RoomPaletteSet.MapStatues] = 0xc2bc9c,
            [RoomPaletteSet.WreckedShipMapPowerOff] = 0xc2bd7b,
            [RoomPaletteSet.BlueRefill] = 0xc2be58,
            [RoomPaletteSet.YellowRefill] = 0xc2bf3d,
            [RoomPaletteSet.SaveStation] = 0xc2c021,
            [RoomPaletteSet.Kraid] = 0xc2b510,
            [RoomPaletteSet.Crocomire] = 0xc2b798,
            [RoomPaletteSet.Draygon] = 0xc2ba2c,
        }.ToFrozenDictionary();

    /// <summary>The installed content identity of the selected character sheets.</summary>
    internal static string CharacterIdentity(RoomCharacterAtlasCatalog catalog) => SelectedPresentationHash.FromTransfers(
        nameof(RoomCharacterAtlasCatalog), catalog.Sheets.ToDictionary(pair => Characters[pair.Key], pair => pair.Value),
        atlas => atlas.Transfer, catalog.Cre.Transfer);

    /// <summary>The installed content identity of the selected block tables.</summary>
    internal static string BlockIdentity(RoomMetatileCatalog catalog) => SelectedPresentationHash.FromTransfers(
        nameof(RoomMetatileCatalog), catalog.Atlases.ToDictionary(pair => Blocks[pair.Key], pair => pair.Value),
        atlas => atlas.Transfer, catalog.Cre.Transfer);

    /// <summary>The installed content identity of the selected room palettes.</summary>
    internal static string PaletteIdentity(RoomStaticPaletteCatalog catalog) => SelectedPresentationHash.FromTransfers(
        nameof(RoomStaticPaletteCatalog), catalog.Palettes.ToDictionary(pair => Palettes[pair.Key], pair => pair.Value),
        palette => palette.Transfer);
}
