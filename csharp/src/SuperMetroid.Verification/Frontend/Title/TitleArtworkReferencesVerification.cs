using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Frontend.Title;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>Checks title-artwork selector coverage without entering gameplay.</summary>
internal static class TitleArtworkReferencesVerification
{
    /// <summary>Validates statically identified title selectors without running gameplay or reading a ROM.</summary>
    internal static void VerifyTitleArtworkReferences()
    {
        var frames = TitleGraphicsFormat.Sprites
            .Select(sprite => new TitleSpriteFrame { Sprite = sprite, Parts = [] }).ToArray();
        AssertEqual(TitleGraphicsFormat.SpriteFrameCount, frames.Length,
            "compiled title selectors require exactly 31 distinct artwork frames");
        var document = new TitleFloorMapDocument
        {
            Version = TitleGraphicsFormat.Version,
            Width = TitleGraphicsFormat.MapWidth,
            Height = TitleGraphicsFormat.MapHeight,
            // The installed floor map, so the title's floor composes its Baby Metroid cells; the check varies only the
            // sprite selectors (#1583).
            Tiles = [.. SuperMetroid.AssetExtraction.Frontend.Map.ImportAreaMapPresentation.TitleGraphicsOf(
                SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps).FloorMap.ToArray().Select(tile => (int)tile)],
            Sprites = frames,
        };
        byte[] mode7 = Png(TitleGraphicsFormat.FloorWidth, TitleGraphicsFormat.FloorHeight, 256);
        byte[] objects = Png(TitleGraphicsFormat.ObjectWidth, TitleGraphicsFormat.ObjectHeight, 16);
        byte[] baby = Png(TitleGraphicsFormat.BabyWidth, TitleGraphicsFormat.BabyHeight, 256);
        _ = Load(document);
        using (var stream = new MemoryStream()) TitleGraphicsCharacters.WriteMap(stream, document);
        foreach (TitleSpriteFrame frame in frames)
            TitleSpriteDrawing.DrawSprite(MenuSheetFixtures.WithTitle(Load(document)).Get(SpriteSheetId.Title), frame.Sprite, ScreenSpritesFixtures.Blank(), new(0, 0), TitleSpriteColouring.Text);

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
                TitleGraphicsCharacters.WriteMap(stream, invalid);
            }, $"authoring rejects missing title composition {frames[index].Sprite}");
        }
        _ = Load(document with { Sprites = frames.Reverse().ToArray() });
        AssertThrows<InvalidDataException>(() => Load(document with { Sprites = frames[..^1] }),
            "missing title frame count is rejected");
        var duplicate = (TitleSpriteFrame[])frames.Clone();
        duplicate[0] = frames[1];
        AssertThrows<InvalidDataException>(() => Load(document with { Sprites = duplicate }),
            "duplicate title identities are rejected");
        Console.WriteLine("PASS title artwork: all 31 required selectors checked at load/write; frame ordering remains valid. No ROM or gameplay probes.");
        return;

        TitleGraphicsCharacters Load(TitleFloorMapDocument value)
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
