using SuperMetroid.Verification.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Bosses.Phantoon;

/// <summary>Checks Phantoon's death explosion schedule, positions, sounds, and full-pool behavior.</summary>
internal static class PhantoonDeathExplosionsVerification
{
    /// <summary>Compares thirteen native records and frame-exact repeated requests while forbidding schedule reads in production.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Phantoon death explosions")]
    internal static void VerifyCompiledPhantoonDeathExplosions(SuperMetroidAddressSpace rom)
    {
        for (int i = 0; i < 13; i++)
        {
            var entry = PhantoonDeathExplosionDefinitions.Read(i);
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(0xa7da1d + i * 4)), entry.X, "Native death explosion X offset");
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(0xa7da1e + i * 4)), entry.Y, "Native death explosion Y offset");
            AssertEqual((MiscDustAnimation)rom.ReadCartridgeByte(0xa7da1f + i * 4), entry.Type, "Native death explosion type");
            AssertEqual(rom.ReadCartridgeByte(0xa7da20 + i * 4), entry.Delay, "Native death explosion delay");
        }
        foreach (ushort origin in new ushort[] { 0, 128, 0xffff })
        foreach (bool full in new[] { false, true })
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            var body = enemies.SlotTable.Slots[0];
            var tentacles = enemies.SlotTable.Slots[2];
            var state = new PhantoonEnemyState(body) { Tentacles = tentacles };
            Action<RoomEnemySlot, PhantoonEnemyState> step = enemies.PhantoonBehavior.RunPhantoonDyingExplosions;
            body.Position = body.Position.WithWhole(new(origin, origin));
            body.VariableE = 15;
            body.VariableF = (ushort)PhantoonAiFunction.DyingExplosions;
            if (full) foreach (var projectile in enemies.Projectiles.EnemyProjectiles) projectile.Kind = RoomEnemyProjectileKind.MiscDustExplosion;
            int index = 0, passes = 0, timer = 15, requests = 0;
            for (int frame = 0; passes < 3; frame++)
            {
                AssertTrue(frame < 1024, "Death schedule reaches native handoff");
                if (!full) foreach (var projectile in enemies.Projectiles.EnemyProjectiles) projectile.Clear();
                bool due = --timer <= 0;
                int current = index;
                byte type = rom.ReadCartridgeByte(0xa7da1f + current * 4);
                if (due)
                {
                    timer = rom.ReadCartridgeByte(0xa7da20 + current * 4);
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
                    AssertEqual((SoundEffectId?)SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, type == 0x1d ? (ushort)0x24 : (ushort)0x2b), state.LastCombatSoundEffect, "Death sound choice survives pool exhaustion");
                    if (!full)
                    {
                        var explosion = enemies.Projectiles.EnemyProjectiles.Single(p => p.IsActive);
                        AssertEqual(unchecked((ushort)(origin + (sbyte)rom.ReadCartridgeByte(0xa7da1d + current * 4))), explosion.Position.X.Whole, "Real death explosion wrapped world X");
                        AssertEqual(unchecked((ushort)(origin + (sbyte)rom.ReadCartridgeByte(0xa7da1e + current * 4))), explosion.Position.Y.Whole, "Real death explosion wrapped world Y");
                        int listAddress = 0x86e42c + type * 2;
                        AssertTrue(explosion.Animation is MiscDustProjectileAnimation { Cursor.Program: var explosionProgram } &&
                                MiscDustProjectileNativeListAddresses.Entry(explosionProgram) == CartridgeImportSource.Require(rom).ReadCartridgeWord(listAddress),
                            "Real explosion animation selection");
                    }
                }
            }
            AssertEqual(29, requests, "Native thirteen plus eight plus eight schedule");
        }
        Console.WriteLine("Phantoon death: 52 native bytes and six frame-exact 29-request sequences preserve timing, positions, animation and full-pool behavior with schedule reads forbidden.");
    }

}
