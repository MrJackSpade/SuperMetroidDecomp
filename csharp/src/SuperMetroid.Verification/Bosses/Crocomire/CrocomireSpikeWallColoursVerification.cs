using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>
/// The pieces of the wall Crocomire's skeleton breaks through keep the wall's colours as they fall (#2657): the game clears
/// his palette before it spawns them, so they show the wall's and the wall spikes' rows, never his skeleton's hurt flash,
/// which the port drew them in once they fell clear of the wall.
/// </summary>
internal static class CrocomireSpikeWallColoursVerification
{
    /// <summary>Breaks the wall through the skeleton's reveal and checks each piece's shading.</summary>
    internal static void VerifyCrocomireSpikeWallPiecesShowWallColours()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        CrocomireEnemyState state = CrocomirePlacement.PlaceCrocomire(enemies, new(576, 144));
        // His death has shown the wall spikes' colours by the time his skeleton breaks the wall.
        state.Colours.ShowWallSpikes();
        enemies.CrocomireBehavior.Skeleton.RunCrocomireSkeletonRevealAndWallBreak(state);

        EnemyProjectile[] pieces = [.. enemies.Projectiles.EnemyProjectiles.Where(
            projectile => projectile.Kind == RoomEnemyProjectileKind.CrocomireSpikeWallPieces)];
        AssertEqual(8, pieces.Length, "the skeleton breaks the wall into eight pieces");
        AssertTrue(state.Body.Shading.Shades.Contains(EnemyShades.HurtFlash), "the skeleton itself draws in the hurt flash look");
        foreach (EnemyProjectile piece in pieces)
            AssertTrue(!piece.Graphics.Shading.Shades.Contains(EnemyShades.HurtFlash), "a wall piece does not draw in the skeleton's hurt flash");
        // The colours the game shows the pieces in: its breakable wall's and wall spikes' colours ($A4:B8BD, $A4:B91D), which
        // its sprite rows two and three hold unchanged from before the skeleton breaks the wall until the pieces have fallen.
        var catalog = InstalledFixtures.BossArtwork.CrocomireColors;
        HashSet<RgbColor> wall = [.. Enumerable.Range(1, 15).SelectMany(index => new[]
        {
            catalog.Get(SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork.CrocomireColorCatalog.Band.InitialWall, index),
            catalog.Get(SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork.CrocomireColorCatalog.Band.WallSpikes, index),
        })];
        foreach (EnemyProjectile piece in pieces)
        {
            // Each piece is coloured as drawing it colours it (#2660): from the sheet its graphics name in Crocomire's room,
            // under the shading it carries, which reads every shade it names from that sheet.
            SpriteSheetFrame picture = InstalledFixtures.EnemySheets.Spawned(RoomEnemyGraphicsSetDefinitions.Crocomire, piece.Graphics.Owner)
                .Frame(EnemySheetDefinitions.ProjectileFrameName(EnemyProjectileFrameId.CrocomireSpikeWallPiece));
            var shown = piece.Graphics.Shading.ColoursOf(picture.Sheet, ColourBlendOracle.Instance);
            HashSet<RgbColor> drawn = [];
            for (int y = picture.Corner.Y; y < picture.Corner.Y + picture.Size.Y; y++)
            for (int x = picture.Corner.X; x < picture.Corner.X + picture.Size.X; x++)
            {
                int pixel = y * picture.Sheet.Size.X + x;
                if (picture.Sheet.Pixels[pixel].A != 0)
                    drawn.Add(shown[picture.Sheet.KeyAt(pixel)]);
            }
            RgbColor[] foreign = [.. drawn.Where(colour => !wall.Contains(colour))];
            AssertTrue(drawn.Count > 0 && foreign.Length == 0,
                $"a falling wall piece shows only the wall's and its spikes' colours; {foreign.Length} of its {drawn.Count} are others ({string.Join(", ", foreign)})");
        }
        Console.WriteLine("  Crocomire spike wall: the falling pieces draw in the wall's colours, not the skeleton's hurt flash.");
    }

    /// <summary>Runs the Crocomire spike wall colours check.</summary>
    [VerificationCommand("--crocomire-spike-wall-colours")]
    internal static int CrocomireSpikeWallColoursCommand()
    {
        Suite(nameof(VerifyCrocomireSpikeWallPiecesShowWallColours), VerifyCrocomireSpikeWallPiecesShowWallColours);
        return 0;
    }
}
