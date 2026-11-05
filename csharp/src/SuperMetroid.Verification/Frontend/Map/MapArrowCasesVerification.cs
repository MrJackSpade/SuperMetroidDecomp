using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks map arrow direction selection, the installed presentation, and native shape mapping.</summary>
internal static class MapArrowCasesVerification
{
    /// <summary>Checks extracted directions, direction bounds and required named directions.</summary>
    internal static void VerifyMapArrowCases(ISnesAddressSpace rom)
    {
        byte[] bytes = MapArrowExtractor.Extract(rom);
        var original = MapArrowFile.Read(new MemoryStream(bytes));
        Suite(nameof(VerifyMapArrowXSelection), () => VerifyMapArrowXSelection(rom, original));
        Suite(nameof(VerifyMapArrowYSelection), () => VerifyMapArrowYSelection(rom, original));
        Suite(nameof(VerifyMapArrowShapeCases), () => VerifyMapArrowShapeCases(rom));
        var document = RequiredJson.Deserialize<MapArrowDocument>(bytes,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        foreach (int invalid in new[] { int.MinValue, -1, 0, 5, 256, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => original.Get((MapScrollDirection)invalid), "invalid arrow direction");
        foreach (string name in document.Arrows.Keys)
        {
            var missing = new Dictionary<string, MapArrowEntry>(document.Arrows);
            missing.Remove(name);
            AssertThrows<InvalidDataException>(() =>
            {
                using var written = new MemoryStream();
                MapArrowFile.Write(written, document with { Arrows = missing });
                _ = MapArrowFile.Read(new MemoryStream(written.ToArray(), writable: false));
            }, "all four named directions remain required on read");
        }
    }

    /// <summary>Compares four installed arrow X positions with native direction records.</summary>
    private static void VerifyMapArrowXSelection(ISnesAddressSpace rom, MapArrowPresentation presentation)
    {
        for (int index = 0; index < 4; index++)
            AssertEqual(ReadVerificationWord(rom, 0x81af32 + 10 * index),
                presentation.Get((MapScrollDirection)(index + 1)).Position.X, "original arrow X selected by direction");
    }

    /// <summary>Compares arrow Y positions after the native one-pixel OAM bias.</summary>
    private static void VerifyMapArrowYSelection(ISnesAddressSpace rom, MapArrowPresentation presentation)
    {
        for (int index = 0; index < 4; index++)
            AssertEqual((ushort)(ReadVerificationWord(rom, 0x81af34 + 10 * index) - 1),
                presentation.Get((MapScrollDirection)(index + 1)).Position.Y, "original arrow Y selected by direction with native OAM bias");
    }

    /// <summary>Checks each direction's sprite base through the native animation and variant tables.</summary>
    private static void VerifyMapArrowShapeCases(ISnesAddressSpace rom)
    {
        for (int index = 0; index < 4; index++)
        {
            int animation = ReadVerificationWord(rom, 0x81af36 + 10 * index);
            int variants = ReadVerificationWord(rom, 0x82c1e4 + 2 * (animation - 1));
            AssertEqual(ReadVerificationWord(rom, 0x820000 | variants),
                MapSpriteOrdinals.Spritemaps.Word(MapArrowSprites.SpriteBase((MapScrollDirection)(index + 1))), "original direction shape selector");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 0, 5, 256, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => MapArrowSprites.SpriteBase((MapScrollDirection)invalid),
                "unsupported arrow shape direction");
    }
}
