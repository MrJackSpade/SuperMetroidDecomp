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
    /// <summary>Bomb Torizo's and Golden Torizo's rooms, each a suite of its own.</summary>
    internal static FullRunSuitesVerification.RegisteredSuite[] TorizoTransferPicturesSuites() =>
    [
        new($"{nameof(VerifyTorizoTransferPictures)}BombTorizo", () => VerifyTorizoTransferPictures(RoomHeaderDefinitions.BombTorizo, EncounterArtwork.BombTorizo)),
        new($"{nameof(VerifyTorizoTransferPictures)}GoldenTorizo", () => VerifyTorizoTransferPictures(RoomHeaderDefinitions.GoldenTorizo, EncounterArtwork.GoldenTorizo)),
    ];

    /// <summary>
    /// In <paramref name="room"/>, every frame of the room's sheets <paramref name="torizo"/>'s looks change shows, in each
    /// look it is drawn in, its finished picture of that look.
    /// </summary>
    private static void VerifyTorizoTransferPictures(RoomHeaderDefinition room, EncounterArtwork torizo)
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(room);
        int frames = UploadedPictureChecks.AssertFramesFollowLooks(runtime.Enemies.GraphicsSet.LoadedSet, torizo, $"{room.Name}'s swaps");
        Console.WriteLine($"Torizo transfer pictures: {frames} frames of {room.Name} follow every look of its swaps.");
    }

    /// <summary>Runs the Torizo transfer picture checks, one suite per room.</summary>
    [VerificationCommand("--torizo-transfer-pictures")]
    internal static int TorizoTransferPicturesCommand()
    {
        foreach (FullRunSuitesVerification.RegisteredSuite suite in TorizoTransferPicturesSuites())
            Suite(suite.Name, suite.Body);
        return 0;
    }
}
