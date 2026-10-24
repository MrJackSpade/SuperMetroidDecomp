using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Title.Artwork;
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
    [PpuLayoutBoundary("exact hardware test: reads the Baby Metroid characters' native Mode 7 VRAM high bytes")]
    internal static void VerifyTitleScreenPublication()
    {
        var maps = InstalledFixtures.Maps;
        var title = new TitleSequenceState(titleGradientPresentation: maps.TitleGradient,
            titlePalettePresentation: maps.TitlePalette, titleGraphicsPresentation: maps.TitleGraphics, spriteSheets: MenuSheetFixtures.WithTitle(maps.TitleGraphics));
        SpriteSheet titleSheet = MenuSheetFixtures.WithTitle(maps.TitleGraphics).Get(SpriteSheetId.Title);
        byte[] baby = maps.TitleGraphics.BabyCharacters.ToArray();
        int pageBytes = TitleSequenceRomData.Transfers.BabyCharacterPageByteCount;
        var shownPages = new List<int>();
        bool drewSprites = false;
        for (int frame = 0; frame < 9 * TitleSequenceRomData.Timing.BabyFrameDuration; frame++)
        {
            FloorScene snapshot = title.CaptureRenderSnapshot();
            byte[] snapshotVram = title.ShownVram();
            var shownBytes = new byte[pageBytes];
            for (int pixel = 0; pixel < pageBytes; pixel++)
                shownBytes[pixel] = snapshotVram[(TitleVramLayout.BabyMetroidCharacters.Offset + pixel) * 2 + 1];
            int page = Enumerable.Range(0, 3).Single(candidate => baby.AsSpan(candidate * pageBytes, pageBytes).SequenceEqual(shownBytes));
            if (shownPages.Count == 0 || shownPages[^1] != page)
                shownPages.Add(page);
            AssertTrue(snapshot.Paint.Colors.SequenceEqual(title.palette.Colors), $"frame {frame} draws the title's own palette");
            AssertTrue(PaintedSpriteEntries.AreOwn(snapshot.Paint.SpriteDraws, title.objects.Draws),
                $"frame {frame} draws the title's own sprite entries");
            foreach (var draw in snapshot.Paint.SpriteDraws)
                AssertTrue(draw is ImageSpriteDraw image && ReferenceEquals(titleSheet.Image, image.Image.Sheet),
                    $"frame {frame} sprite entries draw from the title sheet");
            drewSprites |= snapshot.Paint.SpriteDraws.Length > 0;
            title.Step(0);
        }
        AssertTrue(drewSprites, "the title cards place sprite entries");
        AssertTrue(shownPages.Count >= 8 && shownPages.Select((page, index) => page == new[] { 0, 1, 2, 1 }[(index + Array.IndexOf(new[] { 0, 1, 2, 1 }, shownPages[0])) % 4]).All(match => match),
            $"the shown Baby Metroid page cycles 0, 1, 2, 1 ({string.Join(",", shownPages)})");

        Rgba32[] shown = DisplayRendering.Render(new DisplayPacket(DisplayRendering.NextFrame(), title.CaptureRenderSnapshot()));
        using var state = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(state, title);
        state.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<TitleSequenceState>(state);
        restored.BindTitleGradient(maps.TitleGradient);
        restored.BindTitlePalette(maps.TitlePalette);
        restored.BindFixtureTitleGraphics(maps.TitleGraphics);
        AssertTrue(shown.SequenceEqual(DisplayRendering.Render(new DisplayPacket(DisplayRendering.NextFrame(), restored.CaptureRenderSnapshot()))),
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
