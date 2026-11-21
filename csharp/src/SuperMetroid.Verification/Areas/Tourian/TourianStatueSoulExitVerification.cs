using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>
/// Checks where the Tourian statues' released souls and falling particles end, through the production motion steps
/// (#1956): a soul rising from each statue's eye is deleted the update it rises above the room's top, and a particle thrown
/// from each eye at each of its 64 launch angles the update it sinks to the bottom of the statues' screen, including one
/// thrown above the room's top that falls back. Each is deleted on the same update the original's test of the 256-pixel
/// screen row its height lay in (an odd row for a soul, the second row for a particle) deleted it.
/// </summary>
internal static class TourianStatueSoulExitVerification
{
    /// <summary>The updates any effect is followed before it must have been deleted.</summary>
    private const int UpdateLimit = 400;

    /// <summary>The original's 256-pixel screen row of a stored height.</summary>
    private static int ScreenRow(int y) => y / 256;

    /// <summary>Whether <paramref name="effect"/> has left the room: its motion ends it (#2424).</summary>
    private static bool IsDeleting(EnemyProjectile effect) => !effect.IsActive;

    /// <summary>The room, Samus and bombs an effect's motion is handed, of which the souls' and particles' motions read none.</summary>
    private static readonly Lazy<MotionInput> MotionInputs =
        new(() => new MotionInput(RoomLevelData.CollisionOnly(new(1, 1), [RoomCollisionType.Air]), RoomEnemyFrameFixtures.StandaloneSamus(),
            InstalledFixtures.CreateBombSystem()));

    /// <summary>What an effect's motion is handed besides the effect.</summary>
    /// <param name="Room">The room's blocks.</param>
    /// <param name="Samus">Samus.</param>
    /// <param name="Bombs">Samus's bombs.</param>
    private sealed record MotionInput(RoomLevelData Room, SuperMetroid.Core.Samus.SamusState Samus,
        SuperMetroid.Core.Samus.Projectiles.SamusBombProjectileSystem Bombs);

    /// <summary>Moves <paramref name="effect"/> one update by the motion its spawn gave it, as the projectile pass does (#2482).</summary>
    private static void Move(TourianUnlockBehavior unlock, EnemyProjectile effect) =>
        unlock.Effects.RunMotion(new EnemyProjectileMotionContext(effect, MotionInputs.Value.Room, MotionInputs.Value.Samus,
            Position16<RoomPixels>.Zero, ShortCounter: SuperMetroid.Core.Compatibility.ShortFrameCount.Zero, FrameCounter: 0, MotionInputs.Value.Bombs));

    /// <summary>Follows every soul and particle from the statues' eyes to its deletion.</summary>
    internal static void VerifyTourianStatueSoulExit()
    {
        int updates = 0;
        int thrownAboveTop = 0;
        foreach (TourianStatueAnimationDefinition statue in TourianStatueAnimationDefinitions.Statues.Values)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
                statueRoom: new SuperMetroid.Core.World.TourianStatueRoom(new SuperMetroid.Core.Fx.RoomLiquidSurfaceLine(216)));
            EnemySystemFixtures.UnlockEffects(enemies).SpawnTourianSoul(statue.Eye);
            EnemyProjectile soul = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive);
            for (int update = 0; ; update++)
            {
                AssertTrue(update < UpdateLimit, $"the {statue.Statue} statue's soul leaves the room");
                Move(EnemySystemFixtures.UnlockEffects(enemies), soul);
                updates++;
                int y = soul.Position.Y.Whole;
                AssertEqual(ScreenRow(y) % 2 != 0, IsDeleting(soul),
                    $"the {statue.Statue} statue's soul at Y {y} is deleted on the update the original's odd screen row deleted it");
                if (!IsDeleting(soul))
                    continue;
                AssertTrue(CoordinateAxis.AsSigned(y) < 0, $"the {statue.Statue} statue's soul is deleted above the room's top");
                break;
            }
        }

        for (ushort random = 0; random < 64; random++)
        foreach (TourianStatueAnimationDefinition statue in TourianStatueAnimationDefinitions.Statues.Values)
        {
            ushort sample = random;
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
                services: new StandaloneRoomEnemyServices(() => sample), statueRoom: new SuperMetroid.Core.World.TourianStatueRoom(new SuperMetroid.Core.Fx.RoomLiquidSurfaceLine(216)));
            TourianUnlockBehavior unlock = EnemySystemFixtures.UnlockEffects(enemies);
            unlock.SpawnTourianSoul(statue.Eye);
            EnemyProjectile soul = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive);
            unlock.RunCue(new EnemyProjectileCueContext(soul, MotionInputs.Value.Samus, Position16<RoomPixels>.Zero, ShortCounter: SuperMetroid.Core.Compatibility.ShortFrameCount.Zero,
                MotionInputs.Value.Bombs, enemies.Projectiles), new TourianStatueProjectileCue.Signal(TourianStatueProjectileCueKind.SpawnParticle));
            EnemyProjectile particle = enemies.Projectiles.EnemyProjectiles.Single(projectile =>
                projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.TourianStatueParticle);
            bool aboveTop = false;
            for (int update = 0; ; update++)
            {
                AssertTrue(update < UpdateLimit, $"the {statue.Statue} statue's particle {random} sinks");
                Move(unlock, particle);
                updates++;
                int y = particle.Position.Y.Whole;
                aboveTop |= CoordinateAxis.AsSigned(y) < 0;
                AssertEqual(ScreenRow(y) == 1, IsDeleting(particle),
                    $"the {statue.Statue} statue's particle {random} at Y {y} is deleted on the update the original's second screen row deleted it");
                if (!IsDeleting(particle))
                    continue;
                AssertTrue(CoordinateAxis.AsSigned(y) >= TourianStatueParticleDefinitions.SunkDepth,
                    $"the {statue.Statue} statue's particle {random} is deleted at the bottom of the statues' screen");
                break;
            }
            if (aboveTop)
                thrownAboveTop++;
        }
        AssertTrue(thrownAboveTop > 0, "a particle thrown above the room's top falls back and sinks");
        Console.WriteLine($"Tourian statue effects: four souls and 256 particles over {updates} updates end where the original's screen rows ended them; {thrownAboveTop} particles fell back from above the room's top.");
    }

    /// <summary>Runs the focused soul and particle exit check.</summary>
    [VerificationCommand("--tourian-statue-soul-exit")]
    internal static int TourianStatueSoulExitCommand()
    {
        VerifyTourianStatueSoulExit();
        return 0;
    }
}
