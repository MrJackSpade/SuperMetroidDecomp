using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Real runtime crash-finish admission with zero bombs and live combo particles.</summary>
internal static class SparkCapacityInputAudit
{
    public static int Run(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int failures = 0;
        foreach (ushort beam in new ushort[] { 0, 1, 2, 4, 8 })
        {
            var runtime = FlatFloorMovementFixture.Create(bus, water: false);
            var samus = runtime.Samus!;
            if (beam != 0)
            {
                samus.EquippedBeams = (ushort)(0x1000 | beam);
                samus.PowerBombs = samus.MaxPowerBombs = 2;
                samus.SelectedHudItem = 3;
                if (!runtime.Projectiles.TryActivateCombo(bus, samus, runtime.BombProjectiles, out _))
                    throw new InvalidDataException("Expected live combo allocation.");
            }
            samus.Pose = SamusPoseIds.ShinesparkHorizontalRightPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            // Isolate the reported finish boundary, then run the actual alpha/beta
            // dispatcher; do not pass an expected projectile count directly to Step.
            typeof(SamusShinesparkState).GetProperty(nameof(SamusShinesparkState.Phase))!
                .SetValue(samus.Shinespark, ShinesparkPhase.CrashFinish);
            runtime.StepFrame(0);
            int expected = beam == 0 ? 2 : 1;
            int actual = samus.Shinespark.ReleasedCrashEchoCount;
            int expectedCount = beam == 0 ? 2 : 5;
            bool ownsExpectedSlots = runtime.Projectiles.Slots[4].PreInstruction == SamusProjectilePreInstruction.ShinesparkEcho &&
                (beam != 0 || runtime.Projectiles.Slots[3].PreInstruction == SamusProjectilePreInstruction.ShinesparkEcho);
            if (runtime.LastShinesparkMovement?.CrashSequenceFinished != true || actual != expected ||
                runtime.Projectiles.ProjectileCounter != expectedCount || !ownsExpectedSlots)
            {
                failures++;
                Console.WriteLine($"SPARK CAPACITY beam={beam}: echoes={actual}, expected={expected}, " +
                    $"projectiles={runtime.Projectiles.ProjectileCounter}, bombs={runtime.BombProjectiles.BombCounter}.");
            }
            // The next real alpha pass must advance the owned echo exactly once,
            // including its ordinary instruction-list pass, before Samus moves.
            runtime.StepFrame(0);
            if (samus.Shinespark.SecondReleasedCrashEcho.Radius != 8 ||
                runtime.Projectiles.Slots[4].XVelocity != 8)
            {
                failures++;
                Console.WriteLine($"SPARK STEP beam={beam}: shared radius/alpha dispatch mismatch.");
            }
        }
        Console.WriteLine($"Spark runtime capacity: 5 cases, {failures} mismatches.");
        return failures == 0 ? 0 : 1;
    }
}
