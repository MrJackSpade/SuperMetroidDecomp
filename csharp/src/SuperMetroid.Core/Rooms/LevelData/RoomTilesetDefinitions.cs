namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Native graphics-theme dispatch to independently installed room artwork resources.</summary>
public static class RoomTilesetDefinitions
{
    /// <summary>$8F:E7A7..E7DF: graphics themes $00..$1C precede unrelated data.</summary>
    public const int Count = 0x1d;

    /// <summary>Twenty-nine retail graphics-set selectors from the bank-$8F room tileset table.</summary>
    private enum Theme : byte
    {
        /// <summary>$8F:E6A2, <c>Tileset_Table_0_UpperCrateria</c>, graphics-set selector $00.</summary>
        UpperCrateria = 0x00,
        /// <summary>$8F:E6AB, <c>Tileset_Table_1_RedCrateria</c>, graphics-set selector $01.</summary>
        RedCrateria = 0x01,
        /// <summary>$8F:E6B4, <c>Tileset_Table_2_LowerCrateria</c>, graphics-set selector $02.</summary>
        LowerCrateria = 0x02,
        /// <summary>$8F:E6BD, <c>Tileset_Table_3_OldTourian</c>, graphics-set selector $03.</summary>
        OldTourian = 0x03,
        /// <summary>$8F:E6C6, <c>Tileset_Table_4_WreckedShip_PowerOn</c>, graphics-set selector $04.</summary>
        WreckedShip_PowerOn = 0x04,
        /// <summary>$8F:E6CF, <c>Tileset_Table_5_WreckedShip_PowerOff</c>, graphics-set selector $05.</summary>
        WreckedShip_PowerOff = 0x05,
        /// <summary>$8F:E6D8, <c>Tileset_Table_6_GreenBlueBrinstar</c>, graphics-set selector $06.</summary>
        GreenBlueBrinstar = 0x06,
        /// <summary>$8F:E6E1, <c>Tileset_Table_7_RedBrinstar_Kraid</c>, graphics-set selector $07.</summary>
        RedBrinstar_Kraid = 0x07,
        /// <summary>$8F:E6EA, <c>Tileset_Table_8_StatuesHall</c>, graphics-set selector $08.</summary>
        StatuesHall = 0x08,
        /// <summary>$8F:E6F3, <c>Tileset_Table_9_HeatedNorfair</c>, graphics-set selector $09.</summary>
        HeatedNorfair = 0x09,
        /// <summary>$8F:E6FC, <c>Tileset_Table_A_UnheatedNorfair</c>, graphics-set selector $0A.</summary>
        UnheatedNorfair = 0x0a,
        /// <summary>$8F:E705, <c>Tileset_Table_B_SandlessMaridia</c>, graphics-set selector $0B.</summary>
        SandlessMaridia = 0x0b,
        /// <summary>$8F:E70E, <c>Tileset_Table_C_SandyMaridia</c>, graphics-set selector $0C.</summary>
        SandyMaridia = 0x0c,
        /// <summary>$8F:E717, <c>Tileset_Table_D_Tourian</c>, graphics-set selector $0D.</summary>
        Tourian = 0x0d,
        /// <summary>$8F:E720, <c>Tileset_Table_E_MotherBrain</c>, graphics-set selector $0E.</summary>
        MotherBrain = 0x0e,
        /// <summary>$8F:E729, <c>Tileset_Table_F_BlueCeres</c>, graphics-set selector $0F.</summary>
        BlueCeres = 0x0f,
        /// <summary>$8F:E732, <c>Tileset_Table_10_WhiteCeres</c>, graphics-set selector $10.</summary>
        WhiteCeres = 0x10,
        /// <summary>$8F:E73B, <c>Tileset_Table_11_BlueCeresElevator</c>, graphics-set selector $11.</summary>
        BlueCeresElevator = 0x11,
        /// <summary>$8F:E744, <c>Tileset_Table_12_WhiteCeresElevator</c>, graphics-set selector $12.</summary>
        WhiteCeresElevator = 0x12,
        /// <summary>$8F:E74D, <c>Tileset_Table_13_BlueCeresRidley</c>, graphics-set selector $13.</summary>
        BlueCeresRidley = 0x13,
        /// <summary>$8F:E756, <c>Tileset_Table_14_WhiteCeresRidley</c>, graphics-set selector $14.</summary>
        WhiteCeresRidley = 0x14,
        /// <summary>$8F:E75F, <c>Tileset_Table_15_Map_Statues</c>, graphics-set selector $15.</summary>
        Map_Statues = 0x15,
        /// <summary>$8F:E768, <c>Tileset_Table_16_WreckedShipMap_PowerOff</c>, graphics-set selector $16.</summary>
        WreckedShipMap_PowerOff = 0x16,
        /// <summary>$8F:E771, <c>Tileset_Table_17_BlueRefill</c>, graphics-set selector $17.</summary>
        BlueRefill = 0x17,
        /// <summary>$8F:E77A, <c>Tileset_Table_18_YellowRefill</c>, graphics-set selector $18.</summary>
        YellowRefill = 0x18,
        /// <summary>$8F:E783, <c>Tileset_Table_19_SaveStation</c>, graphics-set selector $19.</summary>
        SaveStation = 0x19,
        /// <summary>$8F:E78C, <c>Tileset_Table_1A_Kraid</c>, graphics-set selector $1A.</summary>
        Kraid = 0x1a,
        /// <summary>$8F:E795, <c>Tileset_Table_1B_Crocomire</c>, graphics-set selector $1B.</summary>
        Crocomire = 0x1b,
        /// <summary>$8F:E79E, <c>Tileset_Table_1C_Draygon</c>, graphics-set selector $1C.</summary>
        Draygon = 0x1c,
    }

    /// <summary>Maps a room theme to the compressed block table used for its level words.</summary>
    private static RoomBlockSet SelectBlockSource(Theme theme) => theme switch
    {
        Theme.UpperCrateria or Theme.RedCrateria => RoomBlockSet.UpperCrateria,
        Theme.LowerCrateria or Theme.OldTourian => RoomBlockSet.LowerCrateria,
        Theme.WreckedShip_PowerOn or Theme.WreckedShip_PowerOff => RoomBlockSet.WreckedShip,
        Theme.GreenBlueBrinstar => RoomBlockSet.GreenBlueBrinstar,
        Theme.RedBrinstar_Kraid or Theme.StatuesHall => RoomBlockSet.RedBrinstarKraidStatuesHall,
        Theme.HeatedNorfair or Theme.UnheatedNorfair => RoomBlockSet.Norfair,
        Theme.SandlessMaridia => RoomBlockSet.SandlessMaridia,
        Theme.SandyMaridia => RoomBlockSet.SandyMaridia,
        Theme.Tourian or Theme.MotherBrain => RoomBlockSet.Tourian,
        Theme.BlueCeres or Theme.WhiteCeres or Theme.BlueCeresElevator or Theme.WhiteCeresElevator or Theme.BlueCeresRidley or Theme.WhiteCeresRidley => RoomBlockSet.Ceres,
        Theme.Map_Statues or Theme.WreckedShipMap_PowerOff or Theme.BlueRefill or Theme.YellowRefill or Theme.SaveStation => RoomBlockSet.UtilityRoomStatues,
        Theme.Kraid => RoomBlockSet.Kraid,
        Theme.Crocomire => RoomBlockSet.Crocomire,
        Theme.Draygon => RoomBlockSet.Draygon,
        _ => throw new InvalidDataException("Invalid graphics theme."),
    };

    /// <summary>Maps a room theme to the character tiles uploaded for its backgrounds.</summary>
    private static RoomCharacterSet SelectCharacterSource(Theme theme) => theme switch
    {
        Theme.UpperCrateria or Theme.RedCrateria => RoomCharacterSet.UpperCrateria,
        Theme.LowerCrateria or Theme.OldTourian => RoomCharacterSet.LowerCrateria,
        Theme.WreckedShip_PowerOn or Theme.WreckedShip_PowerOff => RoomCharacterSet.WreckedShip,
        Theme.GreenBlueBrinstar => RoomCharacterSet.GreenBlueBrinstar,
        Theme.RedBrinstar_Kraid or Theme.StatuesHall => RoomCharacterSet.RedBrinstarKraidStatuesHall,
        Theme.HeatedNorfair or Theme.UnheatedNorfair => RoomCharacterSet.Norfair,
        Theme.SandlessMaridia => RoomCharacterSet.SandlessMaridia,
        Theme.SandyMaridia => RoomCharacterSet.SandyMaridia,
        Theme.Tourian or Theme.MotherBrain => RoomCharacterSet.Tourian,
        Theme.BlueCeres or Theme.WhiteCeres => RoomCharacterSet.Ceres,
        Theme.BlueCeresElevator or Theme.WhiteCeresElevator => RoomCharacterSet.CeresElevator,
        Theme.BlueCeresRidley or Theme.WhiteCeresRidley => RoomCharacterSet.CeresRidley,
        Theme.Map_Statues or Theme.WreckedShipMap_PowerOff or Theme.BlueRefill or Theme.YellowRefill or Theme.SaveStation => RoomCharacterSet.UtilityRoomStatues,
        Theme.Kraid => RoomCharacterSet.Kraid,
        Theme.Crocomire => RoomCharacterSet.Crocomire,
        Theme.Draygon => RoomCharacterSet.Draygon,
        _ => throw new InvalidDataException("Invalid graphics theme."),
    };

    /// <summary>Maps a room theme to its selected palette resource, including Ceres color variants.</summary>
    private static RoomPaletteSet SelectPaletteSource(Theme theme) => theme switch
    {
        Theme.UpperCrateria => RoomPaletteSet.UpperCrateria,
        Theme.RedCrateria => RoomPaletteSet.RedCrateria,
        Theme.LowerCrateria => RoomPaletteSet.LowerCrateria,
        Theme.OldTourian => RoomPaletteSet.OldTourian,
        Theme.WreckedShip_PowerOn => RoomPaletteSet.WreckedShipPowerOn,
        Theme.WreckedShip_PowerOff => RoomPaletteSet.WreckedShipPowerOff,
        Theme.GreenBlueBrinstar => RoomPaletteSet.GreenBlueBrinstar,
        Theme.RedBrinstar_Kraid => RoomPaletteSet.RedBrinstarKraid,
        Theme.StatuesHall => RoomPaletteSet.StatuesHall,
        Theme.HeatedNorfair => RoomPaletteSet.HeatedNorfair,
        Theme.UnheatedNorfair => RoomPaletteSet.UnheatedNorfair,
        Theme.SandlessMaridia => RoomPaletteSet.SandlessMaridia,
        Theme.SandyMaridia => RoomPaletteSet.SandyMaridia,
        Theme.Tourian => RoomPaletteSet.Tourian,
        Theme.MotherBrain => RoomPaletteSet.MotherBrain,
        Theme.BlueCeres or Theme.BlueCeresElevator or Theme.BlueCeresRidley => RoomPaletteSet.BlueCeres,
        Theme.WhiteCeres or Theme.WhiteCeresElevator or Theme.WhiteCeresRidley => RoomPaletteSet.WhiteCeres,
        Theme.Map_Statues => RoomPaletteSet.MapStatues,
        Theme.WreckedShipMap_PowerOff => RoomPaletteSet.WreckedShipMapPowerOff,
        Theme.BlueRefill => RoomPaletteSet.BlueRefill,
        Theme.YellowRefill => RoomPaletteSet.YellowRefill,
        Theme.SaveStation => RoomPaletteSet.SaveStation,
        Theme.Kraid => RoomPaletteSet.Kraid,
        Theme.Crocomire => RoomPaletteSet.Crocomire,
        Theme.Draygon => RoomPaletteSet.Draygon,
        _ => throw new InvalidDataException("Invalid graphics theme."),
    };

    /// <summary>Selects block, character and palette resources for the room state's native graphics theme.</summary>
    public static TilesetDefinition Get(int graphicsSet)
    {
        if ((uint)graphicsSet >= Count)
            throw new InvalidDataException($"Graphics set ${graphicsSet:X2} is outside the compiled retail tileset table.");
        var theme = (Theme)graphicsSet;
        return new(SelectBlockSource(theme), SelectCharacterSource(theme), SelectPaletteSource(theme));
    }
}
