using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation;
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
    /// <summary>Loads the shared installation's enemy sheets and compares each frame with its composition.</summary>
    internal static void VerifyEnemySheetFiles()
    {
        string content = RepositoryInstallation.Installation.ContentDirectory;
        InstalledEnemySheets installed = EnemySheetFiles.Load(RepositoryInstallation.Installation.EnemySheetDirectory);

        EnemySheetInputs inputs = EnemySheetInputs.Load(content);
        int members = 0, frames = 0;
        foreach (RoomEnemyGraphicsSetDefinition set in RoomEnemyGraphicsSetLists.DrawnUnder)
        {
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
        Console.WriteLine($"Enemy sheets: {members} graphics-set members, {frames} frames ink-exact against their compositions.");
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

    /// <summary>Runs the enemy sheet extraction and ink comparison.</summary>
    [VerificationCommand("--enemy-sheet-files")]
    internal static int EnemySheetFilesCommand()
    {
        Suite(nameof(VerifyEnemySheetFiles), VerifyEnemySheetFiles);
        return 0;
    }
}
