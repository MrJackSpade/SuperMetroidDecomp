using SuperMetroid.AssetExtraction.Artwork;
using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Gunship;

/// <summary>
/// Checks the gunship draws from pictures of its landing site's pre-coloured sheet (#1392, #1513): its own characters before its
/// takeoff, then its picture of each takeoff page as the takeoff reaches it.
/// </summary>
internal static class GunshipLiftoffPicturesVerification
{
    /// <summary>
    /// At the landing site the ship's top shows, before its takeoff and at each takeoff page, its picture of the page reached,
    /// and each picture paints exactly what the frame paints over the characters the set leaves at that page.
    /// </summary>
    internal static void VerifyGunshipLiftoffPictures()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.LandingSite);
        // The room load's uploads reach the OBJ characters when its first frame drains them.
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        RoomEnemySlot top = runtime.Enemies.SlotTable.Slots.First(slot => slot.DefinitionId == EnemyDefinitionId.ShipTop);
        // The bottom draws at the top's character index, which it reads from the slot before it.
        RoomEnemySlot bottom = runtime.Enemies.SlotTable.Slots[top.SlotIndex + 1];
        AssertEqual(EnemyDefinitionId.ShipBottomEntrance, bottom.DefinitionId, "the ship's bottom follows its top");
        var camera = new Position16<RoomPixels>(unchecked((ushort)(top.Position.X.Whole - 128)), unchecked((ushort)(top.Position.Y.Whole - 112)));
        IEnemySheetSource sheets = runtime.EnemySession.Sheets ?? throw new InvalidOperationException("The runtime loads the enemy sheets.");
        RoomEnemyGraphicsSetDefinition set = runtime.Enemies.GraphicsSet.LoadedSet;

        int pages = GunshipLiftoffTransferDefinitions.Frames.Length;
        int pictured = 0;
        for (int step = 0; step <= pages; step++)
        {
            // Step zero is the ship before its takeoff, then each takeoff page in turn.
            IReadOnlyList<EncounterArtworkStep> reached =
                step == 0 ? [] : [new EncounterArtworkStep(EncounterArtworkSequence.GunshipLiftoff, step - 1)];
            if (step > 0)
                runtime.EnemySession.ArtworkProgress.Show(EncounterArtworkSequence.GunshipLiftoff, step - 1);
            runtime.Enemies.StepFrame(new RoomEnemyFrame { Camera = camera, TimeIsFrozen = false, Samus = runtime.Player.Samus, Level = runtime.Room.LevelData });
            SpriteArtwork live = EnemySheetFixtures.LivePage(set, reached);
            var sprites = new SpriteFrameBuffer();
            sprites.BeginFrame();
            runtime.Enemies.Drawing.AdvanceLayers(camera, 0, 7);
            runtime.Enemies.Drawing.DrawLayers(sprites, camera, 0, 7);
            sprites.FinalizeFrame();
            foreach (RoomEnemySlot part in new[] { top, bottom })
            {
                EnemyFrameId frame = part.Frame ?? throw new InvalidOperationException($"The ship's {part.DefinitionId} shows a frame.");
                string name = EnemySheetDefinitions.FrameName(frame);
                IReadOnlyList<FrameArtworkVariation> variations = sheets.Variations(set, part.DefinitionId, name);
                if (variations.Count > 0)
                    pictured++;
                var source = new SpriteSource(SpriteSourceKind.Enemy, part.SlotIndex);
                ImmutableArray<SpriteDraw> drawn = [.. sprites.Draws.Where(draw => draw.Source == source)];
                UploadedPictureChecks.AssertDrawnPicture(sheets.For(set, part.DefinitionId),
                    UploadedPictureChecks.ExpectedPicture(name, variations, reached), drawn,
                    $"after {step} liftoff pages the ship's {part.DefinitionId}");

                CharacterSpriteDraw parted = EnemyPartComposition.Spritemap(live, source, RuntimeArtworkContent.Of(runtime).EnemyTiles.Spritemaps
                    ?? throw new InvalidOperationException("The runtime loads the enemy compositions."), new EnemySpritemapDraw
                {
                    Frame = frame, Origin = new(drawn[0].Origin.X, drawn[0].Origin.Y), PaletteBits = NativeSpriteGraphicsWords.PaletteBits(part.PaletteRowIn(runtime.Enemies)), BaseTileIndex = part.NativeTileBaseIn(runtime.Enemies),
                }) ?? throw new InvalidOperationException($"The ship's {part.DefinitionId} frame has parts.");
                UploadedPictureChecks.AssertPaintsLikeParts(drawn, [parted], $"the ship's {part.DefinitionId} after {step} liftoff uploads");
            }
        }
        AssertTrue(pictured > 0, "the ship is drawn over characters its takeoff uploads");
        Console.WriteLine($"Gunship liftoff pictures: the ship's top shows its picture of each of the {pages} takeoff pages, each painting its frame over the characters that page leaves.");
    }

    /// <summary>Runs the gunship liftoff picture check.</summary>
    [VerificationCommand("--gunship-liftoff-pictures")]
    internal static int GunshipLiftoffPicturesCommand()
    {
        Suite(nameof(VerifyGunshipLiftoffPictures), VerifyGunshipLiftoffPictures);
        return 0;
    }
}
