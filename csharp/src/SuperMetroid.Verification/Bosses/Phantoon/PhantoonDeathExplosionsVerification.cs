using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Bosses.Phantoon;

/// <summary>Checks Phantoon's death explosion schedule, positions, sounds, and full-pool behavior.</summary>
internal static class PhantoonDeathExplosionsVerification
{
    /// <summary>Compares thirteen native records and frame-exact repeated requests while forbidding schedule reads in production.</summary>
    internal static void VerifyCompiledPhantoonDeathExplosions(CartridgeImportAddressSpace rom)
    {
        for (int i = 0; i < 13; i++)
        {
            var entry = PhantoonDeathExplosions.Read(i);
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(0xa7da1d + i * 4)), entry.Offset.X, "Native death explosion X offset");
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(0xa7da1e + i * 4)), entry.Offset.Y, "Native death explosion Y offset");
            AssertEqual((MiscDustAnimation)rom.ReadCartridgeByte(0xa7da1f + i * 4), entry.Type, "Native death explosion type");
            AssertEqual(rom.ReadCartridgeByte(0xa7da20 + i * 4), entry.Delay, "Native death explosion delay");
        }
        foreach (ushort origin in new ushort[] { 0, 128, 0xffff })
        foreach (bool full in new[] { false, true })
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            var body = enemies.SlotTable.Slots[0];
            var tentacles = enemies.SlotTable.Slots[2];
            var state = new PhantoonEnemyState(body) { Tentacles = tentacles };
            enemies.PhantoonBehavior.Combat.Phantoon = state;
            Action<RoomEnemySlot, PhantoonEnemyState> step = enemies.PhantoonBehavior.Death.RunPhantoonDyingExplosions;
            body.Position = body.Position.WithWhole(new(origin, origin));
            state.BodyState.PhaseTimer = 15;
            state.BodyState.Phase = PhantoonPhase.DyingExplosions;
            if (full) foreach (var projectile in enemies.Projectiles.EnemyProjectiles) projectile.Kind = RoomEnemyProjectileKind.MiscDustExplosion;
            int index = 0, passes = 0, timer = 15, requests = 0;
            for (int frame = 0; passes < 3; frame++)
            {
                AssertTrue(frame < 1024, "Death schedule reaches native handoff");
                if (!full) foreach (var projectile in enemies.Projectiles.EnemyProjectiles) projectile.Delete();
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
                AssertEqual((ushort)timer, state.BodyState.PhaseTimer, "Exact death explosion frame timer");
                AssertEqual((ushort)index, state.TentaclesState.ExplosionIndex, "Exact death record cursor and repeat");
                AssertEqual((ushort)passes, state.BodyState.ExplosionPass, "Exact death repeat counter");
                // A due explosion takes the emptied pool's one place; a full pool leaves the schedule as it is.
                if (!full)
                    AssertEqual(due ? 1 : 0, enemies.Projectiles.EnemyProjectiles.Count(p => p.IsActive), "Exact death request frame");
                AssertEqual((passes == 3 ? PhantoonPhase.BeginFinalWavyDeath : PhantoonPhase.DyingExplosions), state.BodyState.Phase, "Exact final death phase handoff");
                if (due)
                {
                    AssertEqual((SoundEffectId?)SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, type == 0x1d ? (ushort)0x24 : (ushort)0x2b), enemies.Audio.LatestSound, "Death sound choice survives pool exhaustion");
                    if (!full)
                    {
                        var explosion = enemies.Projectiles.EnemyProjectiles.Single(p => p.IsActive);
                        AssertEqual(unchecked((ushort)(origin + (sbyte)rom.ReadCartridgeByte(0xa7da1d + current * 4))), explosion.Position.X.Whole, "Real death explosion wrapped world X");
                        AssertEqual(unchecked((ushort)(origin + (sbyte)rom.ReadCartridgeByte(0xa7da1e + current * 4))), explosion.Position.Y.Whole, "Real death explosion wrapped world Y");
                        int listAddress = 0x86e42c + type * 2;
                        AssertTrue(explosion.Animation is MiscDustProjectileAnimation { PlayingSegment: var explosionProgram } &&
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
