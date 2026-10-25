using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Enemies;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Bosses.SporeSpawn;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Spore Spawn's sway phase, stored as a turn angle, wraps around the turn exactly as the cartridge's byte phase does.</summary>
internal static class SporeSpawnSwayVerification
{
    /// <summary>
    /// Runs the production movement update across both ends of the turn: a negative step from the first table unit
    /// wraps to the last, and a positive step from the last wraps to zero, as the cartridge's byte-truncated add does.
    /// </summary>
    internal static void VerifySporeSpawnSwayPhaseWraps()
    {
        foreach ((byte start, int delta, byte expected) in new VerifySporeSpawnSwayPhaseWrapsCase[]
                 {
                     new VerifySporeSpawnSwayPhaseWrapsCase(1, -2, 255),
                     new VerifySporeSpawnSwayPhaseWrapsCase(255, 1, 0),
                     new VerifySporeSpawnSwayPhaseWrapsCase(254, 2, 0),
                     new VerifySporeSpawnSwayPhaseWrapsCase(0, -1, 255),
                     new VerifySporeSpawnSwayPhaseWrapsCase(128, 2, 130),
                 })
        {
            var enemies = new RoomEnemySystem(new EnemySession());
            RoomEnemySlot body = enemies.SlotTable.Slots[0];
            body.Bind(EnemyDefinitionId.SporeSpawn, body.Definition);
            body.Position = body.Position.WithWhole(new(128, 624));
            var state = new SporeSpawnEnemyState(body)
            {
                Function = SporeSpawnFunction.Moving,
                MovementCenter = body.Position.Whole,
                MaximumXRadius = 48,
                AngleDelta = delta,
                Angle = RotationAngle.FromTableIndex(start),
            };
            enemies.SporeSpawnBehavior.SporeSpawn = state;
            var frame = new ClockedEnemyAiFrame(
                new RoomEnemyFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false },
                new FrameCounters(0));

            enemies.SporeSpawnBehavior.RunSporeSpawnMainAi(body, frame);

            AssertEqual(RotationAngle.FromTableIndex(expected), state.Angle,
                $"Spore Spawn sway phase {start} stepped by {delta} wraps around the turn");
        }
        Console.WriteLine("  Spore Spawn sway: the phase wraps around the turn in both directions.");
    }

    /// <summary>Runs the Spore Spawn sway-phase wrap check.</summary>
    [VerificationCommand("--spore-spawn-sway")]
    internal static int SporeSpawnSwayCommand()
    {
        Suite(nameof(VerifySporeSpawnSwayPhaseWraps), () => VerifySporeSpawnSwayPhaseWraps());
        return 0;
    }
}
