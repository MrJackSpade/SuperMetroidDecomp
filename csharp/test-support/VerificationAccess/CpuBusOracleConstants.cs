using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

// #629 moved these CPU-bus addresses into typed domain catalogs. Verification oracles keep the
// literal cartridge values they assert against until each suite moves onto the domain views.

/// <summary>Cartridge values of <see cref="RoomScrollGrid"/> that only verification reads.</summary>
internal static class RoomScrollGridConstants
{
    /// <summary>24-bit WRAM address of the first room scroll-zone byte, $7E:CD20.</summary>
    public const int WorkRamAddress = 0x7ecd20;

    /// <summary>Provides verification helpers for <see cref="RoomScrollGrid"/>.</summary>
    extension(RoomScrollGrid)
    {
        /// <inheritdoc cref="RoomScrollGridConstants.WorkRamAddress"/>
        internal static int WorkRamAddress => RoomScrollGridConstants.WorkRamAddress;
    }
}

/// <summary>Cartridge values of <see cref="HudState"/> that only verification reads.</summary>
internal static class HudStateConstants
{
    /// <summary>$7E:C608, native mutable HUD tilemap buffer.</summary>
    public const int WorkRamAddress = 0x7ec608;

    /// <summary>Provides verification helpers for <see cref="HudState"/>.</summary>
    extension(HudState)
    {
        /// <inheritdoc cref="HudStateConstants.WorkRamAddress"/>
        internal static int WorkRamAddress => HudStateConstants.WorkRamAddress;
    }
}

/// <summary>Cartridge values of <see cref="MotherBrainBg2Definitions"/> that only verification reads.</summary>
internal static class MotherBrainBg2DefinitionsConstants
{
    /// <summary>EnemyBG2Tilemap at $7E:2000.</summary>
    public const int WorkAddress = 0x7e2000;

    /// <summary>Provides verification helpers for <see cref="MotherBrainBg2Definitions"/>.</summary>
    extension(MotherBrainBg2Definitions)
    {
        /// <inheritdoc cref="MotherBrainBg2DefinitionsConstants.WorkAddress"/>
        internal static int WorkAddress => MotherBrainBg2DefinitionsConstants.WorkAddress;
    }
}

/// <summary>Cartridge values of <see cref="MotherBrainDrainedPaletteRomData"/> that only verification reads.</summary>
internal static class MotherBrainDrainedPaletteRomDataConstants
{
    /// <summary>$AD:EF44/EF81 write the trailing source word directly to low WRAM $017C.</summary>
    public const int TrailingWordWram = 0x7e017c;

    /// <summary>Provides verification helpers for <see cref="MotherBrainDrainedPaletteRomData"/>.</summary>
    extension(MotherBrainDrainedPaletteRomData)
    {
        /// <inheritdoc cref="MotherBrainDrainedPaletteRomDataConstants.TrailingWordWram"/>
        internal static int TrailingWordWram => MotherBrainDrainedPaletteRomDataConstants.TrailingWordWram;
    }
}

/// <summary>X-ray setup WRAM buffer addresses and the frozen reveal map, as verification reads them.</summary>
internal static class XraySetupMemory
{
    /// <summary>$7E:6000, first BG1 screen captured by $91:CB57.</summary>
    public const int SavedBg1 = 0x7e6000;
    /// <summary>$7E:6800, second BG1 screen captured by $91:CB1C.</summary>
    public const int SavedBg1SecondScreen = SavedBg1 + XrayTilemapLayout.ScreenWords * 2;

    /// <summary>Reads the frozen reveal map through the production X-ray view.</summary>
    public static ushort[] ReadReveal(ISnesMemoryBus memory) => new XrayMemory(memory).RevealTilemap.ToArray();
}
