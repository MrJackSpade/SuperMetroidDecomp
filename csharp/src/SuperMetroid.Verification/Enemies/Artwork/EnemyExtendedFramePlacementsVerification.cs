using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Artwork;

using SuperMetroid.Verification.Harness;

/// <summary>
/// Confirms the extended-frame placements gameplay draws by are the installed compositions' own (#1463): the draw list
/// places each component's sheet picture at its composition's offset and applies the composition's display binding.
/// </summary>
internal static class EnemyExtendedFramePlacementsVerification
{
    /// <summary>
    /// For every enemy frame, the installed placements resolve the same displayed frame, the same component count and the
    /// same component offsets as the installed compositions they were derived from, and the runtime catalog binds them.
    /// </summary>
    internal static void VerifyEnemyExtendedFramePlacements()
    {
        EnemyTileSourceArtwork stock = InstalledFixtures.EnemyTiles;
        EnemyExtendedFrameCatalog compositions = stock.ExtendedFrames
            ?? throw new InvalidOperationException("Verification requires the installed extended frames.");
        EnemyExtendedFramePlacementCatalog placements = stock.ExtendedFramePlacements
            ?? throw new InvalidOperationException("Verification requires the installed extended frame placements.");
        AssertTrue(ReferenceEquals(compositions.Placements, placements),
            "the runtime catalog binds the placements derived from the installed compositions");
        int installed = 0, components = 0;
        foreach (EnemyFrameId frame in Enum.GetValues<EnemyFrameId>())
        {
            bool composed = compositions.TryGetDisplay(frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> expected);
            bool placed = placements.TryGetDisplay(frame, out ReadOnlyMemory<EnemyExtendedComponentPlacement> actual);
            AssertEqual(composed, placed, $"{frame} has placements exactly when it has installed compositions");
            AssertEqual(compositions.GetDisplayFrame(frame), placements.GetDisplayFrame(frame),
                $"{frame} displays the frame its composition binding selects");
            if (!composed)
                continue;
            installed++;
            AssertEqual(expected.Length, actual.Length, $"{frame} places every displayed component");
            for (int index = 0; index < expected.Length; index++)
                AssertEqual(expected.Span[index].Offset, actual.Span[index].Offset, $"{frame} component {index} offset");
            components += expected.Length;
        }
        AssertTrue(installed > 0 && components > 0, "the installation holds extended frames with components");
        Console.WriteLine($"  Extended frame placements: {installed} frames, {components} components match their compositions.");
    }

    /// <summary>Runs the focused extended-frame placement check.</summary>
    [VerificationCommand("--enemy-extended-frame-placements")]
    internal static int EnemyExtendedFramePlacementsCommand()
    {
        Suite(nameof(VerifyEnemyExtendedFramePlacements), () => VerifyEnemyExtendedFramePlacements());
        return 0;
    }
}
