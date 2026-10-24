using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>
/// Checks Crocomire's room draws from pictures of each skeleton upload his death makes (#1392): the picture the draw
/// selects for each region holds exactly the characters live there.
/// </summary>
internal static class CrocomireSkeletonPicturesVerification
{
    /// <summary>
    /// In Crocomire's room, before his death's first skeleton upload and after each, every region his uploads make shows
    /// the picture of the upload made last over it, whose characters are those live there.
    /// </summary>
    internal static void VerifyCrocomireSkeletonPictures()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        // The fixture starts in Ceres, whose room leaves its door tiles over standard characters; a session reaching
        // Crocomire has started on Zebes, whose game start shows the standard characters again.
        runtime.HudSetup.ShowStandardObjectCharacters();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Crocomire);
        // The room load's uploads reach the OBJ characters when its first frame drains them.
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        UploadedPictureChecks.AssertRegionsFollowUploads(runtime, PlacedUploads.Of(CrocomireSkeletonTransferDefinitions.Sources), "Crocomire's skeleton");
        Console.WriteLine($"Crocomire skeleton pictures: each region follows his {PlacedUploads.Of(CrocomireSkeletonTransferDefinitions.Sources).Count} skeleton uploads.");
    }

    /// <summary>Runs the Crocomire skeleton picture check.</summary>
    [VerificationCommand("--crocomire-skeleton-pictures")]
    internal static int CrocomireSkeletonPicturesCommand()
    {
        Suite(nameof(VerifyCrocomireSkeletonPictures), VerifyCrocomireSkeletonPictures);
        return 0;
    }
}
