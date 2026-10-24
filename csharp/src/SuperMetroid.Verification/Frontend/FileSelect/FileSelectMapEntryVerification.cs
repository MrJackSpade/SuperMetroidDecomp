using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map.Artwork;
namespace SuperMetroid.Verification.Frontend.FileSelect;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Checks the file-select map entry's palette fade, transfer boundaries, and opening window.</summary>
internal static class FileSelectMapEntryVerification
{
    /// <summary>Verifies two CGRAM cells fade in sixteen steps before the exact twenty-eight-update reveal rectangle.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyFileSelectMapEntry()
    {
        var white = Enumerable.Repeat(new PaletteRgb5 { Red = 31, Green = 31, Blue = 31 },
            SnesCgram.ColorCount).ToArray();
        var document = new MapStaticPalettesDocument
        {
            Version = MapStaticPalettesFormat.Version,
            Pause = white,
            FileSelect = white,
            World = Enum.GetValues<AreaId>().Where(area => area != AreaId.Ceres)
                .ToDictionary(area => area.ToString(), _ => white),
        };
        using var json = new MemoryStream();
        MapStaticPalettesFile.Write(json, document);
        json.Position = 0;
        MapStaticPalettes palettes = MapStaticPalettesFile.Read(json);
        var entry = new FileSelectMapEntry(palettes);
        AssertEqual(0x7fff, entry.Palette.Colors[14], "entry setup does not prematurely fade palette");
        AssertEqual(FileSelectMapEntryPhase.PaletteFade, entry.Phase, "entry starts in its forced-black palette fade");
        entry.Step();
        AssertEqual(0x739c, entry.Palette.Colors[14], "gradual palette begins at step one, not global-fade no-op");
        AssertEqual(0x739c, entry.Palette.Colors[30], "both blank-cell palette entries fade together");
        for (int tick = 2; tick <= 16; tick++) entry.Step();
        AssertEqual(0, entry.Palette.Colors[14], "sixteenth gradual step reaches exact black");
        AssertEqual(FileSelectMapEntryPhase.PaletteFade, entry.Phase, "palette endpoint retains native completion boundary");
        entry.Step();
        AssertEqual(FileSelectMapEntryPhase.LoadForeground, entry.Phase, "seventeenth call completes gradual palette fade");
        for (int color = 0; color < 256; color++)
            AssertEqual(color is 14 or 30 ? 0 : 0x7fff, entry.Palette.Colors[color], "entry changes only the two target colors");
        entry.Step();
        AssertEqual(FileSelectMapEntryPhase.LoadBackground, entry.Phase, "foreground transfer has its own entry boundary");
        entry.Step();
        AssertEqual(FileSelectMapEntryPhase.SetupWindow, entry.Phase, "background transfer has its own entry boundary");
        entry.Step();
        AssertEqual(FileSelectMapEntryPhase.Revealing, entry.Phase, "window setup precedes first expansion update");
        for (int tick = 0; tick <= 27; tick++)
        {
            int left = 127 - 4 * tick, right = 129 + 4 * tick;
            int top = 111 - 4 * tick, bottom = 113 + 4 * tick;
            AssertEqual(left, entry.Left, "entry reveal window left edge");
            AssertEqual(right, entry.Right, "entry reveal window right edge (inclusive)");
            AssertEqual(top, entry.Top, "entry reveal window top edge");
            AssertEqual(bottom, entry.Bottom, "entry reveal window bottom edge (exclusive)");
            AssertTrue(!entry.IsComplete, "entry window remains active before signed margin underflow");
            entry.Step();
        }
        AssertTrue(entry.IsComplete, "twenty-eighth reveal update disables the window");
    }

    /// <summary>Runs the focused file-select map entry transition check.</summary>
    [VerificationCommand("--file-select-map-entry")]
    internal static int FileSelectMapEntryCommand()
    {
        Suite(nameof(VerifyFileSelectMapEntry), () => VerifyFileSelectMapEntry());
        return 0;
    }
}
