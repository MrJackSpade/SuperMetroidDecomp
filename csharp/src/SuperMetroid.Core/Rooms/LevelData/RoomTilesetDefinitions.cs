namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Native graphics-theme dispatch to independently installed room artwork resources.</summary>
public static class RoomTilesetDefinitions
{
    /// <summary>$8F:E7A7..E7DF: graphics themes $00..$1C precede unrelated data.</summary>
    public const int Count = 0x1d;

    /// <summary>Twenty-nine retail graphics-set selectors from the bank-$8F room tileset table.</summary>
    private enum Theme
    {
        /// <summary>$8F:E6A2, <c>Tileset_Table_0_UpperCrateria</c>, graphics-set selector $00.</summary>
        UpperCrateria,
        /// <summary>$8F:E6AB, <c>Tileset_Table_1_RedCrateria</c>, graphics-set selector $01.</summary>
        RedCrateria,
        /// <summary>$8F:E6B4, <c>Tileset_Table_2_LowerCrateria</c>, graphics-set selector $02.</summary>
        LowerCrateria,
        /// <summary>$8F:E6BD, <c>Tileset_Table_3_OldTourian</c>, graphics-set selector $03.</summary>
        OldTourian,
        /// <summary>$8F:E6C6, <c>Tileset_Table_4_WreckedShip_PowerOn</c>, graphics-set selector $04.</summary>
        WreckedShip_PowerOn,
        /// <summary>$8F:E6CF, <c>Tileset_Table_5_WreckedShip_PowerOff</c>, graphics-set selector $05.</summary>
        WreckedShip_PowerOff,
        /// <summary>$8F:E6D8, <c>Tileset_Table_6_GreenBlueBrinstar</c>, graphics-set selector $06.</summary>
        GreenBlueBrinstar,
        /// <summary>$8F:E6E1, <c>Tileset_Table_7_RedBrinstar_Kraid</c>, graphics-set selector $07.</summary>
        RedBrinstar_Kraid,
        /// <summary>$8F:E6EA, <c>Tileset_Table_8_StatuesHall</c>, graphics-set selector $08.</summary>
        StatuesHall,
        /// <summary>$8F:E6F3, <c>Tileset_Table_9_HeatedNorfair</c>, graphics-set selector $09.</summary>
        HeatedNorfair,
        /// <summary>$8F:E6FC, <c>Tileset_Table_A_UnheatedNorfair</c>, graphics-set selector $0A.</summary>
        UnheatedNorfair,
        /// <summary>$8F:E705, <c>Tileset_Table_B_SandlessMaridia</c>, graphics-set selector $0B.</summary>
        SandlessMaridia,
        /// <summary>$8F:E70E, <c>Tileset_Table_C_SandyMaridia</c>, graphics-set selector $0C.</summary>
        SandyMaridia,
        /// <summary>$8F:E717, <c>Tileset_Table_D_Tourian</c>, graphics-set selector $0D.</summary>
        Tourian,
        /// <summary>$8F:E720, <c>Tileset_Table_E_MotherBrain</c>, graphics-set selector $0E.</summary>
        MotherBrain,
        /// <summary>$8F:E729, <c>Tileset_Table_F_BlueCeres</c>, graphics-set selector $0F.</summary>
        BlueCeres,
        /// <summary>$8F:E732, <c>Tileset_Table_10_WhiteCeres</c>, graphics-set selector $10.</summary>
        WhiteCeres,
        /// <summary>$8F:E73B, <c>Tileset_Table_11_BlueCeresElevator</c>, graphics-set selector $11.</summary>
        BlueCeresElevator,
        /// <summary>$8F:E744, <c>Tileset_Table_12_WhiteCeresElevator</c>, graphics-set selector $12.</summary>
        WhiteCeresElevator,
        /// <summary>$8F:E74D, <c>Tileset_Table_13_BlueCeresRidley</c>, graphics-set selector $13.</summary>
        BlueCeresRidley,
        /// <summary>$8F:E756, <c>Tileset_Table_14_WhiteCeresRidley</c>, graphics-set selector $14.</summary>
        WhiteCeresRidley,
        /// <summary>$8F:E75F, <c>Tileset_Table_15_Map_Statues</c>, graphics-set selector $15.</summary>
        Map_Statues,
        /// <summary>$8F:E768, <c>Tileset_Table_16_WreckedShipMap_PowerOff</c>, graphics-set selector $16.</summary>
        WreckedShipMap_PowerOff,
        /// <summary>$8F:E771, <c>Tileset_Table_17_BlueRefill</c>, graphics-set selector $17.</summary>
        BlueRefill,
        /// <summary>$8F:E77A, <c>Tileset_Table_18_YellowRefill</c>, graphics-set selector $18.</summary>
        YellowRefill,
        /// <summary>$8F:E783, <c>Tileset_Table_19_SaveStation</c>, graphics-set selector $19.</summary>
        SaveStation,
        /// <summary>$8F:E78C, <c>Tileset_Table_1A_Kraid</c>, graphics-set selector $1A.</summary>
        Kraid,
        /// <summary>$8F:E795, <c>Tileset_Table_1B_Crocomire</c>, graphics-set selector $1B.</summary>
        Crocomire,
        /// <summary>$8F:E79E, <c>Tileset_Table_1C_Draygon</c>, graphics-set selector $1C.</summary>
        Draygon,
    }

    /// <summary>The compressed block table each room theme's level words use.</summary>
    private static readonly Dictionary<Theme, RoomBlockSet> BlockSources = new()
    {
        [Theme.UpperCrateria] = RoomBlockSet.UpperCrateria,
        [Theme.RedCrateria] = RoomBlockSet.UpperCrateria,
        [Theme.LowerCrateria] = RoomBlockSet.LowerCrateria,
        [Theme.OldTourian] = RoomBlockSet.LowerCrateria,
        [Theme.WreckedShip_PowerOn] = RoomBlockSet.WreckedShip,
        [Theme.WreckedShip_PowerOff] = RoomBlockSet.WreckedShip,
        [Theme.GreenBlueBrinstar] = RoomBlockSet.GreenBlueBrinstar,
        [Theme.RedBrinstar_Kraid] = RoomBlockSet.RedBrinstarKraidStatuesHall,
        [Theme.StatuesHall] = RoomBlockSet.RedBrinstarKraidStatuesHall,
        [Theme.HeatedNorfair] = RoomBlockSet.Norfair,
        [Theme.UnheatedNorfair] = RoomBlockSet.Norfair,
        [Theme.SandlessMaridia] = RoomBlockSet.SandlessMaridia,
        [Theme.SandyMaridia] = RoomBlockSet.SandyMaridia,
        [Theme.Tourian] = RoomBlockSet.Tourian,
        [Theme.MotherBrain] = RoomBlockSet.Tourian,
        [Theme.BlueCeres] = RoomBlockSet.Ceres,
        [Theme.WhiteCeres] = RoomBlockSet.Ceres,
        [Theme.BlueCeresElevator] = RoomBlockSet.Ceres,
        [Theme.WhiteCeresElevator] = RoomBlockSet.Ceres,
        [Theme.BlueCeresRidley] = RoomBlockSet.Ceres,
        [Theme.WhiteCeresRidley] = RoomBlockSet.Ceres,
        [Theme.Map_Statues] = RoomBlockSet.UtilityRoomStatues,
        [Theme.WreckedShipMap_PowerOff] = RoomBlockSet.UtilityRoomStatues,
        [Theme.BlueRefill] = RoomBlockSet.UtilityRoomStatues,
        [Theme.YellowRefill] = RoomBlockSet.UtilityRoomStatues,
        [Theme.SaveStation] = RoomBlockSet.UtilityRoomStatues,
        [Theme.Kraid] = RoomBlockSet.Kraid,
        [Theme.Crocomire] = RoomBlockSet.Crocomire,
        [Theme.Draygon] = RoomBlockSet.Draygon,
    };

    /// <summary>The character tiles uploaded for each room theme's backgrounds.</summary>
    private static readonly Dictionary<Theme, RoomCharacterSet> CharacterSources = new()
    {
        [Theme.UpperCrateria] = RoomCharacterSet.UpperCrateria,
        [Theme.RedCrateria] = RoomCharacterSet.UpperCrateria,
        [Theme.LowerCrateria] = RoomCharacterSet.LowerCrateria,
        [Theme.OldTourian] = RoomCharacterSet.LowerCrateria,
        [Theme.WreckedShip_PowerOn] = RoomCharacterSet.WreckedShip,
        [Theme.WreckedShip_PowerOff] = RoomCharacterSet.WreckedShip,
        [Theme.GreenBlueBrinstar] = RoomCharacterSet.GreenBlueBrinstar,
        [Theme.RedBrinstar_Kraid] = RoomCharacterSet.RedBrinstarKraidStatuesHall,
        [Theme.StatuesHall] = RoomCharacterSet.RedBrinstarKraidStatuesHall,
        [Theme.HeatedNorfair] = RoomCharacterSet.Norfair,
        [Theme.UnheatedNorfair] = RoomCharacterSet.Norfair,
        [Theme.SandlessMaridia] = RoomCharacterSet.SandlessMaridia,
        [Theme.SandyMaridia] = RoomCharacterSet.SandyMaridia,
        [Theme.Tourian] = RoomCharacterSet.Tourian,
        [Theme.MotherBrain] = RoomCharacterSet.Tourian,
        [Theme.BlueCeres] = RoomCharacterSet.Ceres,
        [Theme.WhiteCeres] = RoomCharacterSet.Ceres,
        [Theme.BlueCeresElevator] = RoomCharacterSet.CeresElevator,
        [Theme.WhiteCeresElevator] = RoomCharacterSet.CeresElevator,
        [Theme.BlueCeresRidley] = RoomCharacterSet.CeresRidley,
        [Theme.WhiteCeresRidley] = RoomCharacterSet.CeresRidley,
        [Theme.Map_Statues] = RoomCharacterSet.UtilityRoomStatues,
        [Theme.WreckedShipMap_PowerOff] = RoomCharacterSet.UtilityRoomStatues,
        [Theme.BlueRefill] = RoomCharacterSet.UtilityRoomStatues,
        [Theme.YellowRefill] = RoomCharacterSet.UtilityRoomStatues,
        [Theme.SaveStation] = RoomCharacterSet.UtilityRoomStatues,
        [Theme.Kraid] = RoomCharacterSet.Kraid,
        [Theme.Crocomire] = RoomCharacterSet.Crocomire,
        [Theme.Draygon] = RoomCharacterSet.Draygon,
    };

    /// <summary>Each room theme's selected palette resource, including Ceres color variants.</summary>
    private static readonly Dictionary<Theme, RoomPaletteSet> PaletteSources = new()
    {
        [Theme.UpperCrateria] = RoomPaletteSet.UpperCrateria,
        [Theme.RedCrateria] = RoomPaletteSet.RedCrateria,
        [Theme.LowerCrateria] = RoomPaletteSet.LowerCrateria,
        [Theme.OldTourian] = RoomPaletteSet.OldTourian,
        [Theme.WreckedShip_PowerOn] = RoomPaletteSet.WreckedShipPowerOn,
        [Theme.WreckedShip_PowerOff] = RoomPaletteSet.WreckedShipPowerOff,
        [Theme.GreenBlueBrinstar] = RoomPaletteSet.GreenBlueBrinstar,
        [Theme.RedBrinstar_Kraid] = RoomPaletteSet.RedBrinstarKraid,
        [Theme.StatuesHall] = RoomPaletteSet.StatuesHall,
        [Theme.HeatedNorfair] = RoomPaletteSet.HeatedNorfair,
        [Theme.UnheatedNorfair] = RoomPaletteSet.UnheatedNorfair,
        [Theme.SandlessMaridia] = RoomPaletteSet.SandlessMaridia,
        [Theme.SandyMaridia] = RoomPaletteSet.SandyMaridia,
        [Theme.Tourian] = RoomPaletteSet.Tourian,
        [Theme.MotherBrain] = RoomPaletteSet.MotherBrain,
        [Theme.BlueCeres] = RoomPaletteSet.BlueCeres,
        [Theme.BlueCeresElevator] = RoomPaletteSet.BlueCeres,
        [Theme.BlueCeresRidley] = RoomPaletteSet.BlueCeres,
        [Theme.WhiteCeres] = RoomPaletteSet.WhiteCeres,
        [Theme.WhiteCeresElevator] = RoomPaletteSet.WhiteCeres,
        [Theme.WhiteCeresRidley] = RoomPaletteSet.WhiteCeres,
        [Theme.Map_Statues] = RoomPaletteSet.MapStatues,
        [Theme.WreckedShipMap_PowerOff] = RoomPaletteSet.WreckedShipMapPowerOff,
        [Theme.BlueRefill] = RoomPaletteSet.BlueRefill,
        [Theme.YellowRefill] = RoomPaletteSet.YellowRefill,
        [Theme.SaveStation] = RoomPaletteSet.SaveStation,
        [Theme.Kraid] = RoomPaletteSet.Kraid,
        [Theme.Crocomire] = RoomPaletteSet.Crocomire,
        [Theme.Draygon] = RoomPaletteSet.Draygon,
    };

    /// <summary>The theme of each retail graphics set, in graphics-set order.</summary>
    private static readonly Theme[] GraphicsSetThemes =
    [
        Theme.UpperCrateria, Theme.RedCrateria, Theme.LowerCrateria, Theme.OldTourian, Theme.WreckedShip_PowerOn,
        Theme.WreckedShip_PowerOff, Theme.GreenBlueBrinstar, Theme.RedBrinstar_Kraid, Theme.StatuesHall, Theme.HeatedNorfair,
        Theme.UnheatedNorfair, Theme.SandlessMaridia, Theme.SandyMaridia, Theme.Tourian, Theme.MotherBrain, Theme.BlueCeres,
        Theme.WhiteCeres, Theme.BlueCeresElevator, Theme.WhiteCeresElevator, Theme.BlueCeresRidley, Theme.WhiteCeresRidley,
        Theme.Map_Statues, Theme.WreckedShipMap_PowerOff, Theme.BlueRefill, Theme.YellowRefill, Theme.SaveStation, Theme.Kraid,
        Theme.Crocomire, Theme.Draygon,
    ];

    /// <summary>The block, character and palette resources of each retail graphics set, in graphics-set order.</summary>
    internal static IReadOnlyList<TilesetDefinition> Tilesets { get; } =
        [.. GraphicsSetThemes.Select(theme => new TilesetDefinition(BlockSources[theme], CharacterSources[theme], PaletteSources[theme]))];
}
