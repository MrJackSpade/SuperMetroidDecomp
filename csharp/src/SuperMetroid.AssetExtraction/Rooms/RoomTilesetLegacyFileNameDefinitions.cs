using System.Collections.Frozen;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The names room palette overrides had before #1363, which carried their cartridge source.</summary>
internal static class RoomTilesetLegacyFileNameDefinitions
{
    /// <summary>Each palette's old file name.</summary>
    internal static readonly FrozenDictionary<RoomPaletteSet, string> Palettes = new Dictionary<RoomPaletteSet, string>
    {
        [RoomPaletteSet.UpperCrateria] = "room-palette-C2AD7C.json",
        [RoomPaletteSet.RedCrateria] = "room-palette-C2AE5D.json",
        [RoomPaletteSet.LowerCrateria] = "room-palette-C2AF43.json",
        [RoomPaletteSet.OldTourian] = "room-palette-C2B015.json",
        [RoomPaletteSet.WreckedShipPowerOn] = "room-palette-C2B0E7.json",
        [RoomPaletteSet.WreckedShipPowerOff] = "room-palette-C2B1A6.json",
        [RoomPaletteSet.GreenBlueBrinstar] = "room-palette-C2B264.json",
        [RoomPaletteSet.RedBrinstarKraid] = "room-palette-C2B35F.json",
        [RoomPaletteSet.StatuesHall] = "room-palette-C2B447.json",
        [RoomPaletteSet.HeatedNorfair] = "room-palette-C2B5E4.json",
        [RoomPaletteSet.UnheatedNorfair] = "room-palette-C2B6BB.json",
        [RoomPaletteSet.SandlessMaridia] = "room-palette-C2B83C.json",
        [RoomPaletteSet.SandyMaridia] = "room-palette-C2B92E.json",
        [RoomPaletteSet.Tourian] = "room-palette-C2BAED.json",
        [RoomPaletteSet.MotherBrain] = "room-palette-C2BBC1.json",
        [RoomPaletteSet.BlueCeres] = "room-palette-C2C104.json",
        [RoomPaletteSet.WhiteCeres] = "room-palette-C2C1E3.json",
        [RoomPaletteSet.MapStatues] = "room-palette-C2BC9C.json",
        [RoomPaletteSet.WreckedShipMapPowerOff] = "room-palette-C2BD7B.json",
        [RoomPaletteSet.BlueRefill] = "room-palette-C2BE58.json",
        [RoomPaletteSet.YellowRefill] = "room-palette-C2BF3D.json",
        [RoomPaletteSet.SaveStation] = "room-palette-C2C021.json",
        [RoomPaletteSet.Kraid] = "room-palette-C2B510.json",
        [RoomPaletteSet.Crocomire] = "room-palette-C2B798.json",
        [RoomPaletteSet.Draygon] = "room-palette-C2BA2C.json",
    }.ToFrozenDictionary();
}
