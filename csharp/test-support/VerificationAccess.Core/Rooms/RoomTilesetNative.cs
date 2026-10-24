using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Verification's native view of room tileset artwork: where the retail cartridge stores each sheet, table and palette,
/// from the pinned disassembly's tileset table and library background.
/// </summary>
internal static class RoomTilesetNative
{
    /// <summary>Retail character sheet sources, from the pinned disassembly's tileset table and library background.</summary>
    internal static readonly IReadOnlyDictionary<RoomCharacterSet, int> RetailCharacters = new Dictionary<RoomCharacterSet, int>
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
        [RoomCharacterSet.TourianStatueGhost] = 0x87ad64,
    };

    /// <summary>Retail block table sources.</summary>
    internal static readonly IReadOnlyDictionary<RoomBlockSet, int> RetailBlocks = new Dictionary<RoomBlockSet, int>
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
    };

    /// <summary>Retail palette sources.</summary>
    internal static readonly IReadOnlyDictionary<RoomPaletteSet, int> RetailPalettes = new Dictionary<RoomPaletteSet, int>
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
    };

    /// <summary>A tileset's cartridge sources.</summary>
    extension(TilesetDefinition tileset)
    {
        /// <summary>Where the tileset's block table is stored.</summary>
        internal int BlockDefinitionsAddress => RetailBlocks[tileset.Blocks];

        /// <summary>Where the tileset's character sheet is stored.</summary>
        internal int CharacterAddress => RetailCharacters[tileset.Characters];

        /// <summary>Where the tileset's palette is stored.</summary>
        internal int PaletteAddress => RetailPalettes[tileset.Palette];
    }

    /// <summary>The character sheet stored at <paramref name="address"/>.</summary>
    internal static RoomCharacterSet CharacterSetAt(int address) =>
        RetailCharacters.Single(pair => pair.Value == address).Key;

    /// <summary>The block table stored at <paramref name="address"/>.</summary>
    internal static RoomBlockSet BlockSetAt(int address) =>
        RetailBlocks.Single(pair => pair.Value == address).Key;

    /// <summary>The palette stored at <paramref name="address"/>.</summary>
    internal static RoomPaletteSet PaletteSetAt(int address) =>
        RetailPalettes.Single(pair => pair.Value == address).Key;
}
