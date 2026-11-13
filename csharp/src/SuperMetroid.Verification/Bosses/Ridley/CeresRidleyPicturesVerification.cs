using SuperMetroid.AssetExtraction.Artwork;
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
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>
/// Checks Ceres Ridley's room draws his scene from pictures of its pre-coloured sheets (#1392): the Baby he carries, the
/// door overlay, his tail and his wings, which no escape page his getaway brings up changes, for no frame is drawn over
/// them (#1513).
/// </summary>
internal static class CeresRidleyPicturesVerification
{
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
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.CeresRidley);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var ridley = new RidleyEnemyState { Function = RidleyAiFunction.CeresActivateSelfDestruct };
        runtime.Enemies.RidleyEncounter.Ridley = ridley;
        // Each frame's uploads and words reach the display as the next accepted frame begins, which resolves their pictures.
        while (ridley.Function == RidleyAiFunction.CeresActivateSelfDestruct)
        {
            runtime.Enemies.CeresRidleyBehavior.Getaway.TickCeresRidleySelfDestruct(ridley);
            runtime.FrameBoundary.BeginAcceptedFrame(0);
        }
        RoomWrittenOverlay overlay = runtime.Room.LoadedScope.Enemies.RidleyEncounter.CeresWarning;
        AssertTrue(!overlay.Pictures.IsEmpty, "Ceres Ridley's getaway shows its warning over BG1");
        // The getaway writes its pages' and glyphs' pictures by name (#1466); the words they stand for are the import's.
        System.Collections.Immutable.ImmutableArray<int> words = overlay.WordsIn(InstalledFixtures.AreaArtwork.CeresEscapeOverlayTilemaps);
        RoomOverlayPictures shown = runtime.Display.Latched.Rooms?.ForegroundText
            ?? throw new InvalidOperationException("The displayed frame shows no warning over BG1.");
        BgTilePicture?[] shownCells = TwoScreenTilemapCells.Lay(shown.Pictures);
        ReadOnlySpan<byte> page = EnemySheetFixtures.LivePage(runtime.Enemies.GraphicsSet, UploadedPictureChecks.Shown(runtime)).Characters;
        int written = 0;
        for (int index = 0; index < TwoScreenTilemapCells.Count; index++)
        {
            if (words[index] == RoomWrittenOverlay.Unwritten)
            {
                AssertTrue(shownCells[index] is null, $"Nothing written at word {index} shows nothing over BG1");
                continue;
            }
            written++;
            var word = new SnesBgTilemapWord((ushort)words[index]);
            BgTilePicture picture = shownCells[index]
                ?? throw new InvalidOperationException($"The warning's word {index}, ${word.Raw:X4}, shows no picture.");
            // Each pixel lies in the part of the warning's picture its word's priority puts it in (#1634), the other part
            // showing nothing there.
            for (int y = 0; y < CharacterPixels; y++)
            for (int x = 0; x < CharacterPixels; x++)
            {
                SuperMetroid.Verification.Rooms.Artwork.RoomBlockPicturesVerification.AssertPaintsTile(
                    $"The warning's word {index} at pixel ({x}, {y})", page, word, x, y, picture.Picture,
                    new Position16<ImagePixels>(picture.Corner.X + x, picture.Corner.Y + y));
            }
        }
        AssertTrue(written > 0, "Ceres Ridley's getaway writes its warning over BG1");
        Console.WriteLine($"Ceres warning pictures: all {written} words the getaway writes over BG1 paint their characters on the OBJ character table.");
    }

    /// <summary>Pixels across and down a character.</summary>
    private const int CharacterPixels = 8;

    /// <summary>Runs the Ceres Ridley picture check.</summary>
    [VerificationCommand("--ceres-ridley-pictures")]
    internal static int CeresRidleyPicturesCommand()
    {
        Suite(nameof(VerifyCeresWarningPictures), VerifyCeresWarningPictures);
        return 0;
    }
}
