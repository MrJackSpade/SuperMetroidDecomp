using SuperMetroid.AssetExtraction.Artwork;
using System.Collections.Immutable;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using SuperMetroid.Core.Assets;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Hardware;

/// <summary>Checks a native-coloured frame's paint against its sources and source-array ownership (#1403, #1512).</summary>
internal static class FramePaintTestsVerification
{
    /// <summary>Confirms CGRAM order, immutable capture of colours and draws, and size validation.</summary>
    internal static void VerifyFramePaintOwnership()
    {
        var cgram = new SnesCgram();
        var sprites = new SpriteFrameBuffer();
        for (int index = 0; index < SnesCgram.ColorCount; index++)
            cgram.SetColor(index, RgbColor.FromWord((ushort)(index * 97)));
        var image = new SpriteImage("Fixture", new(8, 8), new RgbaPixel[64], new byte[64],
            new SpriteImagePalette(SpriteImageColours.Sprite, [.. new RgbColor[SpriteImage.SpriteColourCount]]), new byte[64]);
        // The fixture picture is kept as an installed sprite sheet's would be, with no palette bits, so a capture colours
        // its inks from the cartridge colours as the loader's pictures are (#1512).
        SuperMetroid.AssetExtraction.Artwork.InstalledPictureColours.Record(image, [], paletteBits: 0);
        sprites.BeginFrame();
        sprites.Add(new ImageSpriteDraw(new(SpriteSourceKind.EscapeTimer), new SpriteSheetFrame(image, default, new(8, 8), default,
            ImageColourGroups.None), new(10, 20), SpriteMirror.None));
        sprites.FinalizeFrame();
        // The frame's own draws, which draw the recorded fixture picture; a capture's draws draw native-coloured copies of it,
        // which no sheet loader recorded, so a repaint starts from these.
        ImmutableArray<SpriteDraw> fixtureDraws = sprites.Draws;
        RgbColor[] expectedPalette = cgram.Colors.ToArray();

        FramePaint first = FramePaintConstants.Capture(cgram, sprites);
        FramePaint repeated = FramePaintConstants.Capture(cgram, sprites);
        AssertTrue(Shown(first).SequenceEqual(NativeShown(expectedPalette)), "CGRAM capture changed native words");
        AssertTrue(Order(first.SpriteDraws).SequenceEqual(Order(sprites.Draws)), "capture keeps the frame's draws in emission order");
        AssertTrue(Shown(repeated).SequenceEqual(Shown(first)) && Order(repeated.SpriteDraws).SequenceEqual(Order(first.SpriteDraws)),
            "repeat capture changed the paint");
        AssertTrue(cgram.Colors.SequenceEqual(expectedPalette), "capture mutated its source");

        cgram.SetColor(1, new RgbColor(0, 0, 0));
        sprites.BeginFrame();
        sprites.FinalizeFrame();
        AssertTrue(Shown(first).SequenceEqual(NativeShown(expectedPalette)) && first.SpriteDraws.Length == 1,
            "later simulation writes changed a published frame");

        var fromFixture = FramePaint.FromNative(expectedPalette, fixtureDraws);
        // Flip every color bit (the PPU stores fifteen).
        expectedPalette[0] = RgbColor.FromWord((ushort)(expectedPalette[0].ToWord() ^ 0x7fff));
        AssertTrue(Shown(fromFixture).SequenceEqual(Shown(first)) && Order(fromFixture.SpriteDraws).SequenceEqual(Order(first.SpriteDraws)),
            "fixture arrays escaped into frame ownership");

        AssertThrows<ArgumentException>(() => FramePaint.FromNative(ToColors(new ushort[1]), []), "short CGRAM");
        AssertThrows<ArgumentException>(() => FramePaint.FromNative(expectedPalette, default), "missing draw list");
        Console.WriteLine("Frame paint: CGRAM parity, draw retention, ownership and size checks passed.");

        // A native frame shows native colour 0 as its backdrop and draws each picture as a copy coloured from the native
        // image (#1512): the fixture's sprite picture takes the sprite half.
        static RgbColor[] Shown(FramePaint paint) =>
            [paint.Backdrop, .. paint.SpriteDraws.Cast<ImageSpriteDraw>().SelectMany(draw => draw.Image.Sheet.StandardColours.ToArray())];
        static RgbColor[] NativeShown(RgbColor[] native) => [native[0], .. native.AsSpan(SnesCgram.ColorCount / 2)];

        // A native-coloured frame draws a copy of each picture (#1512), so a draw is known by its source and where, in order.
        static IEnumerable<(SpriteSource Source, Position16<ScreenPixels> Origin)> Order(ImmutableArray<SpriteDraw> draws) =>
            draws.Select(draw => (draw.Source, draw.Origin));
    }
}
