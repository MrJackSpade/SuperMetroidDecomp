using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Confirms the installed enemy sheets (#1392): extraction writes a sheet for every graphics-set member whose behaviour
/// draws, loading names each set's sheet for each of its enemies, and every frame of every installed sheet shows
/// exactly the inks its composition shows over the set's characters.
/// </summary>
internal static class EnemySheetFilesVerification
{
    /// <summary>
    /// How many suites the drawn graphics sets are dealt among, each taking every one of that many sets in turn, so the
    /// neighbouring sets that share the frame-heavy enemies land in different suites.
    /// </summary>
    private const int SetRuns = 8;

    /// <summary>The drawn graphics sets dealt among <see cref="SetRuns"/> suites.</summary>
    internal static FullRunSuitesVerification.RegisteredSuite[] EnemySheetFilesSuites() =>
    [
        .. Enumerable.Range(0, SetRuns).Select(run =>
            new FullRunSuitesVerification.RegisteredSuite($"{nameof(VerifyEnemySheetFiles)}{run + 1}Of{SetRuns}", () => VerifyEnemySheetFiles(run))),
    ];

    /// <summary>
    /// Compares each installed enemy sheet frame of the sets dealt to suite <paramref name="run"/>
    /// of <see cref="SetRuns"/> with its composition.
    /// </summary>
    private static void VerifyEnemySheetFiles(int run)
    {
        // The suites share one load of the installation's sheets and of the artwork they were composed from.
        InstalledEnemySheets installed = InstalledFixtures.EnemySheets;
        EnemySheetInputs inputs = EnemySheetFixtures.Inputs;
        IReadOnlyList<RoomEnemyGraphicsSetDefinition> drawn = RoomEnemyGraphicsSetLists.DrawnUnder;
        int sets = 0, members = 0, frames = 0;
        foreach (RoomEnemyGraphicsSetDefinition set in drawn.Where((_, index) => index % SetRuns == run))
        {
            sets++;
            foreach ((EnemyDefinitionId enemy, SpriteSheetSources sources, _, _) in EnemySheetComposer.Sources(inputs, set).Enemies)
            {
                members++;
                SpriteSheet sheet = installed.For(set.Name, enemy);
                foreach (SheetFrameSource source in sources.Frames)
                {
                    frames++;
                    AssertSamePixels(sheet.Frame(source.Name), source, sources, $"{set.Name} {enemy} {source.Name}");
                }
            }
        }
        Console.WriteLine($"Enemy sheets, suite {run + 1} of {SetRuns}: {sets} of {drawn.Count} graphics sets, {members} members, {frames} frames ink-exact against their compositions.");
    }

    /// <summary>The sheet frame shows exactly the inks its composition shows over the set's characters.</summary>
    private static void AssertSamePixels(SpriteSheetFrame frame, SheetFrameSource source, SpriteSheetSources sources, string label)
    {
        var origin = new Position16<ScreenPixels>(128, 112);
        var image = new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.Enemy), frame, origin, SpriteMirror.None);
        // The cartridge ORs the enemy's palette bits into each part's own row.
        var ored = new SpriteFrame(source.Frame.Name, [.. source.Frame.Parts.Select(part => part with { Palette = (part.Palette ?? 0) | sources.PaletteBits })]);
        var character = new CharacterSpriteDraw(new SpriteSource(SpriteSourceKind.Enemy), ored, source.Characters, origin,
            TileFlip.None, 0, 0);
        UploadedPictureChecks.AssertSameInks([image], [character], label, requirePixels: false);
    }

    /// <summary>Runs the enemy sheet extraction and ink comparison, one suite per run of sets.</summary>
    [VerificationCommand("--enemy-sheet-files")]
    internal static int EnemySheetFilesCommand()
    {
        foreach (FullRunSuitesVerification.RegisteredSuite suite in EnemySheetFilesSuites())
            Suite(suite.Name, suite.Body);
        return 0;
    }
}
