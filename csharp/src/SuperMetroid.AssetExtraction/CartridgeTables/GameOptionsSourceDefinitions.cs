using SuperMetroid.Core.Progression;
using SuperMetroid.Core;
using System.Collections.Frozen;
using System.Collections.ObjectModel;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the options screen's pages, button labels, heading spritemaps and the headings' and cursor's
/// anchors (#1416, #1615): the import pipeline's private knowledge of the cartridge, decoded once into the installed
/// options presentation.
/// </summary>
[EncodingOwner("cartridge addresses of the options screen pages, labels, headings and anchors")]
internal static class GameOptionsSourceDefinitions
{
    /// <summary>Bytes of one stored options page: its tilemap of two-byte cells.</summary>
    internal const int TilemapByteCount =
        GameOptionsPageDefinitions.MenuTilemapWidth * GameOptionsPageDefinitions.MenuTilemapHeight * sizeof(ushort);

    /// <summary>Bank $82, in which the options screen's sixteen-bit label pointers resolve.</summary>
    internal const int MenuBank = 0x820000;

    /// <summary>The bank-$97 compressed tilemap of each options page and language, as $82:EC66..ECB8 loads them.</summary>
    internal static readonly FrozenDictionary<GameOptionsScreen, GameOptionsPageResource> Pages =
        new Dictionary<GameOptionsScreen, GameOptionsPageResource>
        {
            [GameOptionsScreen.Primary] = new(0x978df4, "primary"),
            [GameOptionsScreen.Settings(GameOptionsPage.Controller, TextLanguage.English)] = new(0x978fcd, "English controller"),
            [GameOptionsScreen.Settings(GameOptionsPage.Controller, TextLanguage.Japanese)] = new(0x9791c4, "Japanese controller"),
            [GameOptionsScreen.Settings(GameOptionsPage.Special, TextLanguage.English)] = new(0x97938d, "English special-settings"),
            [GameOptionsScreen.Settings(GameOptionsPage.Special, TextLanguage.Japanese)] = new(0x97953a, "Japanese special-settings"),
        }.ToFrozenDictionary();

    /// <summary>
    /// ControllerButton_TilemapPointers at $82:F647: the bank-$82 label of each assignable button, X, A, B, Select, Y, L
    /// and R, each a 3-by-2 tilemap twelve bytes after the last.
    /// </summary>
    internal static FrozenDictionary<SuperMetroid.Core.Input.ControllerButton, ushort> ControllerLabelSources { get; } =
        new Dictionary<SuperMetroid.Core.Input.ControllerButton, ushort>
        {
            [SuperMetroid.Core.Input.ControllerButton.X] = 0xf659,
            [SuperMetroid.Core.Input.ControllerButton.A] = 0xf665,
            [SuperMetroid.Core.Input.ControllerButton.B] = 0xf671,
            [SuperMetroid.Core.Input.ControllerButton.Select] = 0xf67d,
            [SuperMetroid.Core.Input.ControllerButton.Y] = 0xf689,
            [SuperMetroid.Core.Input.ControllerButton.L] = 0xf695,
            [SuperMetroid.Core.Input.ControllerButton.R] = 0xf6a1,
        }.ToFrozenDictionary();

    /// <summary>The menu spritemap ordinal of each page's heading, as the instruction lists at $82:F47E/F48E/F49E select them.</summary>
    internal static readonly FrozenDictionary<GameOptionsPage, ushort> HeadingSpritemaps =
        new Dictionary<GameOptionsPage, ushort>
        {
            [GameOptionsPage.Primary] = 0x4b,
            [GameOptionsPage.Controller] = 0x4c,
            [GameOptionsPage.Special] = 0x4d,
        }.ToFrozenDictionary();

    /// <summary>$82:F36A, the immediate every page's heading row is loaded from.</summary>
    internal const int HeadingRowOperand = 0x82f36a;

    /// <summary>The immediate each page's heading column is loaded from, in the page setups at $82:F34B/F353/F35B.</summary>
    internal static readonly FrozenDictionary<GameOptionsPage, int> HeadingColumnOperands =
        new Dictionary<GameOptionsPage, int>
        {
            [GameOptionsPage.Primary] = 0x82f34c,
            [GameOptionsPage.Controller] = 0x82f354,
            [GameOptionsPage.Special] = 0x82f35c,
        }.ToFrozenDictionary();

    /// <summary>
    /// Each page's table of cursor rows, one entry of <see cref="CursorRowStride"/> bytes a row; the controller page's
    /// Exit and Reset rows already account for its scroll.
    /// </summary>
    internal static readonly FrozenDictionary<GameOptionsPage, int> CursorRowTables =
        new Dictionary<GameOptionsPage, int>
        {
            [GameOptionsPage.Primary] = 0x82f309,
            [GameOptionsPage.Controller] = 0x82f31d,
            [GameOptionsPage.Special] = 0x82f341,
        }.ToFrozenDictionary();

    /// <summary>Bytes from one cursor row's entry to the next.</summary>
    internal const int CursorRowStride = 4;

    /// <summary>The column each page's cursor missile is drawn at, in screen pixels.</summary>
    internal static readonly FrozenDictionary<GameOptionsPage, int> CursorColumns =
        new Dictionary<GameOptionsPage, int>
        {
            [GameOptionsPage.Primary] = 24,
            [GameOptionsPage.Controller] = 40,
            [GameOptionsPage.Special] = 16,
        }.ToFrozenDictionary();
}
