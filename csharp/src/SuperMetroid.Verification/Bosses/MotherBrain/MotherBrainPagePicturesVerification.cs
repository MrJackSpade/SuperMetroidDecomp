using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// Checks Mother Brain's room draws from pictures of the steps of her page sequence at which each frame is drawn (#1513):
/// each frame her pages change shows, at each page it is drawn over, a picture painting exactly what the frame paints over
/// the characters her room's load and every page up to that one leave, and has no picture of a page it is never drawn
/// over, at which drawing it fails loudly.
/// </summary>
internal static class MotherBrainPagePicturesVerification
{
    /// <summary>At each of Mother Brain's pages, every frame of her room's sheets drawn then shows its picture of that page.</summary>
    internal static void VerifyMotherBrainPagePictures()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.MotherBrain);
        int pages = MotherBrainSpritePageDefinitions.Sequence.Count;
        int frames = UploadedPictureChecks.AssertFramesFollowSequence(runtime.Enemies.GraphicsSet.LoadedSet, EncounterArtworkSequence.MotherBrainPages,
            pages, "Mother Brain's pages");
        Console.WriteLine($"Mother Brain page pictures: {frames} frames follow the {pages} pages they are drawn over.");
    }

    /// <summary>Runs the Mother Brain page picture check.</summary>
    [VerificationCommand("--mother-brain-page-pictures")]
    internal static int MotherBrainPagePicturesCommand()
    {
        Suite(nameof(VerifyMotherBrainPagePictures), VerifyMotherBrainPagePictures);
        return 0;
    }
}
