using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Tooling;

namespace SuperMetroid.Verification.Frontend.Menus;

/// <summary>Checks options-screen cursor positions and controller label transfer geometry against bank-$82 words.</summary>
internal static class OptionsGeometryLookupsVerification
{
    /// <summary>Checks the five installed primary-menu cursor heights against the four-byte-stride native table.</summary>
    internal static void VerifyOptionsPrimaryCursorY(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsCursorRows), () => VerifyOptionsCursorRows(rom, 0x82f309, 5, GameOptionsPage.Primary, "primary cursor Y"));

    /// <summary>Checks the nine installed controller-menu cursor heights against the four-byte-stride native table.</summary>
    internal static void VerifyOptionsControllerCursorY(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsCursorRows), () => VerifyOptionsCursorRows(rom, 0x82f31d, 9, GameOptionsPage.Controller, "controller cursor Y"));

    /// <summary>Checks the three installed special-menu cursor heights against the four-byte-stride native table.</summary>
    internal static void VerifyOptionsSpecialCursorY(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsCursorRows), () => VerifyOptionsCursorRows(rom, 0x82f341, 3, GameOptionsPage.Special, "special cursor Y"));

    /// <summary>Compares the extracted cursor anchors of <paramref name="page"/> with a native table of four-byte rows.</summary>
    private static void VerifyOptionsCursorRows(ISnesAddressSpace rom, int address, int count, GameOptionsPage page, string name)
    {
        var document = RequiredJson.Deserialize<GameOptionsPresentationDocument>(GameOptionsPresentationExtractor.Extract(rom), ArtworkJson.Options);
        Position16<SpritePixels>[] anchors = document.CursorAnchors[GameOptionsPresentationFormat.MenuName(page)];
        AssertEqual(count, anchors.Length, $"{name} row count");
        for (int index = 0; index < count; index++)
            AssertEqual((int)ReadVerificationWord(rom, address + 4 * index), anchors[index].Y, $"{name} native word {index}");
    }

    /// <summary>Checks the seven controller-label boxes against the native byte offsets their labels are drawn at.</summary>
    internal static void VerifyOptionsLabelDestinations(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f639, 2, 7,
            action => checked((ushort)((GameOptionsPageCells.ControllerLabelAnchor(action).Y * GameOptionsPageDefinitions.MenuTilemapWidth
                + GameOptionsPageCells.ControllerLabelAnchor(action).X) * sizeof(ushort))), "controller label box"));

    /// <summary>Checks seven source words used by controller-label transfers.</summary>
    internal static void VerifyOptionsLabelSources(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyOptionsGeometryWords), () => VerifyOptionsGeometryWords(rom, 0x82f647, 2, 7,
            index => GameOptionsSourceDefinitions.ControllerLabelSources[index], "controller label source"));

    /// <summary>Compares an indexed native geometry sequence and rejects indexes beyond its declared count.</summary>
    private static void VerifyOptionsGeometryWords(ISnesAddressSpace rom, int address,
        int stride, int count, Func<int, int> actual, string name)
    {
        for (int index = 0; index < count; index++)
            AssertEqual(ReadVerificationWord(rom, address + stride * index), actual(index),
                $"{name} native word {index}");
        foreach (int index in new[] { int.MinValue, -1, count, count + 1, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => actual(index),
                $"{name} rejects {index}");
    }
}
