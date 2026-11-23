using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.NoobTube;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// The n00b tube's pieces draw on the installed shared sheets they spawn onto (#2626): every sheet holding the tube's
/// crack, shard and bubble pictures is pre-coloured in the crack colours the pieces draw under, so colouring them does not
/// throw as it did when the Glass Tunnel's tube shattered.
/// </summary>
internal static class NoobTubePieceShadingVerification
{
    /// <summary>Colours every installed tube piece picture in the pieces' shading.</summary>
    internal static void VerifyTubePiecesColourInCrackColours()
    {
        PictureShading pieces = PictureShading.Shaded(ChozoAndTubeShades.TubeCracks);
        string[] names = [.. NoobTubeProjectileAnimations.ShownFrames.Select(EnemySheetDefinitions.ProjectileFrameName)];
        int coloured = 0;
        foreach ((string variant, var artwork) in InstalledFixtures.EnemySheets.spawnedVariants.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            foreach (string name in names.Where(artwork.Sheet.frames.ContainsKey))
            {
                SpriteImage sheet = artwork.Sheet.Frame(name).Sheet;
                AssertTrue(sheet.Shades.ContainsKey(ChozoAndTubeShades.TubeCracks),
                    $"spawned variant {variant}'s sheet {sheet.Name} holding {name} is pre-coloured in the tube's crack colours");
                _ = pieces.KeyColours(sheet, ColourBlendOracle.Instance);
                coloured++;
            }
        }
        AssertTrue(coloured > 0, "some installed spawned-object sheet holds the tube's pieces");
    }

    /// <summary>Runs the tube pieces' shading check.</summary>
    [VerificationCommand("--noob-tube-piece-shading")]
    internal static int NoobTubePieceShadingCommand()
    {
        Suite(nameof(VerifyTubePiecesColourInCrackColours), VerifyTubePiecesColourInCrackColours);
        return 0;
    }
}
