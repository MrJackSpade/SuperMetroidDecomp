using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledPhantoonDeathExplosions(SuperMetroidAddressSpace rom)
    {
        for (int i = 0; i < 13; i++)
        {
            var entry = PhantoonDeathExplosionDefinitions.Read(i);
            AssertEqual(unchecked((sbyte)rom.ReadByte(0xa7da1d + i * 4)), entry.X, "Native death explosion X offset");
            AssertEqual(unchecked((sbyte)rom.ReadByte(0xa7da1e + i * 4)), entry.Y, "Native death explosion Y offset");
            AssertEqual(rom.ReadByte(0xa7da1f + i * 4), entry.Type, "Native death explosion type");
            AssertEqual(rom.ReadByte(0xa7da20 + i * 4), entry.Delay, "Native death explosion delay");
        }
        foreach (ushort origin in new ushort[] { 0, 128, 0xffff })
        foreach (bool full in new[] { false, true })
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(enemies, new PhantoonDeathReadGuard(rom));
            var body = enemies.Slots[0];
            var tentacles = enemies.Slots[2];
            var state = new PhantoonEnemyState(body) { Tentacles = tentacles };
            var step = typeof(RoomEnemySystem).GetMethod("RunPhantoonDyingExplosions", BindingFlags.NonPublic | BindingFlags.Instance)!
                .CreateDelegate<Action<RoomEnemySlot, PhantoonEnemyState>>(enemies);
            body.XPosition = body.YPosition = origin;
            body.VariableE = 15;
            body.VariableF = (ushort)PhantoonAiFunction.DyingExplosions;
            if (full) foreach (var projectile in enemies.EnemyProjectiles) projectile.Kind = RoomEnemyProjectileKind.MiscDustExplosion;
            int index = 0, passes = 0, timer = 15, requests = 0;
            for (int frame = 0; passes < 3; frame++)
            {
                AssertTrue(frame < 1024, "Death schedule reaches native handoff");
                if (!full) foreach (var projectile in enemies.EnemyProjectiles) projectile.Clear();
                bool due = --timer <= 0;
                int current = index;
                byte type = rom.ReadByte(0xa7da1f + current * 4);
                if (due)
                {
                    timer = rom.ReadByte(0xa7da20 + current * 4);
                    requests++;
                    if (++index == 13) { index = 5; passes++; }
                }
                step(body, state);
                AssertEqual((ushort)timer, body.VariableE, "Exact death explosion frame timer");
                AssertEqual((ushort)index, tentacles.VariableF, "Exact death record cursor and repeat");
                AssertEqual((ushort)passes, body.VariableA, "Exact death repeat counter");
                AssertEqual(requests, state.DeathExplosionRequests, "Exact death request frame");
                AssertEqual(full ? 0 : requests, state.DeathExplosionsSpawned, "Allocation pressure does not alter schedule");
                AssertEqual((ushort)(passes == 3 ? PhantoonAiFunction.BeginFinalWavyDeath : PhantoonAiFunction.DyingExplosions), body.VariableF, "Exact final death phase handoff");
                if (due)
                {
                    AssertEqual((ushort?)(type == 0x1d ? 0x24 : 0x2b), state.LastCombatSoundEffect, "Death sound choice survives pool exhaustion");
                    if (!full)
                    {
                        var explosion = enemies.EnemyProjectiles.Single(p => p.IsActive);
                        AssertEqual(unchecked((ushort)(origin + (sbyte)rom.ReadByte(0xa7da1d + current * 4))), explosion.XPosition, "Real death explosion wrapped world X");
                        AssertEqual(unchecked((ushort)(origin + (sbyte)rom.ReadByte(0xa7da1e + current * 4))), explosion.YPosition, "Real death explosion wrapped world Y");
                        int listAddress = 0x86e42c + type * 2;
                        AssertEqual((ushort)(rom.ReadByte(listAddress) | rom.ReadByte(listAddress + 1) << 8), explosion.InstructionPointer, "Real explosion animation selection");
                    }
                }
            }
            AssertEqual(29, requests, "Native thirteen plus eight plus eight schedule");
        }
        Console.WriteLine("Phantoon death: 52 native bytes and six frame-exact 29-request sequences preserve timing, positions, animation and full-pool behavior with schedule reads forbidden.");
    }

    private sealed class PhantoonDeathReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa7da1d and < 0xa7da51
            ? throw new InvalidOperationException("Unexpected migrated death schedule read.") : source.ReadByte(address);
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected death schedule bus write.");
    }
}
