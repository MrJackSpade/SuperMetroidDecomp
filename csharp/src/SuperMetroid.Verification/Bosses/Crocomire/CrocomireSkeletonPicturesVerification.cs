using SuperMetroid.Core.Assets;
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
/// Checks Crocomire's room draws from pictures of each step of his skeleton's artwork sequence (#1392, #1513): each frame
/// the sequence changes shows, at each chunk his death bares, a picture painting exactly what the frame paints over the
/// characters his room's load and every chunk up to that one leave.
/// </summary>
internal static class CrocomireSkeletonPicturesVerification
{
    /// <summary>In each of Crocomire's looks, every frame of his room's sheets his looks change shows its finished picture of that look.</summary>
    internal static void VerifyCrocomireSkeletonPictures()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Crocomire);
        int frames = UploadedPictureChecks.AssertFramesFollowLooks(runtime.Enemies.GraphicsSet.LoadedSet, EncounterArtwork.Crocomire, "Crocomire's skeleton");
        Console.WriteLine($"Crocomire skeleton pictures: {frames} frames follow his looks, alive and bared to the skeleton.");
    }

    /// <summary>Runs the Crocomire skeleton picture check.</summary>
    [VerificationCommand("--crocomire-skeleton-pictures")]
    internal static int CrocomireSkeletonPicturesCommand()
    {
        Suite(nameof(VerifyCrocomireSkeletonPictures), VerifyCrocomireSkeletonPictures);
        return 0;
    }
}
