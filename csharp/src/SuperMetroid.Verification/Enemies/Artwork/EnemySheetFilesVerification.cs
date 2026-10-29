using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.World.Artwork;
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
    /// <summary>Extracts the enemy sheets into a scratch directory, loads them and compares each frame with its composition.</summary>
    internal static void VerifyEnemySheetFiles()
    {
        DirectoryInfo scratch = Directory.CreateTempSubdirectory("sm-enemy-sheets-");
        try
        {
            string content = RepositoryInstallation.Installation.ContentDirectory;
            // The sheets' frames are named from the enemy compositions installed beside them.
            string tiles = Path.Combine(content, GameInstallationLayout.EnemyTileDirectoryName);
            Directory.CreateDirectory(Path.Combine(scratch.FullName, GameInstallationLayout.EnemyTileDirectoryName));
            foreach (string file in Directory.GetFiles(tiles))
                File.Copy(file, Path.Combine(scratch.FullName, GameInstallationLayout.EnemyTileDirectoryName, Path.GetFileName(file)));
            string stock = Path.Combine(scratch.FullName, EnemySheetDefinitions.DirectoryName);
            EnemySheetFiles.Extract(content, stock, SupportedCartridge.Sha256);
            InstalledEnemySheets installed = EnemySheetFiles.Load(stock, null);

            EnemySheetInputs inputs = EnemySheetInputs.Load(content);
            int members = 0, frames = 0;
            foreach (RoomEnemyGraphicsSetDefinition set in RoomEnemyGraphicsSetDefinitions.DrawnUnder)
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
            int renamedSheets = VerifyLegacyKeyedOverrideSheets(scratch.FullName, stock);
            Console.WriteLine($"Enemy sheets: {renamedSheets} override sheets keyed by older frame keys load as the stock sheets they copy.");
        }
        finally
        {
            scratch.Delete(recursive: true);
        }
    }

    /// <summary>
    /// Copies every stock sheet showing a frame that had an older key into a player's override directory with its pictures
    /// named by those older keys, as an override written before enemy frames were named for their meaning names them, and
    /// loads the sheets with those overrides: each must still hold exactly its frames' pictures (#1465).
    /// </summary>
    /// <returns>The override sheets written.</returns>
    private static int VerifyLegacyKeyedOverrideSheets(string scratch, string stock)
    {
        Dictionary<string, string> legacyKeys = EnemySpritemapDefinitions.Frames.Concat(EnemyExtendedFrameDefinitions.Frames)
            .Where(definition => definition.LegacyName != definition.Name)
            .ToDictionary(definition => EnemyFrameSheetKeys.Of(definition.Frame), definition => PascalCase(definition.LegacyName), StringComparer.Ordinal);
        string overrides = Path.Combine(scratch, "legacy-keyed-sheet-overrides");
        Directory.CreateDirectory(overrides);
        int renamed = 0;
        foreach (string path in Directory.GetFiles(stock, "*.json"))
        {
            // Only a sheet's own frame manifest is named for its owner and ordinal.
            if (!Path.GetFileNameWithoutExtension(path).Contains('.', StringComparison.Ordinal))
                continue;
            JsonObject manifest = JsonNode.Parse(File.ReadAllBytes(path)) as JsonObject
                ?? throw new InvalidOperationException($"Verification requires sheet manifest {path} to be a JSON object.");
            JsonObject pictures = manifest["frames"] as JsonObject
                ?? throw new InvalidOperationException($"Verification requires sheet manifest {path} to list its frames.");
            var older = new JsonObject();
            bool changed = false;
            foreach ((string picture, JsonNode? value) in pictures.ToList())
            {
                pictures.Remove(picture);
                string legacy = LegacyPicture(picture, legacyKeys);
                changed |= legacy != picture;
                older.Add(legacy, value);
            }
            if (!changed)
                continue;
            manifest["frames"] = older;
            File.WriteAllText(Path.Combine(overrides, Path.GetFileName(path)), manifest.ToJsonString());
            renamed++;
        }
        AssertTrue(renamed > 0, "some installed enemy sheet shows a frame that had an older key");
        _ = EnemySheetFiles.Load(stock, overrides);
        return renamed;
    }

    /// <summary><paramref name="picture"/> with each current frame key in it replaced by the frame's older key.</summary>
    private static string LegacyPicture(string picture, Dictionary<string, string> legacyKeys)
    {
        var older = new StringBuilder(picture.Length);
        int start = 0;
        while (start < picture.Length)
        {
            bool word = char.IsAsciiLetterOrDigit(picture[start]);
            int end = start;
            while (end < picture.Length && char.IsAsciiLetterOrDigit(picture[end]) == word)
                end++;
            string run = picture[start..end];
            older.Append(word && legacyKeys.TryGetValue(run, out string? key) ? key : run);
            start = end;
        }
        return older.ToString();
    }

    /// <summary>An older composition name's underscore-separated words, each capitalized, joined: its older sheet key.</summary>
    private static string PascalCase(string name) => string.Concat(name.Split('_')
        .Select(word => word.Length == 0 ? word : char.ToUpperInvariant(word[0]) + word[1..]));

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
