using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationAssert;

namespace SuperMetroid.Verification.Enemies.Gunship;

/// <summary>
/// Checks the gunship draws from pictures of its landing site's pre-coloured sheet (#1392): its own characters before its
/// takeoff, then each liftoff upload's as the takeoff makes them.
/// </summary>
internal static class GunshipLiftoffPicturesVerification
{
    /// <summary>
    /// At the landing site the ship's top shows, before its takeoff and after each liftoff upload, the picture for what each
    /// region its frame is drawn over then holds, and each picture paints exactly what the frame painted over the room's
    /// live OBJ characters.
    /// </summary>
    internal static void VerifyGunshipLiftoffPictures()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        // The fixture starts in Ceres, whose room leaves its door tiles over standard characters; a session reaching the
        // landing site has started on Zebes, whose game start uploads the standard characters again.
        runtime.HudSetup.QueueStandardObjectTiles();
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

        var made = new List<RuntimeCharacterUpload>();
        int pictured = 0;
        for (int step = 0; step <= GunshipLiftoffTransferDefinitions.Uploads.Count; step++)
        {
            if (step > 0)
            {
                RuntimeCharacterUpload upload = GunshipLiftoffTransferDefinitions.Uploads[step - 1];
                runtime.EnemySession.ObjCharacters.Record(upload);
                made.Add(upload);
            }
            runtime.Enemies.StepFrame(new RoomEnemyFrame { Camera = camera, TimeIsFrozen = false, Samus = runtime.Player.Samus, Level = runtime.Room.LevelData });
            SpriteArtwork live = runtime.Artwork.GameplayObjectPage(runtime.EnemySession.ObjCharacters);
            var sprites = new SpriteFrameBuffer();
            sprites.BeginFrame();
            runtime.Enemies.Drawing.AdvanceLayers(camera, 0, 7);
            runtime.Enemies.Drawing.DrawLayers(sprites, camera, 0, 7);
            sprites.FinalizeFrame();
            foreach (RoomEnemySlot part in new[] { top, bottom })
            {
                EnemyFrameId frame = part.Frame ?? throw new InvalidOperationException($"The ship's {part.DefinitionId} shows a frame.");
                string name = EnemySheetDefinitions.FrameName(frame);
                IReadOnlyList<CharacterRegion[]> layers = sheets.Layers(set, part.DefinitionId, name);
                if (layers.Count > 0)
                    pictured++;
                var source = new SpriteSource(SpriteSourceKind.Enemy, part.SlotIndex);
                ImmutableArray<SpriteDraw> drawn = [.. sprites.Draws.Where(draw => draw.Source == source)];
                UploadedPictureChecks.AssertDrawnPictures(sheets.For(set, part.DefinitionId),
                    UploadedPictureChecks.ExpectedPictures(sheets, name, layers, made), drawn,
                    $"after {step} liftoff uploads the ship's {part.DefinitionId}");

                CharacterSpriteDraw parted = EnemyPartComposition.Spritemap(live, source, runtime.EnemySession.TileArtwork?.Spritemaps
                    ?? throw new InvalidOperationException("The runtime loads the enemy compositions."), new EnemySpritemapDraw
                {
                    Frame = frame, Origin = new(drawn[0].Origin.X, drawn[0].Origin.Y), PaletteBits = part.PaletteIndex, BaseTileIndex = part.VramTilesIndex,
                }) ?? throw new InvalidOperationException($"The ship's {part.DefinitionId} frame has parts.");
                UploadedPictureChecks.AssertPaintsLikeParts(runtime, drawn, [parted], $"the ship's {part.DefinitionId} after {step} liftoff uploads");
            }
        }
        AssertTrue(pictured > 0, "the ship is drawn over characters its takeoff uploads");
        Console.WriteLine($"Gunship liftoff pictures: the ship's top shows the picture for each of the {GunshipLiftoffTransferDefinitions.Uploads.Count} liftoff uploads, each painting its frame over the live characters.");
    }

    /// <summary>Runs the gunship liftoff picture check.</summary>
    [VerificationCommand("--gunship-liftoff-pictures")]
    internal static int GunshipLiftoffPicturesCommand()
    {
        Suite(nameof(VerifyGunshipLiftoffPictures), VerifyGunshipLiftoffPictures);
        return 0;
    }
}
