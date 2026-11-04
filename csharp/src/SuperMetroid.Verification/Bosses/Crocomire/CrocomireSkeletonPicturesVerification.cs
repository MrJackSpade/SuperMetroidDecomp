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
    /// <summary>At each of the skeleton's chunks, every frame of Crocomire's room's sheets the chunks change shows its picture of that chunk.</summary>
    internal static void VerifyCrocomireSkeletonPictures()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Crocomire);
        int frames = UploadedPictureChecks.AssertFramesFollowSequence(runtime.Enemies.GraphicsSet.LoadedSet, EncounterArtworkSequence.CrocomireSkeleton,
            CrocomireSkeletonTransferDefinitions.FrameCount, "Crocomire's skeleton");
        Console.WriteLine($"Crocomire skeleton pictures: {frames} frames follow his {CrocomireSkeletonTransferDefinitions.FrameCount} skeleton chunks.");
    }

    /// <summary>Runs the Crocomire skeleton picture check.</summary>
    [VerificationCommand("--crocomire-skeleton-pictures")]
    internal static int CrocomireSkeletonPicturesCommand()
    {
        Suite(nameof(VerifyCrocomireSkeletonPictures), VerifyCrocomireSkeletonPictures);
        return 0;
    }
}
