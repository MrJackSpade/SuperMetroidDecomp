using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Spatial;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Confirms the installed enemy sheets (#1392): extraction writes a sheet for every graphics-set member whose behaviour
/// draws, loading names each set's sheet for each of its enemies, and every frame of every installed sheet paints
/// exactly what its composition paints over the set's characters and colours, in its own and in altered colours.
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
                foreach ((EnemyDefinitionId enemy, SpriteSheetSources sources) in EnemySheetComposer.Sources(inputs, set).Enemies)
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
            Console.WriteLine($"Enemy sheets: {members} graphics-set members, {frames} frames pixel-exact against their compositions.");
        }
        finally
        {
            scratch.Delete(recursive: true);
        }
    }

    /// <summary>
    /// The sheet frame draws exactly what its composition draws: in the set's colours from the image, and in altered
    /// colours through its palette map.
    /// </summary>
    private static void AssertSamePixels(SpriteSheetFrame frame, SheetFrameSource source, SpriteSheetSources sources, string label)
    {
        foreach (bool altered in new[] { false, true })
        {
            var cgram = new SnesCgram();
            for (int colour = 0; colour < sources.SpriteColours.Length; colour++)
            {
                cgram.SetColor(128 + colour, altered
                    ? Bgr555.FromWord((ushort)((sources.SpriteColours[colour].ToWord() + 0x0421 * (colour % 7 + 1)) & 0x7fff))
                    : sources.SpriteColours[colour]);
            }
            var origin = new Position16<ScreenPixels>(128, 112);
            var image = new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.Enemy), frame, origin, SpriteMirror.None);
            // The cartridge ORs the enemy's palette bits into each part's own row.
            var ored = new SpriteFrame(source.Frame.Name, [.. source.Frame.Parts.Select(part => part with { Palette = (part.Palette ?? 0) | sources.PaletteBits })]);
            var character = new CharacterSpriteDraw(new SpriteSource(SpriteSourceKind.Enemy), ored, source.Characters, origin,
                SnesTileFlipFlags.None, 0, 0);
            Raster expected = Render(character, cgram), actual = Render(image, cgram);
            string colours = altered ? "altered" : "own";
            for (int pixel = 0; pixel < expected.Pixels.Length; pixel++)
            {
                if (expected.Pixels[pixel] != actual.Pixels[pixel] || expected.Priorities[pixel] != actual.Priorities[pixel] ||
                    expected.Palettes[pixel] != actual.Palettes[pixel])
                    throw new InvalidOperationException($"{label} ({colours} colours) differs at screen pixel {pixel}.");
            }
        }
    }

    /// <summary>Renders one draw to a full screen's colours, priorities and palette rows.</summary>
    private static Raster Render(SpriteDraw draw, SnesCgram cgram)
    {
        int count = SnesPpuLayout.ScreenSize.X * SnesPpuLayout.ScreenSize.Y;
        var raster = new Raster(new Rgba32[count], new byte[count], new byte[count]);
        SnesObjRenderer.RenderResolved([draw], cgram, new ObjRasterTargets(raster.Pixels, raster.Priorities, raster.Palettes));
        return raster;
    }

    /// <summary>One rendered screen.</summary>
    /// <param name="Pixels">Colours, row by row.</param>
    /// <param name="Priorities">Each pixel's winning priority, or transparent.</param>
    /// <param name="Palettes">Each pixel's palette row, or transparent.</param>
    private sealed record Raster(Rgba32[] Pixels, byte[] Priorities, byte[] Palettes);

    /// <summary>Runs the enemy sheet extraction and pixel comparison.</summary>
    [VerificationCommand("--enemy-sheet-files")]
    internal static int EnemySheetFilesCommand()
    {
        Suite(nameof(VerifyEnemySheetFiles), VerifyEnemySheetFiles);
        return 0;
    }
}
