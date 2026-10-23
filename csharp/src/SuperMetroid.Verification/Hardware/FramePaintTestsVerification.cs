using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Hardware;

/// <summary>Checks a frame's paint against its sources and source-array ownership (#1403).</summary>
internal static class FramePaintTestsVerification
{
    /// <summary>Confirms CGRAM order, immutable capture of colours and draws, and size validation.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyFramePaintOwnership()
    {
        var cgram = new SnesCgram();
        var sprites = new SpriteFrameBuffer();
        for (int index = 0; index < SnesCgram.ColorCount; index++)
            cgram.SetColor(index, Bgr555.FromWord((ushort)(index * 97)));
        var image = new SpriteImage("Fixture", new(8, 8), new Rgba32[64], new byte[64],
            new SpriteImagePalette(SpriteImageColours.Sprite, [.. new Bgr555[SpriteImage.SpriteColourCount]]), new byte[64], new byte[64]);
        sprites.BeginFrame();
        sprites.Add(new ImageSpriteDraw(new(SpriteSourceKind.EscapeTimer), new SpriteSheetFrame(image, default, new(8, 8), default,
            SpritePaletteRows.None, []), new(10, 20), SpriteMirror.None));
        sprites.FinalizeFrame();
        Bgr555[] expectedPalette = cgram.Colors.ToArray();

        FramePaint first = FramePaint.Capture(cgram, sprites);
        FramePaint repeated = FramePaint.Capture(cgram, sprites);
        AssertTrue(first.Colors.SequenceEqual(expectedPalette), "CGRAM capture changed native words");
        AssertTrue(first.SpriteDraws.SequenceEqual(sprites.Draws), "capture keeps the frame's draws in emission order");
        AssertTrue(repeated.Colors.SequenceEqual(first.Colors) && repeated.SpriteDraws.SequenceEqual(first.SpriteDraws),
            "repeat capture changed the paint");
        AssertTrue(cgram.Colors.SequenceEqual(expectedPalette), "capture mutated its source");

        cgram.SetColor(1, new Bgr555(0, 0, 0));
        sprites.BeginFrame();
        sprites.FinalizeFrame();
        AssertTrue(first.Colors.SequenceEqual(expectedPalette) && first.SpriteDraws.Length == 1,
            "later simulation writes changed a published frame");

        var fromFixture = new FramePaint(expectedPalette, first.SpriteDraws);
        // Flip every color bit (the PPU stores fifteen).
        expectedPalette[0] = Bgr555.FromWord((ushort)(expectedPalette[0].ToWord() ^ 0x7fff));
        AssertTrue(fromFixture.Colors.SequenceEqual(first.Colors) && fromFixture.SpriteDraws.SequenceEqual(first.SpriteDraws),
            "fixture arrays escaped into frame ownership");

        AssertThrows<ArgumentException>(() => new FramePaint(ToColors(new ushort[1]), []), "short CGRAM");
        AssertThrows<ArgumentException>(() => new FramePaint(expectedPalette, default), "missing draw list");
        Console.WriteLine("Frame paint: CGRAM parity, draw retention, ownership and size checks passed.");
    }
}
