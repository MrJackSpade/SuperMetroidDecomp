using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.SporeSpawn;

/// <summary>
/// Spore Spawn's death drift is one signed velocity set as his death begins (#2243): from each side of the arena's centre
/// at (128,624) it points toward the centre on both axes, and every dying update moves him by exactly it.
/// </summary>
internal static class SporeSpawnDeathDriftVerification
{
    /// <summary>
    /// Sets up the death drift from one start in each quadrant around the arena's centre, then runs two dying updates and
    /// checks each moves the body by the velocity the setup stored, which neither update changes.
    /// </summary>
    internal static void VerifySporeSpawnDeathDrift()
    {
        foreach ((Position16<RoomPixels> start, int xSign, int ySign) in new (Position16<RoomPixels>, int, int)[]
                 {
                     (new(60, 560), 1, 1),
                     (new(200, 700), -1, -1),
                     (new(60, 700), 1, -1),
                     (new(200, 560), -1, 1),
                 })
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
                services: new VerificationRoomEnemyServices(null));
            enemies.VerificationServices().NextRandom = () => 1;
            RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
            body.Bind(EnemyDefinitionId.SporeSpawn, body.Definition);
            body.Position = body.Position.WithWhole(start);
            var state = new SporeSpawnEnemyState
            {
                Movement = SporeSpawnMovement.SetupDeath,
                MovementCenter = new(128, 624),
            };
            state.StalkLinks = [.. SporeSpawnProjectileDefinitions.StalkSegments.Select(segment => enemies.SporeSpawnBehavior.SpawnSporeSpawnStalk(body.HeldOccupant(), body, segment))];
            body.SetSpeciesState(state);

            enemies.SporeSpawnBehavior.RunSporeSpawnMainAi(body, state);
            FixedVector<RoomPixels> velocity = state.DeathVelocity;
            string from = $"Spore Spawn's death drift from ({start.X},{start.Y})";
            AssertEqual(start, body.Position.Whole, $"{from}: setting it up does not move him");
            AssertEqual(xSign, Math.Sign(velocity.X.RawValue), $"{from}: X points toward the arena's centre");
            AssertEqual(ySign, Math.Sign(velocity.Y.RawValue), $"{from}: Y points toward the arena's centre");

            state.Movement = SporeSpawnMovement.Dying;
            for (int update = 0; update < 2; update++)
            {
                FixedPosition<RoomPixels> expected = body.Position.AdvanceX(velocity.X).AdvanceY(velocity.Y);
                enemies.SporeSpawnBehavior.RunSporeSpawnMainAi(body, state);
                AssertEqual(expected, body.Position, $"{from}: dying update {update} moves him by the velocity set up");
                AssertEqual(velocity, state.DeathVelocity, $"{from}: dying update {update} keeps the velocity");
            }
        }
        Console.WriteLine("  Spore Spawn death drift: one signed velocity toward the arena's centre, set once and applied each dying update.");
    }

    /// <summary>Runs the Spore Spawn death drift check.</summary>
    [VerificationCommand("--spore-spawn-death-drift")]
    internal static int SporeSpawnDeathDriftCommand()
    {
        Suite(nameof(VerifySporeSpawnDeathDrift), () => VerifySporeSpawnDeathDrift());
        return 0;
    }
}
