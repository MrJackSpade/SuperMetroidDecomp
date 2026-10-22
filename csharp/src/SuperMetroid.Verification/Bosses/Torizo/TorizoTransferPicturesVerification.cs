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
/// Checks the Torizos' rooms draw from pictures of each transfer their instruction lists make (#1392): the picture the
/// draw selects for each region holds exactly the characters live there.
/// </summary>
internal static class TorizoTransferPicturesVerification
{
    /// <summary>
    /// In Bomb Torizo's and Golden Torizo's rooms, before the first instruction-time transfer and after each, every region
    /// the transfers make shows the picture of the transfer made last over it, whose characters are those live there.
    /// </summary>
    internal static void VerifyTorizoTransferPictures()
    {
        foreach (RoomHeaderDefinition room in new[] { RoomHeaderDefinitions.BombTorizo, RoomHeaderDefinitions.GoldenTorizo })
        {
            SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            // The fixture starts in Ceres, whose room leaves its door tiles over standard characters; a session reaching
            // a Torizo has started on Zebes, whose game start uploads the standard characters again.
            runtime.HudSetup.QueueStandardObjectTiles();
            runtime.LoadCartridgeRoomForDebug(room);
            // The room load's uploads reach the OBJ characters when its first frame drains them.
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            UploadedPictureChecks.AssertRegionsFollowUploads(runtime, TorizoInstructionVramTransferDefinitions.Uploads, $"{room.Name}'s transfers");
        }
        Console.WriteLine($"Torizo transfer pictures: each region of both Torizos' rooms follows their {TorizoInstructionVramTransferDefinitions.Uploads.Count} instruction-time transfers.");
    }

    /// <summary>Runs the Torizo transfer picture check.</summary>
    [VerificationCommand("--torizo-transfer-pictures")]
    internal static int TorizoTransferPicturesCommand()
    {
        Suite(nameof(VerifyTorizoTransferPictures), VerifyTorizoTransferPictures);
        return 0;
    }
}
