using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Checks #1405's resolved sprite-draw colours: a frame publishes each draw with the colours each of its part rows
/// shows and whether those pixels blend, and keeps the image's own colours where its row still holds them.
/// </summary>
internal static class DrawColoursVerification
{
    /// <summary>An override, a part naming its own row, and a row a palette effect changed.</summary>
    internal static void VerifyDrawColours()
    {
        // Two pixels: one of part row 2 the draw's override can reach, one whose part names row 3.
        var standard = new RgbColor[SpriteImage.SpriteColourCount];
        for (int colour = 0; colour < standard.Length; colour++)
            standard[colour] = new RgbColor(colour % 32, colour / 32, 1);
        byte[] indices = [2 * 16 + 5, 3 * 16 + 6];
        byte[] partRows = [2, 3 | SpriteImage.FixedPartRow];
        var sheet = new SpriteImage("Pair", new(2, 1), [standard[indices[0]].ToRgbaPixel(), standard[indices[1]].ToRgbaPixel()], indices,
            new SpriteImagePalette(SpriteImageColours.Sprite, [.. standard]), [0, 0], partRows);
        var frame = new SpriteSheetFrame(sheet, default, new(2, 1), default,
            SpritePaletteRows.None | SpritePaletteRowSet.Rows[2] | SpritePaletteRowSet.Rows[3], []);
        var colours = new RgbColor[ScreenPalette.ColorCount];
        standard.CopyTo(colours, ScreenPalette.ColorCount / 2);
        var white = new RgbColor(31, 31, 31);
        for (int colour = 0; colour < 16; colour++)
            colours[ScreenPalette.ColorCount / 2 + 7 * 16 + colour] = white;

        // The override ORs row 5 into part row 2, which shows row 7 and blends; the fixed part keeps row 3.
        var flashed = new ImageSpriteDraw(new(SpriteSourceKind.Enemy), frame, default, SpriteMirror.None, DrawnColouring.InUnnamedRow(5));
        DrawColours shown = DrawColours.Resolve(flashed, colours);
        AssertTrue(shown.FollowsPartRows, "an overridden draw keys its pixels by part row");
        int overridden = shown.Key(indices[0] / 16, partRows[0]), fixedPart = shown.Key(indices[1] / 16, partRows[1]);
        AssertEqual(white, shown.Colour(overridden, 5), "an overridden part row shows the row the override reaches");
        AssertTrue(shown.Blends(overridden), "an overridden part row blends as the row it reaches does");
        AssertEqual(standard[indices[1]], shown.Colour(fixedPart, 6), "a part naming its row shows that row");
        AssertTrue(!shown.Blends(fixedPart), "a part naming row 3 does not blend");
        AssertTrue(!shown.IsOwn, "a flashed draw is not drawn in its own colours");

        // Without an override, an untouched frame keeps the image's own colours; a changed row shows the frame's.
        var plain = new ImageSpriteDraw(new(SpriteSourceKind.Enemy), frame, default, SpriteMirror.None);
        AssertTrue(DrawColours.Resolve(plain, colours).IsOwn, "a row the frame does not use leaves a plain draw in its own colours");
        var untouched = new RgbColor[ScreenPalette.ColorCount];
        standard.CopyTo(untouched, ScreenPalette.ColorCount / 2);
        AssertTrue(DrawColours.Resolve(plain, untouched).IsOwn, "a draw whose rows hold its image's colours shows its own");
        untouched[ScreenPalette.ColorCount / 2 + 2 * 16 + 5] = white;
        DrawColours changed = DrawColours.Resolve(plain, untouched);
        AssertTrue(!changed.FollowsPartRows, "a plain draw keys its pixels by ink row");
        AssertEqual(white, changed.Colour(changed.Key(2, partRows[0]), 5), "a changed row shows the frame's colour");
        AssertTrue(changed.ShowsOwnColours(changed.Key(3, partRows[1])), "an unchanged row keeps the image's own colours");

        // The frame publishes its draws resolved.
        var paint = new FramePaint(untouched, [plain]);
        AssertEqual(white, ((ImageSpriteDraw)paint.SpriteDraws[0]).Colours!.Colour(2, 5), "a frame publishes its draws' colours");
        // An ink row the part row does not name: a plain draw shows the ink row, as the hardware shows a pixel's index.
        var inked = new SpriteImage("Inked", new(1, 1), [standard[4 * 16 + 1].ToRgbaPixel()], [4 * 16 + 1],
            new SpriteImagePalette(SpriteImageColours.Sprite, [.. standard]), [0], [0]);
        var inkedFrame = new SpriteSheetFrame(inked, default, new(1, 1), default, SpritePaletteRows.None | SpritePaletteRowSet.Rows[4], []);
        untouched[ScreenPalette.ColorCount / 2 + 4 * 16 + 1] = white;
        DrawColours ink = DrawColours.Resolve(new ImageSpriteDraw(new(SpriteSourceKind.Enemy), inkedFrame, default, SpriteMirror.None), untouched);
        AssertEqual(white, ink.Colour(ink.Key(4, 0), 1), "a plain draw shows its pixel's ink row, not its part row");
        AssertTrue(ink.Blends(ink.Key(4, 0)), "a pixel inked in row 4 blends");
    }

    /// <summary>Runs the resolved draw-colour confirmation.</summary>
    [VerificationCommand("--draw-colours")]
    internal static int DrawColoursCommand()
    {
        Suite(nameof(VerifyDrawColours), VerifyDrawColours);
        return 0;
    }
}
