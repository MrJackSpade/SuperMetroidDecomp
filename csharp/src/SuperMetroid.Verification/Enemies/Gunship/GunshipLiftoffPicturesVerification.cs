using SuperMetroid.AssetExtraction.Artwork;
using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Gunship;
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
/// Checks the gunship draws from finished pictures of its landing site's pre-coloured sheet (#1392, #1538): landed and once
/// its engines are readied for the takeoff the ship paints its frames over the characters then, and every frame the
/// takeoff's look changes shows its picture of each look it is drawn in.
/// </summary>
internal static class GunshipLiftoffPicturesVerification
{
    /// <summary>
    /// At the landing site the ship's top and bottom show, landed and in the takeoff look, the finished picture of the look
    /// shown, painting exactly what the frame paints over the characters the set leaves in that look; and every frame of the
    /// set's sheets the takeoff changes follows its looks.
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

        int pictured = 0;
        foreach (EncounterLook look in new[] { EncounterLook.Loaded(EncounterArtwork.Gunship), GunshipTakeoffDefinitions.TakeoffLook })
        {
            // The ship landed, then in its takeoff look.
            IReadOnlyList<EncounterLook> reached = look.Name == EncounterLook.LoadedName ? [] : [look];
            runtime.EnemySession.Looks.Show(look);
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
                IReadOnlyList<EncounterArtwork> changing = sheets.ChangingEncounters(set, part.DefinitionId, name);
                if (changing.Count > 0)
                    pictured++;
                var source = new SpriteSource(SpriteSourceKind.Enemy, part.SlotIndex);
                ImmutableArray<SpriteDraw> drawn = [.. sprites.Draws.Where(draw => draw.Source == source)];
                UploadedPictureChecks.AssertDrawnPicture(sheets.For(set, part.DefinitionId).Frame,
                    UploadedPictureChecks.ExpectedPicture(name, changing, reached), drawn,
                    $"in {look} the ship's {part.DefinitionId}");

                CharacterSpriteDraw parted = EnemyPartComposition.Spritemap(live, source, RuntimeArtworkContent.Of(runtime).EnemyTiles.Spritemaps
                    ?? throw new InvalidOperationException("The runtime loads the enemy compositions."), new EnemySpritemapDraw
                {
                    Frame = frame, Origin = new(drawn[0].Origin.X, drawn[0].Origin.Y), PaletteBits = NativeSpriteGraphicsWords.PaletteBits(part.PaletteRowIn(runtime.Enemies)), BaseTileIndex = part.NativeTileBaseIn(runtime.Enemies),
                }) ?? throw new InvalidOperationException($"The ship's {part.DefinitionId} frame has parts.");
                UploadedPictureChecks.AssertPaintsLikeParts(drawn, [parted], $"the ship's {part.DefinitionId} in {look}");
            }
        }
        int followed = UploadedPictureChecks.AssertFramesFollowLooks(set, EncounterArtwork.Gunship, "the gunship's takeoff");
        Console.WriteLine($"Gunship liftoff pictures: the ship paints its frames over the characters each of its looks leaves ({pictured} of its draws follow the looks), and {followed} frames follow the looks they are drawn in.");
    }

    /// <summary>Runs the gunship liftoff picture check.</summary>
    [VerificationCommand("--gunship-liftoff-pictures")]
    internal static int GunshipLiftoffPicturesCommand()
    {
        Suite(nameof(VerifyGunshipLiftoffPictures), VerifyGunshipLiftoffPictures);
        return 0;
    }
}
