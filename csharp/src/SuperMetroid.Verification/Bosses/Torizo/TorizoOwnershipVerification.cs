using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// #2030: a Torizo's state belongs to its entity, so what reads it finds it through the Torizo itself: the belly glow
/// through the room's enemy places, Golden Torizo's held Super Missile and its eye beam's floor spark through the Torizo
/// that caught or fired them.
/// </summary>
internal static class TorizoOwnershipVerification
{
    /// <summary>The spark's updates watched: past its first eight-update frame and its replay, short of a second replay's end.</summary>
    private const int SparkUpdates = 12;

    /// <summary>Runs the belly glow, held Super Missile and eye-beam spark checks.</summary>
    internal static void VerifyTorizoOwnership()
    {
        VerifyBellyGlowFindsTheRoomsTorizo();
        VerifyHeldSuperMissileFollowsItsHolder();
        VerifyEyeBeamSparkWatchesItsOwner(explosionsEnabled: false);
        VerifyEyeBeamSparkWatchesItsOwner(explosionsEnabled: true);
        Console.WriteLine("Torizo ownership: the belly glow, the held Super Missile and the eye beam's spark find the Torizo that owns them.");
    }

    /// <summary>The glow shows on the room's Torizo in whichever place it stands, and fails loudly in a room without one.</summary>
    private static void VerifyBellyGlowFindsTheRoomsTorizo()
    {
        RoomEnemySystem enemies = NewEnemies();
        var frame = new ColourEffectFrame(ColourEffectId.BombTorizoBelly, 0);
        AssertThrows<InvalidOperationException>(() => TorizoBehavior.ShowBellyGlow(enemies.SlotTable.Slots, frame),
            "the belly glow shows on no Torizo in a room without one");
        RoomEnemySlot torizo = enemies.SlotTable.Slots[3];
        TorizoEnemyState state = torizo.BindTorizoFixtureState(0);
        TorizoBody.LoadTorizoBodyColours(torizo, state, new TorizoLook(TorizoLookKind.Normal));
        TorizoBehavior.ShowBellyGlow(enemies.SlotTable.Slots, frame);
        AssertEqual(ColourEffectShades.Of(ColourEffectColours.TorizoBelly, frame), torizo.Shading.Shades[^1],
            "the belly glow shows on the room's Torizo in the place it stands");
    }

    /// <summary>A held Super Missile stays at the hand of the Golden Torizo that caught it as that Torizo moves and turns.</summary>
    private static void VerifyHeldSuperMissileFollowsItsHolder()
    {
        RoomEnemySystem enemies = NewEnemies();
        RoomLevelData level = CreateEmptyRoom(new(64, 64));
        RoomEnemySlot torizo = enemies.SlotTable.Slots[0];
        torizo.Position = torizo.Position.WithWhole(new(400, 400));
        TorizoEnemyState state = torizo.BindTorizoFixtureState(0, isGolden: true);
        enemies.TorizoBehavior.Attacks.SpawnGoldenTorizoSuperMissile(torizo);
        EnemyProjectile missile = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive);

        torizo.Position = torizo.Position.WithWhole(new(300, 350));
        state.Posture = TorizoPostureFlags.FacingRight;
        enemies.StepEnemyProjectiles(level, RoomEnemyFrameFixtures.StandaloneSamus());
        AssertEqual(new Position16<RoomPixels>(300, 350) + TorizoProjectilePlacementDefinitions.HeldSuperMissile, missile.Position.Whole,
            "the held Super Missile stays at the hand of the Golden Torizo that caught it");
    }

    /// <summary>
    /// The eye beam's floor spark sparks again while the Golden Torizo that fired it keeps its explosions disabled, and runs
    /// on to its next frame once they are enabled.
    /// </summary>
    private static void VerifyEyeBeamSparkWatchesItsOwner(bool explosionsEnabled)
    {
        RoomEnemySystem enemies = NewEnemies();
        RoomLevelData level = CreateEmptyRoom(new(64, 64));
        RoomEnemySlot torizo = enemies.SlotTable.Slots[0];
        torizo.Position = torizo.Position.WithWhole(new(400, 400));
        TorizoEnemyState state = torizo.BindTorizoFixtureState(0, isGolden: true);
        state.EyeBeamExplosionsEnabled = explosionsEnabled;
        enemies.TorizoBehavior.Attacks.SpawnGoldenTorizoEyeBeam(torizo);
        EnemyProjectile beam = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive);
        beam.HoldStill();
        beam.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.EyeBeamFloorImpact));

        bool sparkedOn = false;
        SamusState samus = RoomEnemyFrameFixtures.StandaloneSamus();
        for (int update = 0; update < SparkUpdates; update++)
        {
            enemies.StepEnemyProjectiles(level, samus);
            sparkedOn |= beam.Frame == EnemyProjectileFrameId.GoldenTorizoEyeBeamFloorImpactLoopSecond;
        }
        AssertEqual(explosionsEnabled, sparkedOn,
            $"the eye beam's spark runs on only while its Golden Torizo enables its explosions (enabled {explosionsEnabled})");
    }

    /// <summary>A fresh room's enemies, whose random numbers all read zero.</summary>
    private static RoomEnemySystem NewEnemies()
    {
        RoomEnemySystem enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => 0;
        return enemies;
    }
}
