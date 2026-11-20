using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.WorkRobots;

/// <summary>
/// Checks that each Work Robot part's own entity supplies its bomb reaction (#2164): neither the powered robot, which the
/// rooms place only once Phantoon is defeated (#2455), nor the unpowered robot rejects a bomb that selects its own shot
/// reaction, and only the powered one recoils after the common damage.
/// </summary>
internal static class WorkRobotBombRejectionVerification
{
    /// <summary>Asks each robot's entity for its bomb reaction.</summary>
    internal static void VerifyWorkRobotBombRejection()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        var ordinaryProjectiles = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads,
            new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), InstalledFixtures.SamusArtwork(), new global::SuperMetroid.Core.Fx.RoomEarthquake());
        foreach ((EnemyDefinitionId robot, bool powered) in new[] { (EnemyDefinitionId.WorkRobot, true), (EnemyDefinitionId.UnpoweredWorkRobot, false) })
        {
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[powered ? 0 : 1];
            place.Bind(robot, place.Definition);
            var bomb = new SamusBomb(0);
            bomb.OccupyAsPlaced(SamusProjectileType.Of(SamusProjectileFamily.Bomb));
            bomb.Position = FixedPosition<RoomPixels>.FromWhole(place.Position.Whole);
            var context = new EnemyBombContext(place.HeldOccupant(), bomb.HeldBomb(), ordinaryProjectiles, RoomEnemyFrameFixtures.StandaloneSamus(),
                SelectedShot: SuperMetroid.Core.Enemies.WorkRobots.WorkRobotReactions.Shot, UsesFrameHitboxes: false, new Position16<RoomPixels>(0, 0))
            {
                StruckOccupant = place.OccupantStates,
            };
            EnemyBombReaction reaction = (place.Entity as IEnemyBombReaction)?.CommonBombDamageReaction(context)
                ?? throw new InvalidOperationException($"{robot} has no bomb reaction of its own.");
            AssertTrue(reaction.Rejects is null || !reaction.Rejects(context), $"{robot}'s own bomb reaction takes the bomb");
            AssertEqual(powered, reaction.AfterCommon is not null,
                $"{robot}'s own bomb reaction recoils after the common damage only when powered");
        }
        Console.WriteLine("  Work Robot bomb rejection: neither robot rejects a bomb, and only the powered one recoils.");
    }

    /// <summary>Runs the Work Robot bomb rejection check.</summary>
    [VerificationCommand("--work-robot-bomb-rejection")]
    internal static int WorkRobotBombRejectionCommand()
    {
        Suite(nameof(VerifyWorkRobotBombRejection), () => VerifyWorkRobotBombRejection());
        return 0;
    }
}
