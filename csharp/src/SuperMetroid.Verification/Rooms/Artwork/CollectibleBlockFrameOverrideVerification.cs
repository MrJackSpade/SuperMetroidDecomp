using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms.Artwork;

/// <summary>
/// Checks the room block sheet is the way a collectible is restyled (#1453): repainting, in an override of the sheet, the
/// pictures the sheet names for a collectible over its graphics slot changes what the slot's blocks show in a room.
/// </summary>
internal static class CollectibleBlockFrameOverrideVerification
{
    /// <summary>Displayed frames the Morph Ball room is followed over, as the room block picture check follows it.</summary>
    private const int Frames = 40;

    /// <summary>The collectible the Morph Ball room shows over a graphics slot.</summary>
    private const InWorldCollectibleKind ShownItem = InWorldCollectibleKind.MorphBall;

    /// <summary>
    /// In the Morph Ball room, every picture BG1 shows for a graphics slot's block while the Morph Ball is shown over the
    /// slot is found with the stock sheet; an override of the room tileset's sheet that inverts the colours of exactly
    /// those pictures is written to a scratch override directory; the room loaded with the overridden sheet then shows, on
    /// each of the same frames and for each of the same blocks, the same picture, every opaque pixel of it the inverse of
    /// the stock pixel and so different from it.
    /// </summary>
    internal static void VerifyCollectibleFrameOverride()
    {
        GameInstallation installation = RepositoryInstallation.Installation;
        SuperMetroidRuntime stock = LoadMorphBall(installation.LoadRoomBlockSheets());
        string sheetName = RoomBlockSheet.NameOf((stock.Room.Scope ?? throw new InvalidOperationException("The Morph Ball room did not load.")).Assets.Tileset);
        var shown = new List<ShownSlotPicture>();
        for (int frame = 0; frame < Frames; frame++)
        {
            stock.StepFrame(0);
            RoomBlockPictureTable table = PicturesOf(stock, $"stock frame {frame}");
            for (int block = 0; block < table.Blocks; block++)
            {
                if (table.Picture(block) is { } picture && ShowsCollectible(stock, block))
                    shown.Add(new ShownSlotPicture(frame, block, picture));
            }
        }
        AssertTrue(shown.Count > 0, "the Morph Ball room shows its item through a graphics slot's block pictures");

        using var scratch = new TestScratchDirectory("collectible-block-frame");
        Directory.CreateDirectory(scratch.Root);
        string imageFile = SpriteSheetStore.ImageFile(sheetName);
        RgbaPixel[] pixels = RgbaPng.Read(new MemoryStream(File.ReadAllBytes(Path.Combine(installation.RoomBlockSheetDirectory, imageFile)),
            writable: false), out Extent16<ImagePixels> size);
        foreach (Position16<ImagePixels> corner in shown.Select(entry => entry.Picture.Corner).Distinct())
        {
            for (int y = 0; y < RoomBlockSheet.BlockPixels; y++)
            for (int x = 0; x < RoomBlockSheet.BlockPixels; x++)
            {
                int index = (corner.Y + y) * size.X + corner.X + x;
                if (pixels[index].A != 0)
                    pixels[index] = Inverted(pixels[index]);
            }
        }
        using (FileStream edited = File.Create(Path.Combine(scratch.Root, imageFile)))
            RgbaPng.Write(edited, size, pixels);

        SuperMetroidRuntime changed = LoadMorphBall(RoomBlockSheetFiles.Load(installation.RoomBlockSheetDirectory, scratch.Root));
        int compared = 0;
        for (int frame = 0; frame < Frames; frame++)
        {
            changed.StepFrame(0);
            RoomBlockPictureTable table = PicturesOf(changed, $"overridden frame {frame}");
            foreach (ShownSlotPicture entry in shown.Where(entry => entry.Frame == frame))
            {
                string where = $"frame {frame} block {entry.Block}";
                AssertTrue(ShowsCollectible(changed, entry.Block), $"{where}: the overridden room shows the Morph Ball over the same slot");
                SpriteSheetFrame after = table.Picture(entry.Block)
                    ?? throw new InvalidOperationException($"{where}: the overridden room shows the slot's block with no picture.");
                SpriteSheetFrame before = entry.Picture;
                AssertEqual(before.Corner, after.Corner, $"{where}: the overridden room shows the picture named for the Morph Ball over the slot");
                for (int y = 0; y < RoomBlockSheet.BlockPixels; y++)
                for (int x = 0; x < RoomBlockSheet.BlockPixels; x++)
                {
                    RgbaPixel stockPixel = before.Sheet.Pixels[(before.Corner.Y + y) * before.Sheet.Size.X + before.Corner.X + x];
                    if (stockPixel.A == 0)
                        continue;
                    RgbaPixel edited = after.Sheet.Pixels[(after.Corner.Y + y) * after.Sheet.Size.X + after.Corner.X + x];
                    AssertEqual(Inverted(stockPixel), edited, $"{where}: the slot's picture shows the edited frame at ({x}, {y})");
                    AssertTrue(edited != stockPixel, $"{where}: the slot's picture differs from stock at ({x}, {y})");
                    compared++;
                }
            }
        }
        AssertTrue(compared > 0, "the overridden collectible frames are compared at opaque pixels");
        Console.WriteLine($"Collectible block frame override: {shown.Count} slot pictures of the Morph Ball show the edited frames, " +
            $"{compared} opaque pixels compared.");
    }

    /// <summary>Runs the collectible block frame override check.</summary>
    [VerificationCommand("--collectible-block-frame-override")]
    internal static int CollectibleBlockFrameOverrideCommand()
    {
        Suite(nameof(VerifyCollectibleFrameOverride), VerifyCollectibleFrameOverride);
        return 0;
    }

    /// <summary>A retail runtime with <paramref name="sheets"/> as its block sheets, in the Morph Ball room, as the room block picture check loads it.</summary>
    private static SuperMetroidRuntime LoadMorphBall(InstalledRoomArtwork sheets)
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.Artwork.RoomBlockSheets = sheets;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MorphBall);
        runtime.Player.IntroducedSamus.Control.LockHandlers();
        return runtime;
    }

    /// <summary>The pictures the displayed BG1 shows.</summary>
    private static RoomBlockPictureTable PicturesOf(SuperMetroidRuntime runtime, string where) =>
        (runtime.Display.Latched.Rooms ?? throw new InvalidOperationException($"{where}: no room backgrounds are displayed.")).Bg1.Pictures
            ?? throw new InvalidOperationException($"{where}: BG1 shows no block pictures.");

    /// <summary>Whether <paramref name="block"/> is a graphics slot's block and the room shows the Morph Ball in that slot (#1466).</summary>
    private static bool ShowsCollectible(SuperMetroidRuntime runtime, int block) =>
        CollectibleSlotBlocks.SlotOf(block, CollectiblePlms.GraphicsSlots) is { } slot &&
        (runtime.Room.Scope ?? throw new InvalidOperationException("The Morph Ball room has no room scope.")).Appearance.CollectibleIn(slot) == ShownItem;

    /// <summary>The colour an override's repaint gives <paramref name="pixel"/>: each colour channel inverted.</summary>
    private static RgbaPixel Inverted(RgbaPixel pixel) => pixel with { R = (byte)~pixel.R, G = (byte)~pixel.G, B = (byte)~pixel.B };

    /// <summary>A picture BG1 showed for a graphics slot's block while the Morph Ball showed over the slot.</summary>
    /// <param name="Frame">The displayed frame it showed on.</param>
    /// <param name="Block">The block it pictures.</param>
    /// <param name="Picture">The stock picture.</param>
    private readonly record struct ShownSlotPicture(int Frame, int Block, SpriteSheetFrame Picture);
}
