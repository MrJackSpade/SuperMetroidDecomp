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
        {
            var shades = piece.Graphics.Shading.Shades;
            AssertTrue(!shades.Contains(EnemyShades.HurtFlash), "a wall piece does not draw in the skeleton's hurt flash");
            AssertTrue(shades.Contains(CrocomireShades.WallSpikes), "a wall piece draws in the wall spikes' colours his death loaded");
            // The piece draws from the sheet its graphics name in Crocomire's room, which must hold every shade it is drawn
            // in (#2660): drawing it reads each one, and a shade the sheet lacks threw as the pieces fell.
            SpriteSheetFrame picture = InstalledFixtures.EnemySheets.Spawned(RoomEnemyGraphicsSetDefinitions.Crocomire, piece.Graphics.Owner)
                .Frame(EnemySheetDefinitions.ProjectileFrameName(EnemyProjectileFrameId.CrocomireSpikeWallPiece));
            foreach (PictureShade shade in shades)
                _ = picture.Sheet.Shade(shade);
        }
        // Its sheet shows it in the wall's colours: the frame family is pre-coloured in them (#1513).
        AssertTrue(FrameColouringDefinitions.Projectiles[EnemyProjectileFrameId.CrocomireSpikeWallPiece].Shades.Contains(CrocomireShades.Wall),
            "the wall pieces' pictures are pre-coloured in the wall's colours");
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
