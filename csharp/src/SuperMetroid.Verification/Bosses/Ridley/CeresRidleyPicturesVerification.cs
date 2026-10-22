using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.World.Artwork;
using SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationAssert;

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
        UploadedPictureChecks.AssertRegionsFollowUploads(runtime, CeresEscapeCharacterUploads.All, "Ceres Ridley's getaway");

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
            runtime.Enemies.CeresRidleyBehavior.BabyScene.DrawCeresRidleyImmediateBabyAndDoor(sprites, live, Camera));
        AssertTrue(scene.Any(draw => draw.Source.Kind == SpriteSourceKind.CeresBaby), "Ceres Ridley's scene draws the Baby");
        UploadedPictureChecks.AssertPicturesPaintOverLive(runtime, scene, $"Ceres Ridley's scene with {door.DefinitionId} following him");

        runtime.Enemies.RidleyEncounter.Ridley = new RidleyEnemyState
        {
            MovementAnimationEnabled = true,
            FacingDirection = RidleyFacing.Left,
            SpritemapPaletteIndex = SnesObjPalettes.Palette(1),
            TailSegments = [.. Enumerable.Range(0, 7).Select(index => new RidleyTailSegment
            {
                Position = new Position16<RoomPixels>(unchecked((ushort)(80 + index * 10)), unchecked((ushort)(120 + index * 3))),
            })],
        };
        ridley.Position = ridley.Position.WithWhole(new(200, 100));
        ImmutableArray<SpriteDraw> supplements = Draw(runtime, (sprites, live) =>
            runtime.Enemies.CeresRidleyBehavior.BabyScene.DrawRidleySupplementalSprites(sprites, live, ridley, Camera));
        AssertTrue(supplements.Any(draw => draw.Source.Kind == SpriteSourceKind.RidleyWings), "Ceres Ridley draws his wings");
        UploadedPictureChecks.AssertPicturesPaintOverLive(runtime, supplements, "Ceres Ridley's tail and wings");
        Console.WriteLine($"Ceres Ridley pictures: {scene.Length} scene and {supplements.Length} tail and wing pictures paint their frames over the live characters.");
    }

    /// <summary>The sprites <paramref name="draw"/> publishes over the runtime's live characters.</summary>
    private static ImmutableArray<SpriteDraw> Draw(SuperMetroidRuntime runtime, Action<SpriteFrameBuffer, SpriteArtwork> draw)
    {
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        draw(sprites, runtime.Artwork.GameplayObjectPage(runtime.EnemySession.ObjCharacters));
        sprites.FinalizeFrame();
        return sprites.Draws;
    }

    /// <summary>Runs the Ceres Ridley picture check.</summary>
    [VerificationCommand("--ceres-ridley-pictures")]
    internal static int CeresRidleyPicturesCommand()
    {
        Suite(nameof(VerifyCeresRidleyPictures), VerifyCeresRidleyPictures);
        return 0;
    }
}
