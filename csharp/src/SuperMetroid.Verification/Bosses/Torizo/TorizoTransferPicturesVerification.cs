using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// Checks the Torizos' rooms draw from pictures of each step of their artwork sequences (#1392, #1513): each frame a
/// Torizo's swaps change shows, at each swap its programs make, a picture painting exactly what the frame paints over the
/// characters its room's load and every swap up to that one leave.
/// </summary>
internal static class TorizoTransferPicturesVerification
{
    /// <summary>
    /// In Bomb Torizo's and Golden Torizo's rooms, every frame of the room's sheets each Torizo's sequence changes shows, at
    /// each of its steps, its picture of that step.
    /// </summary>
    internal static void VerifyTorizoTransferPictures()
    {
        int frames = 0;
        foreach ((RoomHeaderDefinition room, EncounterArtworkSequence sequence, int steps) in new[]
                 {
                     (RoomHeaderDefinitions.BombTorizo, EncounterArtworkSequence.BombTorizo, TorizoArtworkSwapDefinitions.BombSequence.Count),
                     (RoomHeaderDefinitions.GoldenTorizo, EncounterArtworkSequence.GoldenTorizo, TorizoArtworkSwapDefinitions.GoldenSequence.Count),
                 })
        {
            SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(room);
            frames += UploadedPictureChecks.AssertFramesFollowSequence(runtime.Enemies.GraphicsSet.LoadedSet, sequence, steps, $"{room.Name}'s swaps");
        }
        Console.WriteLine($"Torizo transfer pictures: {frames} frames of both Torizos' rooms follow every step of their artwork sequences.");
    }

    /// <summary>Runs the Torizo transfer picture check.</summary>
    [VerificationCommand("--torizo-transfer-pictures")]
    internal static int TorizoTransferPicturesCommand()
    {
        Suite(nameof(VerifyTorizoTransferPictures), VerifyTorizoTransferPictures);
        return 0;
    }
}
