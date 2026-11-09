using SuperMetroid.AssetExtraction.Artwork;
using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>
/// Checks Ceres Ridley's room draws his scene from pictures of its pre-coloured sheets (#1392): the Baby he carries, the
/// door overlay, his tail and his wings, which no escape page his getaway brings up changes, for no frame is drawn over
/// them (#1513).
/// </summary>
internal static class CeresRidleyPicturesVerification
{
    /// <summary>The camera the checked draws are made under.</summary>
    private static readonly Position16<RoomPixels> Camera = Position16<RoomPixels>.Zero;

    /// <summary>
    /// In Ceres Ridley's room no frame follows his getaway's escape warning look, and, with it brought up, the Baby holding a
    /// pose, the door overlay, and his tail and wings each paint exactly what their frames' parts paint over the characters
    /// the room's load leaves.
    /// </summary>
    internal static void VerifyCeresRidleyPictures()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.CeresRidley);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        RoomEnemyGraphicsSetDefinition set = runtime.Enemies.GraphicsSet.LoadedSet;
        AssertEqual(0, UploadedPictureChecks.AssertFramesFollowLooks(set, EncounterArtwork.CeresEscape, "Ceres Ridley's getaway", requireFrames: false),
            "no frame is drawn in Ceres Ridley's escape warning look");
        runtime.EnemySession.Looks.Show(CeresEscapeReadyingDefinitions.WarningLook);

        RoomEnemySlot ridley = runtime.Enemies.SlotTable.Slots[0];
        RoomEnemySlot door = runtime.Enemies.SlotTable.Slots[1];
        runtime.Enemies.RidleyEncounter.CeresStatus = CeresEscapeStatus.RidleyNotEscaped;
        // A pose part-way through its duration is held for the draw.
        runtime.Enemies.RidleyEncounter.Ridley = new RidleyEnemyState
        {
            MovementAnimationEnabled = false, BabyCurrentPose = CeresBabyPose.Round, BabyPoseDuration = 2, BabyFrameHold = 1,
            BabyPosition = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0080)),
        };
        ImmutableArray<SpriteDraw> scene = Draw(sprites =>
            runtime.Enemies.CeresRidleyBehavior.BabyScene.DrawCeresRidleyImmediateBabyAndDoor(sprites, Camera));
        AssertTrue(scene.Any(draw => draw.Source.Kind == SpriteSourceKind.CeresBaby), "Ceres Ridley's scene draws the Baby");
        UploadedPictureChecks.AssertPicturesPaintOverLooks(set, [], scene,
            $"Ceres Ridley's scene with {door.DefinitionId} following him");

        runtime.Enemies.RidleyEncounter.Ridley = new RidleyEnemyState
        {
            MovementAnimationEnabled = true,
            FacingDirection = RidleyFacing.Left,
            TailSegments = [.. Enumerable.Range(0, 7).Select(index => new RidleyTailSegment
            {
                Position = new Position16<RoomPixels>(unchecked((ushort)(80 + index * 10)), unchecked((ushort)(120 + index * 3))),
            })],
        };
        ridley.Position = ridley.Position.WithWhole(new(200, 100));
        ImmutableArray<SpriteDraw> supplements = Draw(sprites =>
            runtime.Enemies.CeresRidleyBehavior.BabyScene.DrawRidleySupplementalSprites(sprites, ridley, Camera));
        AssertTrue(supplements.Any(draw => draw.Source.Kind == SpriteSourceKind.RidleyWings), "Ceres Ridley draws his wings");
        UploadedPictureChecks.AssertPicturesPaintOverLooks(set, [], supplements, "Ceres Ridley's tail and wings");
        Console.WriteLine($"Ceres Ridley pictures: {scene.Length} scene and {supplements.Length} tail and wing pictures paint their frames over the room's own characters.");
    }

    /// <summary>
    /// Checks the warning Ceres Ridley's getaway writes over the arena's BG1 draws from pictures (#1392): run until its text
    /// is typed, every word written paints, pixel for pixel, the palette index and priority its character on the OBJ
    /// character table paints.
    /// </summary>
    internal static void VerifyCeresWarningPictures()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.CeresRidley);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var ridley = new RidleyEnemyState { Function = RidleyAiFunction.CeresActivateSelfDestruct };
        runtime.Enemies.RidleyEncounter.Ridley = ridley;
        // Each frame's uploads and words reach the display as the next accepted frame begins, which resolves their pictures.
        while (ridley.Function == RidleyAiFunction.CeresActivateSelfDestruct)
        {
            runtime.Enemies.CeresRidleyBehavior.Getaway.TickCeresRidleySelfDestruct(ridley);
            runtime.FrameBoundary.BeginAcceptedFrame(0);
        }
        RoomWrittenOverlay overlay = runtime.Room.LoadedScope.Enemies.RidleyEncounter.CeresWarning;
        AssertTrue(!overlay.Pictures.IsEmpty, "Ceres Ridley's getaway shows its warning over BG1");
        // The getaway writes its pages' and glyphs' pictures by name (#1466); the words they stand for are the import's.
        System.Collections.Immutable.ImmutableArray<int> words = overlay.WordsIn(InstalledFixtures.AreaArtwork.CeresEscapeOverlayTilemaps);
        RoomOverlayPictures shown = runtime.Display.Latched.Rooms?.ForegroundText
            ?? throw new InvalidOperationException("The displayed frame shows no warning over BG1.");
        BgTilePicture?[] shownCells = TwoScreenTilemapCells.Lay(shown.Pictures);
        ReadOnlySpan<byte> page = EnemySheetFixtures.LivePage(runtime.Enemies.GraphicsSet.LoadedSet, UploadedPictureChecks.Shown(runtime)).Characters;
        int written = 0;
        for (int index = 0; index < TwoScreenTilemapCells.Count; index++)
        {
            if (words[index] == RoomWrittenOverlay.Unwritten)
            {
                AssertTrue(shownCells[index] is null, $"Nothing written at word {index} shows nothing over BG1");
                continue;
            }
            written++;
            var word = new SnesBgTilemapWord((ushort)words[index]);
            BgTilePicture picture = shownCells[index]
                ?? throw new InvalidOperationException($"The warning's word {index}, ${word.Raw:X4}, shows no picture.");
            SpriteImage sheet = picture.Picture.Sheet;
            for (int y = 0; y < CharacterPixels; y++)
            {
                int address = word.CharacterIndex * CharacterBytes + (word.FlipVertically ? CharacterPixels - 1 - y : y) * 2;
                for (int x = 0; x < CharacterPixels; x++)
                {
                    int bit = 7 - (word.FlipHorizontally ? CharacterPixels - 1 - x : x);
                    int colour = (page[address] >> bit & 1) | (page[address + 1] >> bit & 1) << 1
                        | (page[address + 16] >> bit & 1) << 2 | (page[address + 17] >> bit & 1) << 3;
                    int pixel = (picture.Picture.Corner.Y + picture.Corner.Y + y) * sheet.Size.X + picture.Picture.Corner.X + picture.Corner.X + x;
                    int expected = colour == 0 ? 0 : word.PaletteIndex * 16 + colour;
                    int painted = InstalledPictureColoursConstants.ComposedIndexOf(sheet, sheet.Inks[pixel]);
                    AssertTrue(painted == expected && (colour == 0 || sheet.Inks[pixel].FrontTile == word.HasPriority),
                        $"The warning's word {index}, ${word.Raw:X4}, paints index {painted} front tile {sheet.Inks[pixel].FrontTile} " +
                        $"at pixel ({x}, {y}), where its character paints index {expected}");
                }
            }
        }
        AssertTrue(written > 0, "Ceres Ridley's getaway writes its warning over BG1");
        Console.WriteLine($"Ceres warning pictures: all {written} words the getaway writes over BG1 paint their characters on the OBJ character table.");
    }

    /// <summary>Pixels across and down a character.</summary>
    private const int CharacterPixels = 8;

    /// <summary>Bytes of one four-bit character.</summary>
    private const int CharacterBytes = 32;

    /// <summary>The sprites <paramref name="draw"/> publishes.</summary>
    private static ImmutableArray<SpriteDraw> Draw(Action<SpriteFrameBuffer> draw)
    {
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        draw(sprites);
        sprites.FinalizeFrame();
        return sprites.Draws;
    }

    /// <summary>Runs the Ceres Ridley picture check.</summary>
    [VerificationCommand("--ceres-ridley-pictures")]
    internal static int CeresRidleyPicturesCommand()
    {
        Suite(nameof(VerifyCeresRidleyPictures), VerifyCeresRidleyPictures);
        Suite(nameof(VerifyCeresWarningPictures), VerifyCeresWarningPictures);
        return 0;
    }
}
