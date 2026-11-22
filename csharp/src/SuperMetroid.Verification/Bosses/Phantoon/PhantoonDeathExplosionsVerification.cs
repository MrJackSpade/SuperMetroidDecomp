using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;

namespace SuperMetroid.Verification.Bosses.Phantoon;

/// <summary>Checks Phantoon's death explosion schedule, positions, sounds, and full-pool behavior.</summary>
internal static class PhantoonDeathExplosionsVerification
{
    /// <summary>Compares thirteen native records and frame-exact repeated requests while forbidding schedule reads in production.</summary>
    internal static void VerifyCompiledPhantoonDeathExplosions(CartridgeImportAddressSpace rom)
    {
        for (int i = 0; i < 13; i++)
        {
            var entry = PhantoonDeathExplosions.Schedule[i];
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(0xa7da1d + i * 4)), entry.Offset.X, "Native death explosion X offset");
            AssertEqual(unchecked((sbyte)rom.ReadCartridgeByte(0xa7da1e + i * 4)), entry.Offset.Y, "Native death explosion Y offset");
            AssertEqual(DustExplosionAnimationIndices.Decode(rom.ReadCartridgeByte(0xa7da1f + i * 4)), entry.Type, "Native death explosion type");
            AssertEqual(rom.ReadCartridgeByte(0xa7da20 + i * 4), entry.Delay, "Native death explosion delay");
        }
        // Phantoon is set up as his room sets him up: each run loads his room afresh, whose population spawns his parts.
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        foreach (ushort origin in new ushort[] { 0, 128, 0xffff })
        foreach (bool full in new[] { false, true })
        {
            runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Phantoon);
            RoomEnemySystem enemies = runtime.Enemies;
            PhantoonEnemyState state = enemies.BossEncounters.Phantoon ?? throw new InvalidOperationException("Phantoon's room loads Phantoon.");
            RoomEnemyPlace body = state.Body;
            Action<RoomEnemyPlace, PhantoonEnemyState> step = enemies.PhantoonBehavior.Death.RunPhantoonDyingExplosions;
            body.Position = body.Position.WithWhole(new(origin, origin));
            state.PhaseWait.LastFor(15);
            state.Phase = PhantoonPhase.DyingExplosions;
            if (full) FamilyProjectileStarts.FillPool(enemies, RoomEnemyProjectileKind.DustOrExplosion, SpriteGraphics.SharedStandard);
            int index = 0, passes = 0, timer = 15, requests = 0;
            for (int frame = 0; passes < 3; frame++)
            {
                AssertTrue(frame < 1024, "Death schedule reaches native handoff");
                // Each frame is one game update, whose sound requests the room records.
                runtime.AudioRequests.BeginRecordedUpdate();
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
                AssertEqual(timer, state.PhaseWait.UpdatesLeft, "Exact death explosion frame timer");
                AssertEqual(index, state.Death.NextExplosion.Place, "Exact death record cursor and repeat");
                AssertEqual((ushort)passes, state.Death.ExplosionPasses, "Exact death repeat counter");
                // A due explosion takes the emptied pool's one place; a full pool leaves the schedule as it is.
                if (!full)
                    AssertEqual(due ? 1 : 0, enemies.Projectiles.EnemyProjectiles.Count(p => p.IsActive), "Exact death request frame");
                AssertEqual((passes == 3 ? PhantoonPhase.BeginFinalWavyDeath : PhantoonPhase.DyingExplosions), state.Phase, "Exact final death phase handoff");
                if (due)
                {
                    // The runtime's requests go on into its session's queues, which admit the update's sounds.
                    IReadOnlyList<SoundEffectId> queued = runtime.AudioRequests.SoundsQueued;
                    AssertEqual((SoundEffectId?)SoundEffectCommands.DecodeCartridge(SoundEffectLibrary.Enemies, type == 0x1d ? (ushort)0x24 : (ushort)0x2b),
                        queued.Count == 0 ? null : queued[^1], "Death sound choice survives pool exhaustion");
                    if (!full)
                    {
                        var explosion = enemies.Projectiles.EnemyProjectiles.Single(p => p.IsActive);
                        AssertEqual(unchecked((ushort)(origin + (sbyte)rom.ReadCartridgeByte(0xa7da1d + current * 4))), explosion.Position.X.Whole, "Real death explosion wrapped world X");
                        AssertEqual(unchecked((ushort)(origin + (sbyte)rom.ReadCartridgeByte(0xa7da1e + current * 4))), explosion.Position.Y.Whole, "Real death explosion wrapped world Y");
                        int listAddress = 0x86e42c + type * 2;
                        AssertTrue(explosion.Animation is DustExplosionProjectileAnimation { PlayingSegment: var explosionProgram } &&
                                DustExplosionProjectileNativeListAddresses.Entry(explosionProgram) == rom.ReadCartridgeWord(listAddress),
                            "Real explosion animation selection");
                    }
                }
                // The update ends with the game's audio dispatch, its sound program accepting each request it was sent by
                // the next update, so the queues hand their waiting sounds on as the running game's do.
                AudioQueues queues = runtime.AudioRequests.QueuesReached;
                _ = queues.AdvanceFrame(queues.SentSoundRequests);
            }
            AssertEqual(29, requests, "Native thirteen plus eight plus eight schedule");
        }
        Console.WriteLine("Phantoon death: 52 native bytes and six frame-exact 29-request sequences preserve timing, positions, animation and full-pool behavior with schedule reads forbidden.");
    }

}
