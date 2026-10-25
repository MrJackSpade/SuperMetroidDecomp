using System.Collections.Frozen;
using System.Collections.ObjectModel;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the options screen's pages, button labels and heading spritemaps (#1416): the import
/// pipeline's private knowledge of the cartridge, decoded once into the installed options presentation.
/// </summary>
[CpuBusBoundary("catalog: options screen cartridge sources")]
internal static class GameOptionsSourceDefinitions
{
    /// <summary>Bank $82, in which the options screen's sixteen-bit label pointers resolve.</summary>
    internal const int MenuBank = 0x820000;

    /// <summary>The bank-$97 compressed tilemap of each options page and language, as $82:EC66..ECB8 loads them.</summary>
    internal static readonly FrozenDictionary<GameOptionsTilemap, GameOptionsPageResource> Pages =
        new Dictionary<GameOptionsTilemap, GameOptionsPageResource>
        {
            [GameOptionsTilemap.Primary] = new(0x978df4, "primary"),
            [GameOptionsTilemap.ControllerEnglish] = new(0x978fcd, "English controller"),
            [GameOptionsTilemap.ControllerJapanese] = new(0x9791c4, "Japanese controller"),
            [GameOptionsTilemap.SpecialEnglish] = new(0x97938d, "English special-settings"),
            [GameOptionsTilemap.SpecialJapanese] = new(0x97953a, "Japanese special-settings"),
        }.ToFrozenDictionary();

    /// <summary>
    /// ControllerButton_TilemapPointers at $82:F647: the bank-$82 label of each assignable button, X, A, B, Select, Y, L
    /// and R, each a 3-by-2 tilemap twelve bytes after the last.
    /// </summary>
    internal static ReadOnlyCollection<ushort> ControllerLabelSources { get; } =
        new List<ushort> { 0xf659, 0xf665, 0xf671, 0xf67d, 0xf689, 0xf695, 0xf6a1 }.AsReadOnly();

    /// <summary>The menu spritemap ordinal of each page's heading, as the instruction lists at $82:F47E/F48E/F49E select them.</summary>
    internal static readonly FrozenDictionary<GameOptionsPage, ushort> HeadingSpritemaps =
        new Dictionary<GameOptionsPage, ushort>
        {
            [GameOptionsPage.Primary] = 0x4b,
            [GameOptionsPage.Controller] = 0x4c,
            [GameOptionsPage.Special] = 0x4d,
        }.ToFrozenDictionary();
}
