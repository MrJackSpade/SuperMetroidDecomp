namespace SuperMetroid.Core.Rooms;

/// <summary>Native graphics-theme dispatch to independently installed room artwork resources.</summary>
public static class RoomTilesetDefinitions
{
    /// <summary>$8F:E7A7..E7DF: graphics themes $00..$1C precede unrelated data.</summary>
    public const int Count = 0x1d;

    /// <summary>Native graphics-set selectors shared by the room's block, character, and palette source tables.</summary>
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

    /// <summary>Native bank-$C1 block-tile table identities selected for each graphics theme.</summary>
    private enum BlockSource : int
    {
        /// <summary>$C1B6F6, <c>TileTables_0_1_UpperCrateria</c>: native BlockSource identity.</summary>
        UpperCrateria = 0xc1b6f6,
        /// <summary>$C1BEEE, <c>TileTables_2_3_LowerCrateria</c>: native BlockSource identity.</summary>
        LowerCrateria = 0xc1beee,
        /// <summary>$C1C5CF, <c>TileTables_4_5_WreckedShip</c>: native BlockSource identity.</summary>
        WreckedShip = 0xc1c5cf,
        /// <summary>$C1CFA6, <c>TileTables_6_GreenBlueBrinstar</c>: native BlockSource identity.</summary>
        GreenBlueBrinstar = 0xc1cfa6,
        /// <summary>$C1D8DC, <c>TileTables_7_8_RedBrinstar_Kraid_StatuesHall</c>: native BlockSource identity.</summary>
        RedBrinstar_Kraid_StatuesHall = 0xc1d8dc,
        /// <summary>$C1E361, <c>TileTables_9_A_Norfair</c>: native BlockSource identity.</summary>
        Norfair = 0xc1e361,
        /// <summary>$C1F4B1, <c>TileTables_B_SandlessMaridia</c>: native BlockSource identity.</summary>
        SandlessMaridia = 0xc1f4b1,
        /// <summary>$C2855F, <c>TileTables_C_SandyMaridia</c>: native BlockSource identity.</summary>
        SandyMaridia = 0xc2855f,
        /// <summary>$C29B01, <c>TileTables_D_E_Tourian</c>: native BlockSource identity.</summary>
        Tourian = 0xc29b01,
        /// <summary>$C2A75E, <c>TileTables_F_10_11_12_13_14_Ceres</c>: native BlockSource identity.</summary>
        Ceres = 0xc2a75e,
        /// <summary>$C2A27B, <c>TileTables_15_16_17_18_19_UtilityRoom_Statues</c>: native BlockSource identity.</summary>
        UtilityRoom_Statues = 0xc2a27b,
        /// <summary>$C1E189, <c>TileTables_1A_Kraid</c>: native BlockSource identity.</summary>
        Kraid = 0xc1e189,
        /// <summary>$C1F3AF, <c>TileTables_1B_Crocomire</c>: native BlockSource identity.</summary>
        Crocomire = 0xc1f3af,
        /// <summary>$C2960D, <c>TileTables_1C_Draygon</c>: native BlockSource identity.</summary>
        Draygon = 0xc2960d,
    }

    /// <summary>Chooses the native block-tile table shared by the supplied graphics theme.</summary>
    /// <param name="theme">Room graphics-set selector.</param>
    /// <returns>Bank-$C1 block-table identity used by room tile decoding.</returns>
    private static BlockSource SelectBlockSource(Theme theme) => theme switch
    {
        Theme.UpperCrateria or Theme.RedCrateria => BlockSource.UpperCrateria,
        Theme.LowerCrateria or Theme.OldTourian => BlockSource.LowerCrateria,
        Theme.WreckedShip_PowerOn or Theme.WreckedShip_PowerOff => BlockSource.WreckedShip,
        Theme.GreenBlueBrinstar => BlockSource.GreenBlueBrinstar,
        Theme.RedBrinstar_Kraid or Theme.StatuesHall => BlockSource.RedBrinstar_Kraid_StatuesHall,
        Theme.HeatedNorfair or Theme.UnheatedNorfair => BlockSource.Norfair,
        Theme.SandlessMaridia => BlockSource.SandlessMaridia,
        Theme.SandyMaridia => BlockSource.SandyMaridia,
        Theme.Tourian or Theme.MotherBrain => BlockSource.Tourian,
        Theme.BlueCeres or Theme.WhiteCeres or Theme.BlueCeresElevator or Theme.WhiteCeresElevator or Theme.BlueCeresRidley or Theme.WhiteCeresRidley => BlockSource.Ceres,
        Theme.Map_Statues or Theme.WreckedShipMap_PowerOff or Theme.BlueRefill or Theme.YellowRefill or Theme.SaveStation => BlockSource.UtilityRoom_Statues,
        Theme.Kraid => BlockSource.Kraid,
        Theme.Crocomire => BlockSource.Crocomire,
        Theme.Draygon => BlockSource.Draygon,
        _ => throw new InvalidDataException("Invalid graphics theme."),
    };

    /// <summary>Native character-tile source identities selected independently from block and palette tables.</summary>
    private enum CharacterSource : int
    {
        /// <summary>$BAC629, <c>Tiles_0_1_UpperCrateria</c>: native CharacterSource identity.</summary>
        UpperCrateria = 0xbac629,
        /// <summary>$BAF911, <c>Tiles_2_3_LowerCrateria</c>: native CharacterSource identity.</summary>
        LowerCrateria = 0xbaf911,
        /// <summary>$BBAE9E, <c>Tiles_4_5_WreckedShip</c>: native CharacterSource identity.</summary>
        WreckedShip = 0xbbae9e,
        /// <summary>$BBE6B0, <c>Tiles_6_GreenBlueBrinstar</c>: native CharacterSource identity.</summary>
        GreenBlueBrinstar = 0xbbe6b0,
        /// <summary>$BCA5AA, <c>Tiles_7_8_RedBrinstar_Kraid_StatuesHall</c>: native CharacterSource identity.</summary>
        RedBrinstar_Kraid_StatuesHall = 0xbca5aa,
        /// <summary>$BDC3F9, <c>Tiles_9_A_Norfair</c>: native CharacterSource identity.</summary>
        Norfair = 0xbdc3f9,
        /// <summary>$BEB130, <c>Tiles_B_SandlessMaridia</c>: native CharacterSource identity.</summary>
        SandlessMaridia = 0xbeb130,
        /// <summary>$BEE78D, <c>Tiles_C_SandyMaridia</c>: native CharacterSource identity.</summary>
        SandyMaridia = 0xbee78d,
        /// <summary>$BFD414, <c>Tiles_D_E_Tourian</c>: native CharacterSource identity.</summary>
        Tourian = 0xbfd414,
        /// <summary>$C0B004, <c>Tiles_F_10_Ceres</c>: native CharacterSource identity.</summary>
        Ceres = 0xc0b004,
        /// <summary>$C0E22A, <c>Tiles_11_12_CeresElevator</c>: native CharacterSource identity.</summary>
        CeresElevator = 0xc0e22a,
        /// <summary>$C18DA9, <c>Tiles_13_14_CeresRidley</c>: native CharacterSource identity.</summary>
        CeresRidley = 0xc18da9,
        /// <summary>$C0860B, <c>Tiles_15_16_17_18_19_UtilityRoom_Statues</c>: native CharacterSource identity.</summary>
        UtilityRoom_Statues = 0xc0860b,
        /// <summary>$BCDFF0, <c>Tiles_1A_Kraid</c>: native CharacterSource identity.</summary>
        Kraid = 0xbcdff0,
        /// <summary>$BDFE2A, <c>Tiles_1B_Crocomire</c>: native CharacterSource identity.</summary>
        Crocomire = 0xbdfe2a,
        /// <summary>$BF9DEA, <c>Tiles_1C_Draygon</c>: native CharacterSource identity.</summary>
        Draygon = 0xbf9dea,
    }

    /// <summary>Chooses the native character graphics source associated with a graphics theme.</summary>
    /// <param name="theme">Room graphics-set selector.</param>
    /// <returns>Character-data source identity for the theme.</returns>
    private static CharacterSource SelectCharacterSource(Theme theme) => theme switch
    {
        Theme.UpperCrateria or Theme.RedCrateria => CharacterSource.UpperCrateria,
        Theme.LowerCrateria or Theme.OldTourian => CharacterSource.LowerCrateria,
        Theme.WreckedShip_PowerOn or Theme.WreckedShip_PowerOff => CharacterSource.WreckedShip,
        Theme.GreenBlueBrinstar => CharacterSource.GreenBlueBrinstar,
        Theme.RedBrinstar_Kraid or Theme.StatuesHall => CharacterSource.RedBrinstar_Kraid_StatuesHall,
        Theme.HeatedNorfair or Theme.UnheatedNorfair => CharacterSource.Norfair,
        Theme.SandlessMaridia => CharacterSource.SandlessMaridia,
        Theme.SandyMaridia => CharacterSource.SandyMaridia,
        Theme.Tourian or Theme.MotherBrain => CharacterSource.Tourian,
        Theme.BlueCeres or Theme.WhiteCeres => CharacterSource.Ceres,
        Theme.BlueCeresElevator or Theme.WhiteCeresElevator => CharacterSource.CeresElevator,
        Theme.BlueCeresRidley or Theme.WhiteCeresRidley => CharacterSource.CeresRidley,
        Theme.Map_Statues or Theme.WreckedShipMap_PowerOff or Theme.BlueRefill or Theme.YellowRefill or Theme.SaveStation => CharacterSource.UtilityRoom_Statues,
        Theme.Kraid => CharacterSource.Kraid,
        Theme.Crocomire => CharacterSource.Crocomire,
        Theme.Draygon => CharacterSource.Draygon,
        _ => throw new InvalidDataException("Invalid graphics theme."),
    };

    /// <summary>Native palette-data source identities selected for room graphics themes.</summary>
    private enum PaletteSource : int
    {
        /// <summary>$C2AD7C, <c>Palettes_0_UpperCrateria</c>: native PaletteSource identity.</summary>
        UpperCrateria = 0xc2ad7c,
        /// <summary>$C2AE5D, <c>Palettes_1_RedCrateria</c>: native PaletteSource identity.</summary>
        RedCrateria = 0xc2ae5d,
        /// <summary>$C2AF43, <c>Palettes_2_LowerCrateria</c>: native PaletteSource identity.</summary>
        LowerCrateria = 0xc2af43,
        /// <summary>$C2B015, <c>Palettes_3_OldTourian</c>: native PaletteSource identity.</summary>
        OldTourian = 0xc2b015,
        /// <summary>$C2B0E7, <c>Palettes_4_WreckedShip_PowerOn</c>: native PaletteSource identity.</summary>
        WreckedShip_PowerOn = 0xc2b0e7,
        /// <summary>$C2B1A6, <c>Palettes_5_WreckedShip_PowerOff</c>: native PaletteSource identity.</summary>
        WreckedShip_PowerOff = 0xc2b1a6,
        /// <summary>$C2B264, <c>Palettes_6_GreenBlueBrinstar</c>: native PaletteSource identity.</summary>
        GreenBlueBrinstar = 0xc2b264,
        /// <summary>$C2B35F, <c>Palettes_7_RedBrinstar_Kraid</c>: native PaletteSource identity.</summary>
        RedBrinstar_Kraid = 0xc2b35f,
        /// <summary>$C2B447, <c>Palettes_8_StatuesHall</c>: native PaletteSource identity.</summary>
        StatuesHall = 0xc2b447,
        /// <summary>$C2B5E4, <c>Palettes_9_HeatedNorfair</c>: native PaletteSource identity.</summary>
        HeatedNorfair = 0xc2b5e4,
        /// <summary>$C2B6BB, <c>Palettes_A_UnheatedNorfair</c>: native PaletteSource identity.</summary>
        UnheatedNorfair = 0xc2b6bb,
        /// <summary>$C2B83C, <c>Palettes_B_SandlessMaridia</c>: native PaletteSource identity.</summary>
        SandlessMaridia = 0xc2b83c,
        /// <summary>$C2B92E, <c>Palettes_C_SandyMaridia</c>: native PaletteSource identity.</summary>
        SandyMaridia = 0xc2b92e,
        /// <summary>$C2BAED, <c>Palettes_D_Tourian</c>: native PaletteSource identity.</summary>
        Tourian = 0xc2baed,
        /// <summary>$C2BBC1, <c>Palettes_E_MotherBrain</c>: native PaletteSource identity.</summary>
        MotherBrain = 0xc2bbc1,
        /// <summary>$C2C104, <c>Palettes_F_11_13_BlueCeres</c>: native PaletteSource identity.</summary>
        BlueCeres = 0xc2c104,
        /// <summary>$C2C1E3, <c>Palettes_10_12_14_WhiteCeres</c>: native PaletteSource identity.</summary>
        WhiteCeres = 0xc2c1e3,
        /// <summary>$C2BC9C, <c>Palettes_Map_Statues</c>: native PaletteSource identity.</summary>
        Palettes_Map_Statues = 0xc2bc9c,
        /// <summary>$C2BD7B, <c>Palettes_16_WreckedShipMap_PowerOff</c>: native PaletteSource identity.</summary>
        WreckedShipMap_PowerOff = 0xc2bd7b,
        /// <summary>$C2BE58, <c>Palettes_17_BlueRefill</c>: native PaletteSource identity.</summary>
        BlueRefill = 0xc2be58,
        /// <summary>$C2BF3D, <c>Palettes_18_YellowRefill</c>: native PaletteSource identity.</summary>
        YellowRefill = 0xc2bf3d,
        /// <summary>$C2C021, <c>Palettes_19_SaveStation</c>: native PaletteSource identity.</summary>
        SaveStation = 0xc2c021,
        /// <summary>$C2B510, <c>Palettes_1A_Kraid</c>: native PaletteSource identity.</summary>
        Kraid = 0xc2b510,
        /// <summary>$C2B798, <c>Palettes_1B_Crocomire</c>: native PaletteSource identity.</summary>
        Crocomire = 0xc2b798,
        /// <summary>$C2BA2C, <c>Palettes_1C_Draygon</c>: native PaletteSource identity.</summary>
        Draygon = 0xc2ba2c,
    }

    /// <summary>Chooses the native palette data associated with a graphics theme.</summary>
    /// <param name="theme">Room graphics-set selector.</param>
    /// <returns>Palette-data source identity for the theme.</returns>
    private static PaletteSource SelectPaletteSource(Theme theme) => theme switch
    {
        Theme.UpperCrateria => PaletteSource.UpperCrateria,
        Theme.RedCrateria => PaletteSource.RedCrateria,
        Theme.LowerCrateria => PaletteSource.LowerCrateria,
        Theme.OldTourian => PaletteSource.OldTourian,
        Theme.WreckedShip_PowerOn => PaletteSource.WreckedShip_PowerOn,
        Theme.WreckedShip_PowerOff => PaletteSource.WreckedShip_PowerOff,
        Theme.GreenBlueBrinstar => PaletteSource.GreenBlueBrinstar,
        Theme.RedBrinstar_Kraid => PaletteSource.RedBrinstar_Kraid,
        Theme.StatuesHall => PaletteSource.StatuesHall,
        Theme.HeatedNorfair => PaletteSource.HeatedNorfair,
        Theme.UnheatedNorfair => PaletteSource.UnheatedNorfair,
        Theme.SandlessMaridia => PaletteSource.SandlessMaridia,
        Theme.SandyMaridia => PaletteSource.SandyMaridia,
        Theme.Tourian => PaletteSource.Tourian,
        Theme.MotherBrain => PaletteSource.MotherBrain,
        Theme.BlueCeres or Theme.BlueCeresElevator or Theme.BlueCeresRidley => PaletteSource.BlueCeres,
        Theme.WhiteCeres or Theme.WhiteCeresElevator or Theme.WhiteCeresRidley => PaletteSource.WhiteCeres,
        Theme.Map_Statues => PaletteSource.Palettes_Map_Statues,
        Theme.WreckedShipMap_PowerOff => PaletteSource.WreckedShipMap_PowerOff,
        Theme.BlueRefill => PaletteSource.BlueRefill,
        Theme.YellowRefill => PaletteSource.YellowRefill,
        Theme.SaveStation => PaletteSource.SaveStation,
        Theme.Kraid => PaletteSource.Kraid,
        Theme.Crocomire => PaletteSource.Crocomire,
        Theme.Draygon => PaletteSource.Draygon,
        _ => throw new InvalidDataException("Invalid graphics theme."),
    };

    /// <summary>Selects block, character and palette resources for the room state's native graphics theme.</summary>
    public static TilesetDefinition Get(byte graphicsSet)
    {
        if (graphicsSet >= Count)
            throw new InvalidDataException($"Graphics set ${graphicsSet:X2} is outside the compiled retail tileset table.");
        var theme = (Theme)graphicsSet;
        return new(            (int)SelectBlockSource(theme), (int)SelectCharacterSource(theme), (int)SelectPaletteSource(theme));
    }
}
