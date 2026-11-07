using SuperMetroid.Core.Assets;
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
    /// In Bomb Torizo's and Golden Torizo's rooms, every frame of the room's sheets each Torizo's looks change shows, in each
    /// look it is drawn in, its finished picture of that look.
    /// </summary>
    internal static void VerifyTorizoTransferPictures()
    {
        int frames = 0;
        foreach ((RoomHeaderDefinition room, EncounterArtwork torizo) in new[]
                 {
                     (RoomHeaderDefinitions.BombTorizo, EncounterArtwork.BombTorizo),
                     (RoomHeaderDefinitions.GoldenTorizo, EncounterArtwork.GoldenTorizo),
                 })
        {
            SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(room);
            frames += UploadedPictureChecks.AssertFramesFollowLooks(runtime.Enemies.GraphicsSet.LoadedSet, torizo, $"{room.Name}'s swaps");
        }
        Console.WriteLine($"Torizo transfer pictures: {frames} frames of both Torizos' rooms follow every look of their swaps.");
    }

    /// <summary>Runs the Torizo transfer picture check.</summary>
    [VerificationCommand("--torizo-transfer-pictures")]
    internal static int TorizoTransferPicturesCommand()
    {
        Suite(nameof(VerifyTorizoTransferPictures), VerifyTorizoTransferPictures);
        return 0;
    }
}
