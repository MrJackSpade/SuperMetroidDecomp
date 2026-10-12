using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static DoorDefinitionsVerification;
using static ImportedSpritemapsVerification;
using static SuiteTimingVerification;

internal static class SporeSpawnProjectileInstructionProgramDefinitionsVerification
{
    internal static void VerifySporeSpawnProjectileInstructionProgramDefinitions() =>
        Suite(nameof(VerifySporeSpawnProjectileInstructionProgramDefinitions), () => VerifySporeSpawnProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    [CpuBusBoundary("exact bus test: retail ROM oracle for Spore Spawn projectile instruction program definitions")]
    internal static void VerifySporeSpawnProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < SporeSpawnProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                SporeSpawnProjectileInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address);
            AssertEqual(definition.Value, native,
                $"Spore Spawn projectile mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = RepositoryInstallation.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NewSporeSpawnProjectileInstructionReadGuard(rom);
        var selectedPresentation = new HashSet<ushort>();
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 1);
        var samus = new SamusState();
        typeof(RoomEnemySystem).GetField("_samusForEnemyDrops", flags)!.SetValue(
            enemies,
            samus);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessEnemyProjectileInstructions", flags)!;
        var spawnSpawner = typeof(RoomEnemySystem).GetMethod(
            "SpawnSporeSpawnSpawner", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ushort>>(enemies);
        var spawnStalk = typeof(RoomEnemySystem).GetMethod(
            "SpawnSporeSpawnStalk", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ushort>>(enemies);

        RoomEnemySlot body = enemies.Slots[0];
        body.Position = body.Position.WithWhole(new(128, 256));
        spawnSpawner(body, 0);
        RoomEnemyProjectileSlot spawner = Single(RoomEnemyProjectileKind.SporeSpawnSpawner);
        Run(spawner, 2, [0xdc02]);
        AssertEqual((ushort)0xdc04, spawner.InstructionPointer,
            "closed ceiling emitter reaches terminal sleep");
        AssertEqual((ushort)0, spawner.InstructionTimer,
            "closed ceiling emitter remains dormant at sleep");
        spawner.InstructionPointer = SporeSpawnProjectileInstructionProgramDefinitions.SpawnerRelease;
        Run(spawner, 6, [0xdc08, 0xdc0c, 0xdc12, 0xdc16, 0xdc1a]);
        AssertEqual((ushort)0xdc1c, spawner.InstructionPointer,
            "ceiling emitter completes its release and reaches sleep");

        RoomEnemyProjectileSlot spore = Single(RoomEnemyProjectileKind.SporeSpawnSpore);
        Run(spore, 4, [0xdc20, 0xdc24, 0xdc28, 0xdc20]);
        AssertEqual((ushort)0xdc22, spore.InstructionPointer,
            "airborne spore completes and restarts its three-pose loop");

        spawnStalk(body, 0);
        RoomEnemyProjectileSlot stalk = Single(RoomEnemyProjectileKind.SporeSpawnStalk);
        Run(stalk, 2, [0xdc30]);
        AssertEqual((ushort)0xdc32, stalk.InstructionPointer,
            "stalk reaches terminal sleep after its display frame");

        spore.InstructionPointer = SporeSpawnProjectileInstructionProgramDefinitions.SporeShot;
        spore.InstructionTimer = 1;
        Run(spore, 8, [0xdc36, 0xdc3c, 0xdc40, 0xdc44, 0xdc4a, 0xdc4e, 0xdc52]);
        AssertTrue(!spore.IsActive,
            "shot spore completes its seven-frame explosion/drop path and deletes");
        AssertEqual(1, enemies.SporeSpawnDropRequests.Count,
            "shot spore publishes exactly one drop request");
        AssertEqual((ushort)0, spore.Damage,
            "shot spore property callback clears damage before deletion");

        AssertEqual(0, guard.ObservedWords.Count,
            "SporeSpawnProjectile execution performs no live spritemap operand reads");
        AssertEqual(SporeSpawnProjectileInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "SporeSpawnProjectile executes every native visual operand");
        for (int index = 0; index < SporeSpawnProjectileInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = SporeSpawnProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"SporeSpawnProjectile executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "SporeSpawnProjectile has a compiled visual selector");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                "SporeSpawnProjectile compiled selector matches the cartridge");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Spore Spawn projectile mechanics byte");
        AssertThrows<InvalidDataException>(
            () => SporeSpawnProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xdc02),
            "Spore Spawn spritemap is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => SporeSpawnProjectileInstructionProgramDefinitions.ReadMechanicsWord(0xdc5a),
            "Spore Spawn callback body is rejected as mechanics");

        _ = ProbeSporeSpawnProjectileInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeSporeSpawnProjectileInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Spore Spawn allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Spore Spawn projectile mechanics lookups allocate no storage");

        Console.WriteLine(
            "Spore Spawn projectile instruction mechanics: twenty-eight compiled words, " +
            "all three real producers, complete release/loop/shot paths, and seventeen " +
            "installed sprite frames match native OAM with zero live operand reads.");

        RoomEnemyProjectileSlot Single(RoomEnemyProjectileKind kind) =>
            enemies.EnemyProjectiles.First(projectile => projectile.Kind == kind);

        void Run(RoomEnemyProjectileSlot projectile, int frames, ReadOnlySpan<ushort> nativeOperands)
        {
            int draw = 0;
            for (int frame = 0; frame < frames; frame++)
            {
                projectile.InstructionTimer = 1;
                process.Invoke(enemies, [projectile, samus, Position16<RoomPixels>.Zero]);
                VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
                if (projectile.IsActive && projectile.InstructionTimer != 0)
                {
                    AssertTrue(draw < nativeOperands.Length, "Spore Spawn has no extra draw steps");
                    AssertEqual(nativeOperands[draw++], projectile.PresentationOperandAddress,
                        "Spore Spawn actual installed operand matches native program step");
                    selectedPresentation.Add(projectile.PresentationOperandAddress);
                }
            }
            AssertEqual(nativeOperands.Length, draw, "Spore Spawn executes every expected draw step");
        }
    }
    private static int ProbeSporeSpawnProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += SporeSpawnProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? SporeSpawnProjectileInstructionProgramDefinitions.SpawnerClosed
                    : SporeSpawnProjectileInstructionProgramDefinitions.SporeShot);
        }
        return checksum;
    }

    private static CartridgeReadGuard NewSporeSpawnProjectileInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(SporeSpawnProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Spore Spawn mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, SporeSpawnProjectileInstructionProgramDefinitions.PresentationWordCount, SporeSpawnProjectileInstructionProgramDefinitions.PresentationWordAddress);

    [VerificationCommand("--spore-spawn-projectile-instruction-mechanics")]
    internal static int SporeSpawnProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifySporeSpawnProjectileInstructionProgramDefinitions), () => VerifySporeSpawnProjectileInstructionProgramDefinitions());
        return 0;
    }
}
