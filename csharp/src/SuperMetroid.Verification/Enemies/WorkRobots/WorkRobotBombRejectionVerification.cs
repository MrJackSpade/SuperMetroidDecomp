using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.WorkRobots;

/// <summary>
/// Checks that each Work Robot part's own entity supplies its bomb reaction (#2164): the powered robot's rejects a bomb
/// that selects its own shot reaction until Phantoon is defeated, and the unpowered robot's never rejects one, as the
/// shared reaction's per-call definition test decided before.
/// </summary>
internal static class WorkRobotBombRejectionVerification
{
    /// <summary>Asks each robot's entity for its bomb reaction with Phantoon alive and defeated.</summary>
    internal static void VerifyWorkRobotBombRejection()
    {
        foreach (bool phantoonDefeated in new[] { false, true })
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
                definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            enemies.VerificationServices().IsAreaBossDefeated = () => phantoonDefeated;
            var ordinaryProjectiles = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads,
                new LeftMuzzleSubpixelCarry(SamusProjectileSystem.PlaceCount), new SamusShotCount(), InstalledFixtures.SamusArtwork());
            foreach ((EnemyDefinitionId robot, bool powered) in new[] { (EnemyDefinitionId.WorkRobot, true), (EnemyDefinitionId.UnpoweredWorkRobot, false) })
            {
                RoomEnemyPlace place = enemies.Places.InUpdateOrder[powered ? 0 : 1];
                place.Bind(robot, place.Definition);
                var bomb = new SamusBomb(0)
                {
                    Type = SamusProjectileType.Of(SamusProjectileFamily.Bomb),
                    Position = FixedPosition<RoomPixels>.FromWhole(place.Position.Whole),
                };
                var context = new EnemyBombContext(place, bomb, ordinaryProjectiles, RoomEnemyFrameFixtures.StandaloneSamus(),
                    SelectedShot: null, UsesFrameHitboxes: false, new Position16<RoomPixels>(0, 0))
                {
                    StruckOccupant = place.OccupantStates,
                };
                EnemyBombReaction reaction = (place.Entity as IEnemyBombReaction)?.CommonBombDamageReaction(context)
                    ?? throw new InvalidOperationException($"{robot} has no bomb reaction of its own.");
                bool rejects = reaction.Rejects?.Invoke(context) ?? false;
                AssertEqual(powered && !phantoonDefeated, rejects,
                    $"{robot}'s own bomb reaction with Phantoon {(phantoonDefeated ? "defeated" : "alive")}");
                AssertTrue(reaction.AfterCommon is not null, $"{robot}'s own bomb reaction recoils after the common damage");
            }
        }
        Console.WriteLine("  Work Robot bomb rejection: the powered robot rejects bombs until Phantoon is defeated and the unpowered one never does.");
    }

    /// <summary>Runs the Work Robot bomb rejection check.</summary>
    [VerificationCommand("--work-robot-bomb-rejection")]
    internal static int WorkRobotBombRejectionCommand()
    {
        Suite(nameof(VerifyWorkRobotBombRejection), () => VerifyWorkRobotBombRejection());
        return 0;
    }
}
