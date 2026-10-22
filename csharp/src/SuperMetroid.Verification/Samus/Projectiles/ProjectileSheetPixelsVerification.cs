using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Spatial;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>
/// Confirms the projectile sheets (#1392): every frame of the power-beam, SpaceTime and intro sheets paints exactly the
/// pixels, priorities and colour-math participation its composition paints over its OBJ page in the sheet's colours.
/// </summary>
internal static class ProjectileSheetPixelsVerification
{
    /// <summary>The square the frames are rendered in; each frame's origin lands at its centre.</summary>
    private static readonly Extent16<ScreenPixels> Canvas = new(256, 256);

    /// <summary>The OBJ palette rows at or past which a sprite takes part in colour math.</summary>
    private const int FirstBlendingRow = 4;

    /// <summary>Renders each frame both ways and compares them.</summary>
    internal static void VerifyProjectileSheetPixels()
    {
        int frames = 0;
        SamusSheetInputs inputs = SamusSheetInputs.Load(RepositoryInstallation.Installation.ContentDirectory);
        foreach (SpriteSheetId id in new[] { SpriteSheetId.ProjectilesPower, SpriteSheetId.ProjectilesSpazerPlasmaIce, SpriteSheetId.IntroProjectiles, SpriteSheetId.Grapple })
        {
            SpriteSheetSources sources = id == SpriteSheetId.Grapple ? GrappleSheetComposer.Sources(inputs) : ProjectileSheetComposer.Sources(inputs, id);
            SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(id);
            var cgram = new SnesCgram();
            for (int colour = 0; colour < sources.SpriteColours.Length; colour++)
                cgram.SetColor(SpriteColours + colour, sources.SpriteColours[colour]);
            var origin = new Position16<ScreenPixels>((ushort)(Canvas.X / 2), (ushort)(Canvas.Y / 2));
            foreach ((string name, SpriteFrame frame, SpriteArtwork characters) in sources.Frames)
            {
                var composed = new CharacterSpriteDraw(new SpriteSource(SpriteSourceKind.SamusProjectile), frame, characters, origin,
                    SnesTileFlipFlags.None, 0, 0);
                SpriteSheetFrame image = sheet.Frame(name);
                var drawn = new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusProjectile), image, origin, SpriteMirror.None, image.Priority);
                Raster expected = Render(composed, cgram), actual = Render(drawn, cgram);
                for (int pixel = 0; pixel < expected.Pixels.Length; pixel++)
                {
                    if (expected.Pixels[pixel] != actual.Pixels[pixel] || expected.Priorities[pixel] != actual.Priorities[pixel] ||
                        Blends(expected.Palettes[pixel]) != Blends(actual.Palettes[pixel]))
                    {
                        throw new InvalidOperationException($"Verification failed: {id} frame {name} differs from its composition at ({pixel % Canvas.X - Canvas.X / 2}, " +
                            $"{pixel / Canvas.X - Canvas.Y / 2}): {actual.Pixels[pixel]} priority {actual.Priorities[pixel]} palette " +
                            $"{actual.Palettes[pixel]}, expected {expected.Pixels[pixel]} priority {expected.Priorities[pixel]} palette {expected.Palettes[pixel]}.");
                    }
                }
                frames++;
            }
        }
        Console.WriteLine($"Projectile sheet pixels: {frames} power-beam, SpaceTime and intro frames paint exactly their compositions.");

        static bool Blends(byte row) => row != SnesObjRenderer.TransparentPriority && row >= FirstBlendingRow;
    }

    /// <summary>CGRAM's first sprite colour.</summary>
    private const int SpriteColours = 128;

    /// <summary>Renders one draw on the canvas.</summary>
    private static Raster Render(SpriteDraw draw, SnesCgram cgram)
    {
        int count = Canvas.X * Canvas.Y;
        var result = new Raster(new Rgba32[count], new byte[count], new byte[count]);
        SnesObjRenderer.RenderResolved([draw], cgram, new ObjRasterTargets(result.Pixels, result.Priorities, result.Palettes), Canvas);
        return result;
    }

    /// <summary>One rendered canvas.</summary>
    /// <param name="Pixels">Colours, row by row.</param>
    /// <param name="Priorities">Each pixel's winning priority, or transparent.</param>
    /// <param name="Palettes">Each pixel's palette row, or transparent.</param>
    private sealed record Raster(Rgba32[] Pixels, byte[] Priorities, byte[] Palettes);

    /// <summary>Runs the projectile sheet pixel check against the repository installation.</summary>
    [VerificationCommand("--projectile-sheet-pixels")]
    internal static int ProjectileSheetPixelsCommand()
    {
        Suite(nameof(VerifyProjectileSheetPixels), VerifyProjectileSheetPixels);
        return 0;
    }
}
