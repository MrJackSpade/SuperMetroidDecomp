using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Verification.Enemies.Deaths;

namespace SuperMetroid.Verification.Enemies.Powamp;

/// <summary>#1291: Powamp's spikes run their typed loop and private delete program as their native lists did.</summary>
internal static class PowampSpikeBehaviorVerification
{
    /// <summary>
    /// A Powamp shot dead bursts into all eight spikes on the looping program, each matching the native loop update for
    /// update; a spike flying into a wall starts the private delete program, which deletes the spike on the same update as
    /// the native list.
    /// </summary>
    internal static void VerifyPowampSpikeBehavior(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, PowampSpikeNativeListAddresses.Extent, "Powamp-spike");
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace body = PowampPairFixtures.PlacePair(enemies).Body;
        body.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112));
        body.Shading = SuperMetroid.Core.Enemies.Drawing.EnemyShades.FrozenLook;
        PowampPairFixtures.ShootDeadAndRunDeathDelay(enemies, body);
        EnemyProjectile[] spikes = [.. enemies.Projectiles.EnemyProjectiles.Where(projectile => projectile.Kind == RoomEnemyProjectileKind.PowampSpike)];
        AssertEqual(8, spikes.Length, "a burst spawns all eight spikes");
        // Spawning searches the pool from its last place down, so the eighth spike takes the lowest of the eight places, and
        // the Powamp dies in the explosion that place selects, in the place below it.
        int lastSpikePlace = spikes.Min(spike => spike.Place);
        EnemyProjectile explosion = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion);
        AssertEqual(lastSpikePlace - 1, explosion.Place, "the Powamp's explosion takes the place below the burst's last spike");
        AssertEqual(EnemyDeathExplosions.Animation(PowampBurstExplosionVerification.ExplosionAfter(lastSpikePlace)), EnemyDeathProjectilePrograms.AnimationOf(explosion),
            "the Powamp dies in the explosion its burst's last spike's place selects");
        // Each spike flies, accelerating from still, the way it points: one update in an open room shows which.
        RoomLevelData open = RoomEnemyFrameFixtures.OpenRoom(new(16, 16));
        Dictionary<EnemyProjectile, CompassDirection> flights = spikes.ToDictionary(spike => spike, spike =>
        {
            enemies.PowampBehavior.FlyPowampSpike(spike, open);
            return PowampSpikeCartridgeOrder.Directions.Single(direction => PowampMotion.SpikeAcceleration(direction) == spike.Velocity);
        });
        // The cartridge's burst counts its direction down from the table's last, so the highest place flies up and left.
        AssertTrue(spikes.OrderByDescending(spike => spike.Place).Select(spike => flights[spike])
                .SequenceEqual(PowampSpikeCartridgeOrder.Directions.Reverse()),
            "the burst's spikes take their pool places in the cartridge's firing order");
        foreach (EnemyProjectile spike in spikes)
        {
            AssertEqual(PowampSpikeSegment.Flying, ProgramOf(spike), $"spike {flights[spike]} starts its looping program");
            (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, spike, Entry(PowampSpikeSegment.Flying), 40,
                $"Powamp spike {flights[spike]}");
            AssertTrue(!deleted, "a spike's loop runs until collision deletes it");
        }

        // In a room of solid blocks, the spike's next move runs into a wall.
        enemies.PowampBehavior.FlyPowampSpike(spikes[0], VerificationHelpersVerification.CreateSolidRowsRoom(new(16, 16), 0, 16));
        AssertEqual(PowampSpikeSegment.Delete, ProgramOf(spikes[0]), "a spike flying into a wall starts the private delete program");
        (bool removed, int updates) = NativeEnemyProjectileParity.Compare(cartridge, enemies, spikes[0], Entry(PowampSpikeSegment.Delete), 4, "Powamp spike deletion");
        AssertTrue(removed, "the private delete program deletes the spike");
        AssertEqual(1, updates, "the spike is deleted on its next instruction pass");
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the spike's native list bytes");
        Console.WriteLine("  Powamp spike behavior: all eight burst spikes loop and the private delete program deletes as natively.");
    }

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    private static ushort Entry(PowampSpikeSegment program) =>
        NativeEnemyProjectileProgramOracle.EntryOf(PowampSpikeNativeListAddresses.Entries, program);

    /// <summary>The typed program <paramref name="projectile"/> runs.</summary>
    private static PowampSpikeSegment ProgramOf(EnemyProjectile projectile) =>
        projectile.Animation is PowampSpikeAnimation animation ? animation.PlayingSegment
            : throw new InvalidOperationException("The spike runs no typed program.");

    /// <summary>Runs the Powamp spike's program oracle and behavior suite.</summary>
    [VerificationCommand("--powamp-spike-programs")]
    internal static int PowampSpikeProgramsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(PowampSpikeAnimationsVerification.VerifyPowampSpikeAnimations), () => PowampSpikeAnimationsVerification.VerifyPowampSpikeAnimations(rom));
        Suite(nameof(VerifyPowampSpikeBehavior), () => VerifyPowampSpikeBehavior(rom));
        return 0;
    }
}
