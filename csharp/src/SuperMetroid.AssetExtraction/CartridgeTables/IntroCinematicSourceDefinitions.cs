using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the opening cinematic's artwork (#1416): the import pipeline's private knowledge of the
/// cartridge, decoded once into the installed opening documents.
/// </summary>
[CpuBusBoundary("catalog: opening cinematic cartridge sources")]
internal static class IntroCinematicSourceDefinitions
{
    /// <summary>Bank $8C, which holds the cinematic spritemaps their bank-relative pointers name.</summary>
    internal const byte SpritemapBank = 0x8c;

    /// <summary>$8C:E3E9, the cinematic's palette.</summary>
    internal const int Palette = 0x8ce3e9;

    /// <summary>$95:F90E, the scenes' compressed background characters.</summary>
    internal const int BackgroundCharacters = 0x95f90e;

    /// <summary>$95:D089, the narration font's compressed characters.</summary>
    internal const int FontOne = 0x95d089;

    /// <summary>$97:88CC, the Samus portrait's compressed tilemap.</summary>
    internal const int SamusHeadTilemap = 0x9788cc;

    /// <summary>$96:FF14, the scenes' compressed background page tilemaps.</summary>
    internal const int BackgroundPageTilemaps = 0x96ff14;

    /// <summary>$95:E4C2, the cinematic sprites' compressed characters.</summary>
    internal const int ObjectCharacters = 0x95e4c2;

    /// <summary>$97:8D12, the first narration's compressed tilemap.</summary>
    internal const int FirstNarrationTilemap = 0x978d12;

    /// <summary>$9A:D200, the shared cinematic sprite characters the opening and the Ceres destruction load.</summary>
    internal const int IntroObjectCharacters = 0x9ad200;

    /// <summary>$8B:A72B, the final text line's tilemap cells.</summary>
    internal const int FinalTextLine = 0x8ba72b;
}
