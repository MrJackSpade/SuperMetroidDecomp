using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Title.Artwork;
namespace SuperMetroid.Verification.Frontend.Title;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Frontend.Title;

/// <summary>Checks title-artwork selector coverage and editable OAM without entering gameplay.</summary>
internal static class TitleArtworkReferencesVerification
{
    /// <summary>Validates statically identified title selectors without running gameplay or reading a ROM.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyTitleArtworkReferences()
    {
        var frames = TitleSpriteDefinitions.All
            .Select(sprite => new TitleSpriteFrame { Sprite = sprite, Parts = [] }).ToArray();
        AssertEqual(TitleGraphicsFormat.SpriteFrameCount, frames.Length,
            "compiled title selectors require exactly 31 distinct artwork frames");
        var document = new TitleFloorMapDocument
        {
            Version = TitleGraphicsFormat.Version,
            Width = TitleGraphicsFormat.MapWidth,
            Height = TitleGraphicsFormat.MapHeight,
            Tiles = new int[TitleGraphicsFormat.MapWidth * TitleGraphicsFormat.MapHeight],
            Sprites = frames,
        };
        byte[] mode7 = Png(TitleGraphicsFormat.FloorWidth, TitleGraphicsFormat.FloorHeight, 256);
        byte[] objects = Png(TitleGraphicsFormat.ObjectWidth, TitleGraphicsFormat.ObjectHeight, 16);
        byte[] baby = Png(TitleGraphicsFormat.BabyWidth, TitleGraphicsFormat.BabyHeight, 256);
        _ = Load(document);
        using (var stream = new MemoryStream()) TitleGraphicsPresentation.WriteMap(stream, document);
        foreach (TitleSpriteFrame frame in frames)
            TitleGraphicsPresentation.DrawSprite(MenuSheetFixtures.WithTitle(Load(document)).Get(SpriteSheetId.Title), frame.Sprite, ScreenSpritesFixtures.Blank(), new(0, 0), 0);

        // An unchanged frame count must not disguise one missing selector. Test every
        // compiled selector independently; the old loader accepted each substitution.
        for (int index = 0; index < frames.Length; index++)
        {
            TitleSpriteFrame[] replacement = (TitleSpriteFrame[])frames.Clone();
            // Repeating the next composition in this slot leaves this one missing at an unchanged count.
            replacement[index] = replacement[index] with { Sprite = frames[(index + 1) % frames.Length].Sprite };
            TitleFloorMapDocument invalid = document with { Sprites = replacement };
            AssertThrows<InvalidDataException>(() => Load(invalid),
                $"loading rejects missing title composition {frames[index].Sprite} despite correct count");
            AssertThrows<InvalidDataException>(() =>
            {
                using var stream = new MemoryStream();
                TitleGraphicsPresentation.WriteMap(stream, invalid);
            }, $"authoring rejects missing title composition {frames[index].Sprite}");
        }
        _ = Load(document with { Sprites = frames.Reverse().ToArray() });
        var edited = (TitleSpriteFrame[])frames.Clone();
        edited[0] = edited[0] with
        {
            Parts = [new SpriteVisualPart
            {
                OffsetX = 3, OffsetY = -4, Size = 8, TileColumn = 1,
                TileRow = 2, Priority = 2, Palette = null, FlipX = true, FlipY = false,
            }],
        };
        TitleGraphicsPresentation selected = Load(document with { Sprites = edited });
        var oam = ScreenSpritesFixtures.Blank();
        TitleGraphicsPresentation.DrawSprite(MenuSheetFixtures.WithTitle(selected).Get(SpriteSheetId.Title), edited[0].Sprite, oam, new(20, 30), 0);
        oam.FinishFrame();
        AssertEqual(4, oam.NextByteOffset, "a required frame may still have editable OAM parts");
        AssertEqual((byte)23, oam.LowTable[0], "edited title X offset is applied");
        AssertEqual((byte)26, oam.LowTable[1], "edited title Y offset is applied");
        AssertThrows<InvalidDataException>(() => Load(document with { Sprites = frames[..^1] }),
            "missing title frame count is rejected");
        var duplicate = (TitleSpriteFrame[])frames.Clone();
        duplicate[0] = frames[1];
        AssertThrows<InvalidDataException>(() => Load(document with { Sprites = duplicate }),
            "duplicate title identities are rejected");
        Console.WriteLine("PASS title artwork: all 31 required selectors checked at load/write; frame ordering and editable OAM remain valid. No ROM or gameplay probes.");
        return;

        TitleGraphicsPresentation Load(TitleFloorMapDocument value)
        {
            using var mode7Stream = new MemoryStream(mode7, writable: false);
            using var mapStream = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(value,
                ArtworkJson.Options), writable: false);
            using var objectStream = new MemoryStream(objects, writable: false);
            using var babyStream = new MemoryStream(baby, writable: false);
            return TitleGraphicsCharacters.Read(mode7Stream, mapStream, objectStream, babyStream);
        }

        static byte[] Png(int width, int height, int colors)
        {
            using var stream = new MemoryStream();
            IndexedPng.Write(stream, new(checked((ushort)width), checked((ushort)height)), new byte[width * height],
                SnesGraphics.DiagnosticPalette(colors));
            return stream.ToArray();
        }
    }

    /// <summary>Runs the title selector coverage check as a standalone verification command.</summary>
    [VerificationCommand("--title-artwork-references")]
    internal static int TitleArtworkReferencesCommand()
    {
        Suite(nameof(VerifyTitleArtworkReferences), () => VerifyTitleArtworkReferences());
        return 0;
    }
}
