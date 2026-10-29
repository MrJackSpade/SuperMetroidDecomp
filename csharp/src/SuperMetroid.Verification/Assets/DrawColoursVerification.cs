using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Checks #1405's resolved sprite-draw colours: a frame publishes each draw with the colours each of its part colour
/// groups shows and whether those pixels blend, and keeps the image's own colours where its group's store still holds them.
/// </summary>
internal static class DrawColoursVerification
{
    /// <summary>A drawn colouring, a part keeping its own group, and a group's store a palette effect changed.</summary>
    internal static void VerifyDrawColours()
    {
        // Two pixels: one of part group 2 the draw's colouring can reach, one whose part keeps group 3.
        ImageColourGroup reached = ImageColourGroup.Numbered(2), kept = ImageColourGroup.Numbered(3);
        PartColourGroup[] partGroups = [PartColourGroup.Following(reached), PartColourGroup.Keeping(kept)];
        var standard = new RgbColor[SpriteImage.SpriteColourCount];
        for (int colour = 0; colour < standard.Length; colour++)
            standard[colour] = new RgbColor(colour % 32, colour / 32, 1);
        byte[] indices = [2 * 16 + 5, 3 * 16 + 6];
        var sheet = new SpriteImage("Pair", new(2, 1), [standard[indices[0]].ToRgbaPixel(), standard[indices[1]].ToRgbaPixel()], indices,
            new SpriteImagePalette(SpriteImageColours.Sprite, [.. standard]), [0, 0], [.. partGroups.Select(part => (byte)part.Code)]);
        var frame = new SpriteSheetFrame(sheet, default, new(2, 1), default, ImageColourGroups.Of(reached, kept), []);
        var colourSets = new SpriteColourSets();
        colourSets.All.CopyFrom(standard);
        var white = new RgbColor(31, 31, 31);
        colourSets.Set(7).Fill(white);
        ShownColourSets colours = ShownColourSets.Of(colourSets);

        // The colouring, held in store 5, combines with part group 2 into store 7, which blends; the kept part shows group 3.
        var flashed = new ImageSpriteDraw(new(SpriteSourceKind.Enemy), frame, default, SpriteMirror.None, DrawnColouring.InFixtureSet(5));
        DrawColours shown = DrawColours.Resolve(flashed, colours);
        AssertTrue(shown.FollowsPartGroups, "a recoloured draw keys its pixels by part group");
        int overridden = shown.Key(ImageColourGroup.OfInk(indices[0]), partGroups[0]);
        int fixedPart = shown.Key(ImageColourGroup.OfInk(indices[1]), partGroups[1]);
        AssertEqual(white, shown.Colour(overridden, 5), "a following part group shows the store the colouring combines it into");
        AssertTrue(shown.Blends(overridden), "a following part group blends as the store it shows does");
        AssertEqual(standard[indices[1]], shown.Colour(fixedPart, 6), "a part keeping its group shows that group's store");
        AssertTrue(!shown.Blends(fixedPart), "a part keeping group 3 does not blend");
        AssertTrue(!shown.IsOwn, "a flashed draw is not drawn in its own colours");

        // Without a colouring, an untouched frame keeps the image's own colours; a changed store shows the frame's.
        var plain = new ImageSpriteDraw(new(SpriteSourceKind.Enemy), frame, default, SpriteMirror.None);
        AssertTrue(DrawColours.Resolve(plain, colours).IsOwn, "a store the frame does not use leaves a plain draw in its own colours");
        var untouched = new SpriteColourSets();
        untouched.All.CopyFrom(standard);
        AssertTrue(DrawColours.Resolve(plain, ShownColourSets.Of(untouched)).IsOwn, "a draw whose groups' stores hold its image's colours shows its own");
        untouched.Set(2).Write(5, white);
        DrawColours changed = DrawColours.Resolve(plain, ShownColourSets.Of(untouched));
        AssertTrue(!changed.FollowsPartGroups, "a plain draw keys its pixels by ink colour group");
        AssertEqual(white, changed.Colour(changed.Key(reached, partGroups[0]), 5), "a changed store shows the frame's colour");
        AssertTrue(changed.ShowsOwnColours(changed.Key(kept, partGroups[1])), "an unchanged store keeps the image's own colours");

        // The frame publishes its draws resolved.
        var paint = new FramePaint(new BackgroundColours(ShownBackgroundColourSets.Of(new BackgroundColourSets()), ShownColourSets.Of(untouched)), [plain]);
        AssertEqual(white, ((ImageSpriteDraw)paint.SpriteDraws[0]).Colours!.Colour(2, 5), "a frame publishes its draws' colours");
        // An ink group the part group does not name: a plain draw shows the ink group, as the hardware shows a pixel's index.
        ImageColourGroup inkGroup = ImageColourGroup.Numbered(4);
        PartColourGroup unnamedPart = PartColourGroup.Following(ImageColourGroup.Every[0]);
        var inked = new SpriteImage("Inked", new(1, 1), [standard[4 * 16 + 1].ToRgbaPixel()], [4 * 16 + 1],
            new SpriteImagePalette(SpriteImageColours.Sprite, [.. standard]), [0], [(byte)unnamedPart.Code]);
        var inkedFrame = new SpriteSheetFrame(inked, default, new(1, 1), default, ImageColourGroups.Of(inkGroup), []);
        untouched.Set(4).Write(1, white);
        DrawColours ink = DrawColours.Resolve(new ImageSpriteDraw(new(SpriteSourceKind.Enemy), inkedFrame, default, SpriteMirror.None), ShownColourSets.Of(untouched));
        AssertEqual(white, ink.Colour(ink.Key(inkGroup, unnamedPart), 1), "a plain draw shows its pixel's ink group, not its part group");
        AssertTrue(ink.Blends(ink.Key(inkGroup, unnamedPart)), "a pixel inked in group 4 blends");
    }

    /// <summary>Runs the resolved draw-colour confirmation.</summary>
    [VerificationCommand("--draw-colours")]
    internal static int DrawColoursCommand()
    {
        Suite(nameof(VerifyDrawColours), VerifyDrawColours);
        return 0;
    }
}
