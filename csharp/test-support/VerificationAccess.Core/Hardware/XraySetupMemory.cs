using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Hardware;

/// <summary>X-ray setup WRAM buffer addresses and the frozen reveal map, as verification reads them.</summary>
internal static class XraySetupMemory
{
    /// <summary>$7E:6000, first BG1 screen captured by $91:CB57.</summary>
    public const int SavedBg1 = 0x7e6000;
    /// <summary>$7E:6800, second BG1 screen captured by $91:CB1C.</summary>
    public const int SavedBg1SecondScreen = SavedBg1 + XrayTilemapLayout.ScreenWords * 2;

    /// <summary>Reads the frozen reveal map through the production X-ray view.</summary>
    public static BackgroundTile[] ReadReveal(SuperMetroid.Core.World.XrayReveal xray) => xray.RevealTilemap.ToArray();
}
