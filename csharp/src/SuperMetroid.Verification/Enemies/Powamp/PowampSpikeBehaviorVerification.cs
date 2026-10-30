using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Enemies.Powamp;

/// <summary>#1291: Powamp's spikes run their typed loop and private delete program as their native lists did.</summary>
internal static class PowampSpikeBehaviorVerification
{
    /// <summary>
    /// A burst spawns all eight spikes on the looping program, each matching the native loop update for update; the
    /// collision handoff starts the private delete program, which deletes the spike on the same update as the native list.
    /// </summary>
    internal static void VerifyPowampSpikeBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, PowampSpikeNativeListAddresses.Extent, "Powamp-spike");
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var body = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112)),
            TileIndexBase = 512,
            Colouring = SharedSpriteColouring.Frozen,
        };
        enemies.PowampBehavior.SpawnPowampSpikeBurst(body);
        RoomEnemyProjectileSlot[] spikes = [.. enemies.Projectiles.EnemyProjectiles.Where(projectile => projectile.Kind == RoomEnemyProjectileKind.PowampSpike)];
        AssertEqual(8, spikes.Length, "a burst spawns all eight spikes");
        foreach (RoomEnemyProjectileSlot spike in spikes)
        {
            AssertEqual(PowampSpikeProgram.Initial, ProgramOf(spike), $"spike {spike.StateOf<PowampSpikeState>().Direction} starts its looping program");
            (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, spike, Entry(PowampSpikeProgram.Initial), 40,
                $"Powamp spike {spike.StateOf<PowampSpikeState>().Direction}");
            AssertTrue(!deleted, "a spike's loop runs until collision deletes it");
        }

        PowampBehavior.BeginPowampSpikeDeletion(spikes[0]);
        AssertEqual(PowampSpikeProgram.Delete, ProgramOf(spikes[0]), "the collision handoff starts the private delete program");
        (bool removed, int updates) = NativeEnemyProjectileParity.Compare(cartridge, enemies, spikes[0], Entry(PowampSpikeProgram.Delete), 4, "Powamp spike deletion");
        AssertTrue(removed, "the private delete program deletes the spike");
        AssertEqual(1, updates, "the spike is deleted on its next instruction pass");
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the spike's native list bytes");
        Console.WriteLine("  Powamp spike behavior: all eight burst spikes loop and the private delete program deletes as natively.");
    }

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    private static ushort Entry(PowampSpikeProgram program) =>
        NativeEnemyProjectileProgramOracle.EntryOf(PowampSpikeNativeListAddresses.Entries, program);

    /// <summary>The typed program <paramref name="projectile"/> runs.</summary>
    private static PowampSpikeProgram ProgramOf(RoomEnemyProjectileSlot projectile) =>
        projectile.Animation is PowampSpikeAnimation animation ? animation.Cursor.Program
            : throw new InvalidOperationException("The spike runs no typed program.");

    /// <summary>Runs the Powamp spike's program oracle and behavior suite.</summary>
    [VerificationCommand("--powamp-spike-programs")]
    internal static int PowampSpikeProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(PowampSpikeAnimationsVerification.VerifyPowampSpikeAnimations), () => PowampSpikeAnimationsVerification.VerifyPowampSpikeAnimations(rom));
        Suite(nameof(VerifyPowampSpikeBehavior), () => VerifyPowampSpikeBehavior(rom));
        return 0;
    }
}
