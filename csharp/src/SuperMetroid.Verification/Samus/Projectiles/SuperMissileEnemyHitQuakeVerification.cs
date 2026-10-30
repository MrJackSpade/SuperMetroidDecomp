using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks the quake issued by a Super Missile enemy hit through the projectile collision prelude.</summary>
internal static class SuperMissileEnemyHitQuakeVerification
{
    /// <summary>
    /// A Super Missile touching an enemy starts quake type $12 for thirty calls
    /// ($A0:9CB7/$A0:A1E9); only its impact on a block ($93:8125) uses type $14, the type
    /// crawlers fall for. In the 13% movie a Super Missile kills a Sciser in Crab Maze and
    /// the other Scisers keep crawling.
    /// </summary>
    internal static void VerifySuperMissileEnemyHitQuake()
    {
        var room = CreateRoom(new(64, 16), new RoomTerrainBlock[64 * 16], new RoomBlockBehavior[64 * 16]);
        var samus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(64, 96)),
            SelectedHudItem = HudItem.SuperMissile, SuperMissiles = 3,
        };
        SamusProjectileSystem projectiles = CreateProjectileFixture();
        SamusBombProjectileSystem bombs = CreateBombFixture();
        projectiles.StepFrame(new SamusTerrain { Level = room }, samus, new ProjectileButtonInput { ControllerInput = ControllerButton.X, ControllerNewInput = ControllerButton.X }, new(0, 0), bombs);
        AssertEqual(SamusProjectileFamily.SuperMissile, projectiles.Slots[0].Type.Family, "X fires a Super Missile");

        projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(0, markCollisionState: true);
        AssertEqual(SuperMissileDefinitions.EnemyHitEarthquakeType, projectiles.Earthquakes.EarthquakeType,
            "an enemy hit starts quake type $12");
        AssertEqual(SuperMissileDefinitions.EarthquakeDuration, projectiles.Earthquakes.EarthquakeTimer,
            "for thirty calls");
        Console.WriteLine("  Super Missile enemy-hit quake: type $12, which leaves crawlers crawling.");
    }

    /// <summary>Runs the focused enemy-hit quake verification command.</summary>
    [VerificationCommand("--super-missile-enemy-hit-quake")]
    internal static int SuperMissileEnemyHitQuakeCommand()
    {
        Suite(nameof(VerifySuperMissileEnemyHitQuake), () => VerifySuperMissileEnemyHitQuake());
        return 0;
    }
}
