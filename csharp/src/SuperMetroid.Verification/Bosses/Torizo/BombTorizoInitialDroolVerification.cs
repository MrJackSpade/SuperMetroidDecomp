using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// Checks the plain form #1620 gave the Bomb Torizo's first drool against the original initializer's chained additions.
/// </summary>
internal static class BombTorizoInitialDroolVerification
{
    /// <summary>
    /// The production first drool of a Bomb Torizo standing anywhere its height and width keep it inside a two-screen
    /// room, both facings and turning, for every jitter the random values select: its height, fall speed and side
    /// position equal the original initializer's chained additions (<c>$86:A65D</c>), the fall speed taking the lift's
    /// carry and the side position the jitter's.
    /// </summary>
    internal static void VerifyBombTorizoInitialDrool()
    {
        const int HalfHeight = 41;
        const int HalfWidth = 18;
        const int RoomExtent = 512;
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        RoomEnemySlot torizo = enemies.SlotTable.Slots[0];
        var randoms = new Queue<ushort>();
        enemies.VerificationServices().NextRandom = randoms.Dequeue;
        int spawns = 0;
        foreach ((int posture, string name) in new[] { (0x0000, "facing left"), (0x8000, "facing right"), (0x4000, "turning") })
        {
            enemies.BombTorizoBehavior.BindFixtureState(torizo, posture);
            for (int y = HalfHeight; y <= RoomExtent - HalfHeight; y += 3)
            for (int x = HalfWidth; x <= RoomExtent - HalfWidth; x += 79)
            for (int sample = 0; sample < 32; sample += 4)
            {
                ushort first = unchecked((ushort)(sample * 0x0421));
                ushort second = unchecked((ushort)(sample * 0x0b03 + 1));
                randoms.Enqueue(first);
                randoms.Enqueue(second);
                torizo.Position = torizo.Position.WithWhole(new(x, y));
                enemies.BombTorizoBehavior.Attacks.SpawnBombTorizoDrool(torizo, RoomEnemyProjectileKind.BombTorizoInitialDrool);
                EnemyProjectile drool = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive);

                int lifted = Add((first & 3) + y, 0xfffb, 0, out int liftCarry);
                int fall = (first & 0x1f) + 0x30 + liftCarry;
                int jitteredX = Add(second & 3, x, 0, out int sideCarry);
                int side = posture switch
                {
                    0x4000 => jitteredX,
                    0x8000 => Add(jitteredX, 0x0008, sideCarry, out _),
                    _ => Add(jitteredX, 0xfff8, sideCarry, out _),
                };
                if (drool.Position.Y.Whole != lifted || drool.Velocity.Y.RawValue != fall || drool.Position.X.Whole != side ||
                    drool.Velocity.X.RawValue != 0)
                {
                    throw new InvalidOperationException(
                        $"Drool {name} from ({x}, {y}) with {first:X4}/{second:X4}: ({drool.Position.X.Whole}, {drool.Position.Y.Whole}) " +
                        $"falling {drool.Velocity.Y.RawValue}, not ({side}, {lifted}) falling {fall}.");
                }
                drool.Delete();
                spawns++;
            }
        }
        AssertEqual(0, randoms.Count, "each drool draws two random values");
        Console.WriteLine($"  Bomb Torizo initial drool: {spawns:N0} production spawns match the original's chained additions.");
    }

    /// <summary>A sixteen-bit addition with a carry in, reporting its carry out.</summary>
    private static int Add(int word, int operand, int carryIn, out int carryOut)
    {
        int sum = word + operand + carryIn;
        carryOut = sum > ushort.MaxValue ? 1 : 0;
        return sum & ushort.MaxValue;
    }
}
