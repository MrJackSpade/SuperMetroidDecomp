using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
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
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>
/// Checks Ceres Ridley's room draws his scene from pictures of its pre-coloured sheets (#1392): the Baby he carries, the
/// door overlay, his tail and his wings, over each upload his getaway makes.
/// </summary>
internal static class CeresRidleyPicturesVerification
{
    /// <summary>The camera the checked draws are made under.</summary>
    private static readonly Position16<RoomPixels> Camera = Position16<RoomPixels>.Zero;

    /// <summary>
    /// In Ceres Ridley's room every region his getaway's uploads make follows them, and, with them all made, the Baby
    /// holding a pose, the door overlay, and his tail and wings each paint exactly what their frames' parts paint over the
    /// live characters.
    /// </summary>
    internal static void VerifyCeresRidleyPictures()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.CeresRidley);
        // The room load's uploads reach the OBJ characters when its first frame drains them.
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        UploadedPictureChecks.AssertRegionsFollowUploads(runtime, PlacedUploads.Of(CeresEscapePageDefinitions.ObjectPages), "Ceres Ridley's getaway");

        RoomEnemySlot ridley = runtime.Enemies.SlotTable.Slots[0];
        RoomEnemySlot door = runtime.Enemies.SlotTable.Slots[1];
        runtime.Enemies.RidleyEncounter.CeresStatus = CeresEscapeStatus.RidleyNotEscaped;
        // A pose part-way through its duration is held for the draw.
        runtime.Enemies.RidleyEncounter.Ridley = new RidleyEnemyState
        {
            MovementAnimationEnabled = false, BabyCurrentPose = CeresBabyPose.Round, BabyPoseDuration = 2, BabyInstructionTimer = 1,
            BabyPosition = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0080)),
        };
        ImmutableArray<SpriteDraw> scene = Draw(runtime, (sprites, live) =>
            runtime.Enemies.CeresRidleyBehavior.BabyScene.DrawCeresRidleyImmediateBabyAndDoor(sprites, Camera));
        AssertTrue(scene.Any(draw => draw.Source.Kind == SpriteSourceKind.CeresBaby), "Ceres Ridley's scene draws the Baby");
        UploadedPictureChecks.AssertPicturesPaintOverLive(runtime, scene, $"Ceres Ridley's scene with {door.DefinitionId} following him");

        runtime.Enemies.RidleyEncounter.Ridley = new RidleyEnemyState
        {
            MovementAnimationEnabled = true,
            FacingDirection = RidleyFacing.Left,
            SpritemapPalette = 1,
            TailSegments = [.. Enumerable.Range(0, 7).Select(index => new RidleyTailSegment
            {
                Position = new Position16<RoomPixels>(unchecked((ushort)(80 + index * 10)), unchecked((ushort)(120 + index * 3))),
            })],
        };
        ridley.Position = ridley.Position.WithWhole(new(200, 100));
        ImmutableArray<SpriteDraw> supplements = Draw(runtime, (sprites, live) =>
            runtime.Enemies.CeresRidleyBehavior.BabyScene.DrawRidleySupplementalSprites(sprites, ridley, Camera));
        AssertTrue(supplements.Any(draw => draw.Source.Kind == SpriteSourceKind.RidleyWings), "Ceres Ridley draws his wings");
        UploadedPictureChecks.AssertPicturesPaintOverLive(runtime, supplements, "Ceres Ridley's tail and wings");
        Console.WriteLine($"Ceres Ridley pictures: {scene.Length} scene and {supplements.Length} tail and wing pictures paint their frames over the live characters.");
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
        RoomTilemapOverlay overlay = runtime.Room.LoadedScope.Bg1Overlay
            ?? throw new InvalidOperationException("Ceres Ridley's getaway wrote no warning over BG1.");
        RoomOverlayPictures shown = runtime.Display.Latched.Rooms?.Bg1Overlay
            ?? throw new InvalidOperationException("The displayed frame shows no warning over BG1.");
        ReadOnlySpan<byte> page = runtime.Artwork.GameplayObjectPage(runtime.EnemySession.SpriteCharacters).Characters;
        int written = 0;
        for (int index = 0; index < RoomTilemapOverlay.WordCount; index++)
        {
            if (overlay.Words[index] == RoomTilemapOverlay.Unwritten)
            {
                AssertTrue(shown.Pictures[index] is null, $"Nothing written at word {index} shows nothing over BG1");
                continue;
            }
            written++;
            var word = new SnesBgTilemapWord((ushort)overlay.Words[index]);
            BgTilePicture picture = shown.Pictures[index]
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
                    AssertTrue(sheet.Inks[pixel].PaletteIndex == expected && (colour == 0 || (sheet.Inks[pixel].Priority != 0) == word.HasPriority),
                        $"The warning's word {index}, ${word.Raw:X4}, paints index {sheet.Inks[pixel].PaletteIndex} priority {sheet.Inks[pixel].Priority} " +
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

    /// <summary>The sprites <paramref name="draw"/> publishes over the runtime's live characters.</summary>
    private static ImmutableArray<SpriteDraw> Draw(SuperMetroidRuntime runtime, Action<SpriteFrameBuffer, SpriteArtwork> draw)
    {
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        draw(sprites, runtime.Artwork.GameplayObjectPage(runtime.EnemySession.SpriteCharacters));
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
