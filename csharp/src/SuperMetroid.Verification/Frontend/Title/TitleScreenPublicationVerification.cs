using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Frontend.Title;

/// <summary>Checks that the title publishes its artwork, Baby Metroid page, palette and sprite entries (#1358).</summary>
internal static class TitleScreenPublicationVerification
{
    /// <summary>
    /// Across two Baby Metroid cycles the captured image shows source pages 0, 1, 2, 1 in the Mode 7 characters at word
    /// $3800, and every frame draws the title's own palette and sprite entries. A debugger round trip keeps the title
    /// state and, once the artwork is rebound, shows the same frame.
    /// </summary>
    internal static void VerifyTitleScreenPublication()
    {
        var maps = InstalledFixtures.Maps;
        TitleGraphicsCharacters graphics = InstalledFixtures.ImportOf(maps).TitleGraphics;
        var title = new TitleSequenceState(titleGradientPresentation: maps.TitleGradient,
            titleBackdrop: maps.TitleBackdrop, spriteSheets: MenuSheetFixtures.WithTitle(graphics));
        SpriteSheet titleSheet = MenuSheetFixtures.WithTitle(graphics).Get(SpriteSheetId.Title);
        byte[] baby = graphics.BabyCharacters.ToArray();
        int pageBytes = TitleFloorTileDefinitions.BabyCharacterPageByteCount;
        var shownPages = new List<int>();
        bool drewSprites = false;
        for (int frame = 0; frame < 9 * TitleSequenceDefinitions.Timing.BabyFrameDuration; frame++)
        {
            FloorScene snapshot = title.CaptureRenderSnapshot();
            byte[] snapshotVram = title.ShownVram();
            var shownBytes = new byte[pageBytes];
            for (int pixel = 0; pixel < pageBytes; pixel++)
                shownBytes[pixel] = snapshotVram[(TitleVramLayout.BabyMetroidCharacters.Offset + pixel) * 2 + 1];
            int page = Enumerable.Range(0, 3).Single(candidate => baby.AsSpan(candidate * pageBytes, pageBytes).SequenceEqual(shownBytes));
            if (shownPages.Count == 0 || shownPages[^1] != page)
                shownPages.Add(page);
            AssertEqual(maps.TitleBackdrop, snapshot.Paint.Backdrop, $"frame {frame} shows the title's own backdrop");
            AssertTrue(PaintedSpriteEntries.AreOwn(snapshot.Paint.SpriteDraws, title.objects.Draws),
                $"frame {frame} draws the title's own sprite entries");
            foreach (var draw in snapshot.Paint.SpriteDraws)
                AssertTrue(draw is ImageSpriteDraw { Shading: not null } image && ReferenceEquals(titleSheet.Image, image.Image.Sheet),
                    $"frame {frame} sprite entries draw from the title sheet in a shading they name");
            drewSprites |= snapshot.Paint.SpriteDraws.Length > 0;
            title.Step(0);
        }
        AssertTrue(drewSprites, "the title cards place sprite entries");
        AssertTrue(shownPages.Count >= 8 && shownPages.Select((page, index) => page == new[] { 0, 1, 2, 1 }[(index + Array.IndexOf(new[] { 0, 1, 2, 1 }, shownPages[0])) % 4]).All(match => match),
            $"the shown Baby Metroid page cycles 0, 1, 2, 1 ({string.Join(",", shownPages)})");

        RgbaPixel[] shown = DisplayRendering.Render(new DisplayPacket(0, title.CaptureRenderSnapshot()));
        using var state = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(state, title);
        state.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<TitleSequenceState>(state);
        restored.BindTitleGradient(maps.TitleGradient);
        restored.BindTitleBackdrop(maps.TitleBackdrop);
        restored.BindFixtureTitleGraphics(graphics);
        AssertTrue(shown.SequenceEqual(DisplayRendering.Render(new DisplayPacket(0, restored.CaptureRenderSnapshot()))),
            "a restored title shows the same frame once its artwork is rebound");
        Console.WriteLine("  Title screen: the Baby Metroid page cycle, palette and sprite entries come from the title's published state and survive a debugger round trip.");
    }

    /// <summary>Runs the title screen publication check.</summary>
    [VerificationCommand("--title-screen")]
    internal static int TitleScreenCommand()
    {
        Suite(nameof(VerifyTitleScreenPublication), () => VerifyTitleScreenPublication());
        return 0;
    }
}
