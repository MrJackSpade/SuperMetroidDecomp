using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Phantoon;
using SuperMetroid.Verification.Harness;

/// <summary>Retail-word and guarded-execution checks for Phantoon's projectile flame programs.</summary>
internal static class PhantoonProjectileInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail source for the Phantoon flame instruction check.</summary>
    internal static void VerifyPhantoonProjectileInstructionProgramDefinitions() =>
        Suite(nameof(VerifyPhantoonProjectileInstructionProgramDefinitions), () => VerifyPhantoonProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Executes starting, falling, impact, orbit, and shot flames through loops and deletion with compiled reads forbidden.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Phantoon projectile instruction program definitions")]
    internal static void VerifyPhantoonProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < PhantoonProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                PhantoonProjectileInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value, CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address),
                $"Phantoon projectile mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NewPhantoonProjectileInstructionReadGuard(rom);

        RoomEnemySystem startingSystem = NewSystem();
        RoomEnemySlot startingOwner = startingSystem.SlotTable.Slots[0];
        startingOwner.Position = startingOwner.Position.WithWhole(new(128, 96));
        startingSystem.PhantoonBehavior.SpawnPhantoonStartingFlame(startingOwner, (byte)0);
        RoomEnemyProjectileSlot starting = startingSystem.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.PhantoonStartingFlame);
        AssertEqual(PhantoonProjectileInstructionProgramDefinitions.StartingFlame,
            starting.InstructionPointer, "real Phantoon starting-flame producer");
        RunForcedTicks(startingSystem, starting, 4);
        AssertEqual((ushort)0x97ec, starting.InstructionPointer,
            "Phantoon starting flame completes three frames and loops");

        RoomEnemySystem fallingSystem = NewSystem();
        RoomEnemyProjectileSlot falling = SpawnDestroyable(fallingSystem, 0);
        AssertEqual(PhantoonProjectileInstructionProgramDefinitions.CasualFalling,
            falling.InstructionPointer, "real Phantoon casual-flame producer");
        RunForcedTicks(fallingSystem, falling, 25);
        AssertEqual((ushort)0x97bc, falling.InstructionPointer,
            "Phantoon casual flame completes all three blink phases and loops");

        RoomEnemySystem idleSystem = NewSystem();
        RoomEnemyProjectileSlot idle = SpawnDestroyable(idleSystem, 0x0200);
        AssertEqual(PhantoonProjectileInstructionProgramDefinitions.DestroyableIdle,
            idle.InstructionPointer, "real Phantoon rage-flame producer");
        RunForcedTicks(idleSystem, idle, 4);
        AssertEqual((ushort)0x9760, idle.InstructionPointer,
            "Phantoon idle flame completes three frames and loops");

        RoomEnemySystem hitGroundSystem = NewSystem();
        RoomEnemyProjectileSlot hitGround = NewDestroyable(
            hitGroundSystem, PhantoonProjectileInstructionProgramDefinitions.CasualHitGround);
        RunForcedTicks(hitGroundSystem, hitGround, 2);
        AssertEqual((ushort)0x9770, hitGround.InstructionPointer,
            "Phantoon ground-impact pose reaches terminal sleep");
        AssertEqual((ushort)0, hitGround.InstructionTimer.RawValue,
            "Phantoon terminal sleep preserves the native zero timer");

        RoomEnemySystem bouncingSystem = NewSystem();
        RoomEnemyProjectileSlot bouncing = NewDestroyable(
            bouncingSystem, PhantoonProjectileInstructionProgramDefinitions.CasualBouncing);
        RunForcedTicks(bouncingSystem, bouncing, 4);
        AssertEqual((ushort)0x9776, bouncing.InstructionPointer,
            "Phantoon bouncing flame completes three frames and loops");

        RoomEnemySystem restingSystem = NewSystem();
        RoomEnemyProjectileSlot resting = NewDestroyable(
            restingSystem, PhantoonProjectileInstructionProgramDefinitions.CasualResting);
        RunForcedTicks(restingSystem, resting, 11);
        AssertTrue(!resting.IsActive,
            "Phantoon resting flame falls through resting and dying frames to delete");

        RoomEnemySystem rainSystem = NewSystem();
        RoomEnemyProjectileSlot rain = NewDestroyable(
            rainSystem, PhantoonProjectileInstructionProgramDefinitions.RainImpact);
        RunForcedTicks(rainSystem, rain, 6);
        AssertTrue(!rain.IsActive,
            "Phantoon rain impact enters the dying program and deletes");

        RoomEnemySystem deleteSystem = NewSystem();
        RoomEnemyProjectileSlot deleting = NewDestroyable(
            deleteSystem, PhantoonProjectileInstructionProgramDefinitions.Delete);
        RunForcedTicks(deleteSystem, deleting, 1);
        AssertTrue(!deleting.IsActive, "Phantoon orbit expiry reaches private deletion");

        RoomEnemySystem shotSystem = NewSystem();
        RoomEnemyProjectileSlot shot = NewDestroyable(
            shotSystem, PhantoonProjectileInstructionProgramDefinitions.DestroyableShot);
        RunForcedTicks(shotSystem, shot, 5);
        AssertTrue(!shot.IsActive,
            "Phantoon shot flame completes four frames, requests a drop, and deletes");
        AssertEqual(1, shotSystem.PhantoonFlameDropRequests.Count,
            "Phantoon shot callback publishes exactly one drop request");

        AssertEqual(0, guard.ObservedWords.Count,
            "PhantoonProjectile execution performs no live spritemap operand reads");
        AssertEqual(PhantoonProjectileInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "PhantoonProjectile executes every native visual operand");
        for (int index = 0; index < PhantoonProjectileInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = PhantoonProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"PhantoonProjectile executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "PhantoonProjectile has a compiled visual selector");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | address), selector,
                "PhantoonProjectile compiled selector matches the cartridge");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids Phantoon mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => PhantoonProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x975e),
            "Phantoon spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => PhantoonProjectileInstructionProgramDefinitions.ReadMechanicsWord(0x980e),
            "Phantoon drop callback body is rejected as mechanics");

        _ = ProbePhantoonProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbePhantoonProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Phantoon allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Phantoon mechanics lookups allocate no storage");

        Console.WriteLine(
            "Phantoon projectile instruction mechanics: fifty-eight compiled words, " +
            "both real producers, every flame program, callbacks, and thirty-one installed " +
            "sprite frames match native OAM without live operand reads.");

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0x0080);
            enemies.DeathsAndDrops._samusForEnemyDrops = new SamusState { Health = 99, MaxHealth = 99 };
            return enemies;
        }

        RoomEnemyProjectileSlot SpawnDestroyable(RoomEnemySystem enemies, ushort parameter)
        {
            RoomEnemySlot body = enemies.SlotTable.Slots[0];
            body.Position = body.Position.WithWhole(new(128, 96));
            // Native parameter word: spawn type in the high byte, index in the low byte.
            enemies.PhantoonBehavior.SpawnPhantoonDestroyableFlame(body, (PhantoonFlameSpawnType)(SnesLittleEndian.HighByte(parameter)), (byte)parameter);
            return enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind ==
                RoomEnemyProjectileKind.PhantoonDestroyableFlame);
        }

        RoomEnemyProjectileSlot NewDestroyable(RoomEnemySystem enemies, ushort program)
        {
            RoomEnemyProjectileSlot projectile = SpawnDestroyable(enemies, 0x0200);
            projectile.InstructionPointer = program;
            projectile.InstructionTimer = NativeCountdown.Start(1);
            return projectile;
        }

        void RunForcedTicks(
            RoomEnemySystem enemies,
            RoomEnemyProjectileSlot projectile,
            int count)
        {
            for (int tick = 0; tick < count; tick++)
            {
                projectile.InstructionTimer = NativeCountdown.Start(1);
                enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
                VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
            }
        }
    }

    /// <summary>Alternates warmed idle and shot control lookups for allocation accounting.</summary>
    private static int ProbePhantoonProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += PhantoonProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? PhantoonProjectileInstructionProgramDefinitions.DestroyableIdle
                    : PhantoonProjectileInstructionProgramDefinitions.DestroyableShot);
        }
        return checksum;
    }

    /// <summary>Rejects migrated Phantoon control reads and observes visual operands used during execution.</summary>
    private static CartridgeReadGuard NewPhantoonProjectileInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(PhantoonProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Phantoon mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, PhantoonProjectileInstructionProgramDefinitions.PresentationWordCount,
                PhantoonProjectileInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the retail-backed Phantoon flame instruction migration command.</summary>
    [VerificationCommand("--phantoon-projectile-instruction-mechanics")]
    internal static int PhantoonProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyPhantoonProjectileInstructionProgramDefinitions), () => VerifyPhantoonProjectileInstructionProgramDefinitions());
        return 0;
    }
}
