using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Artwork;

using SuperMetroid.Verification.Harness;

/// <summary>
/// Confirms the extended-frame displays gameplay draws by are the installed compositions' own (#1463, #1513): the draw
/// list draws a frame's one picture exactly when its composition has a sprite piece, and applies the composition's
/// display binding.
/// </summary>
internal static class EnemyExtendedFrameDisplaysVerification
{
    /// <summary>
    /// For every enemy frame, the installed displays resolve exactly the frames the installed compositions resolve,
    /// picture exactly those with a sprite piece, and select the same displayed frame.
    /// </summary>
    internal static void VerifyEnemyExtendedFrameDisplays()
    {
        EnemyTileSourceArtwork stock = InstalledFixtures.EnemyTiles;
        EnemyExtendedFrameCatalog compositions = stock.ExtendedFrames
            ?? throw new InvalidOperationException("Verification requires the installed extended frames.");
        EnemyExtendedFrameDisplayCatalog displays = compositions.Displays;
        int installed = 0, pictured = 0;
        foreach (EnemyFrameId frame in Enum.GetValues<EnemyFrameId>())
        {
            bool composed = compositions.TryGetDisplay(frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> pieces);
            bool displayed = displays.TryGetDisplay(frame, out bool hasPicture);
            AssertEqual(composed, displayed, $"{frame} is displayed exactly when it has installed compositions");
            AssertEqual(compositions.Displays.GetDisplayFrame(frame), displays.GetDisplayFrame(frame),
                $"{frame} displays the frame its composition binding selects");
            if (!composed)
                continue;
            installed++;
            AssertEqual(!pieces.IsEmpty, hasPicture, $"{frame} has a picture exactly when its composition has a sprite piece");
            if (hasPicture)
                pictured++;
        }
        AssertTrue(installed > 0 && pictured > 0, "the installation holds pictured extended frames");
        Console.WriteLine($"  Extended frame displays: {installed} frames, {pictured} pictured, match their compositions.");
    }

    /// <summary>Runs the focused extended-frame display check.</summary>
    [VerificationCommand("--enemy-extended-frame-displays")]
    internal static int EnemyExtendedFrameDisplaysCommand()
    {
        Suite(nameof(VerifyEnemyExtendedFrameDisplays), () => VerifyEnemyExtendedFrameDisplays());
        return 0;
    }
}
