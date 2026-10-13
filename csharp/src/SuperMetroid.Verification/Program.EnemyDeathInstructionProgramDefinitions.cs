using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static DoorDefinitionsVerification;
using static EnemyProjectileProgramOracleVerification;
using static ImportedSpritemapsVerification;
using static SuiteTimingVerification;

/// <summary>Checks compiled generic enemy death and pickup instruction programs against ROM and live death producers.</summary>
internal static class EnemyDeathInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail ROM and runs the generic enemy-death instruction suite.</summary>
    internal static void VerifyEnemyDeathInstructionProgramDefinitions() =>
        Suite(nameof(VerifyEnemyDeathInstructionProgramDefinitions), () => VerifyEnemyDeathInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Checks native mechanics words and all five death variants through their shared respawn/delete tail.</summary>
    internal static void VerifyEnemyDeathInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        foreach (ushort address in new[]
                 {
                     EnemyDeathInstructionProgramDefinitions.RespawnTail,
                     unchecked((ushort)(EnemyDeathInstructionProgramDefinitions.RespawnTail + 4)),
                     unchecked((ushort)(EnemyDeathInstructionProgramDefinitions.RespawnTail + 6)),
                 })
            AssertTrue(EnemyDeathInstructionProgramDefinitions.Owns(
                    RoomEnemyProjectileKind.EnemyDeathPickup, address),
                $"enemy pickup owns native respawn-tail mechanics $86:{address:X4}");
        AssertTrue(!EnemyDeathInstructionProgramDefinitions.Owns(
                RoomEnemyProjectileKind.EnemyDeathPickup,
                EnemyDeathInstructionProgramDefinitions.BigExplosion),
            "enemy pickup does not own the following death-explosion program");
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < EnemyDeathInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                EnemyDeathInstructionProgramDefinitionsTooling.MechanicsWord(index);
            ushort native = ReadNativeEnemyProjectileProgramWord(rom, definition.Address);
            AssertEqual(definition.Value, native,
                $"generic enemy-death mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NewEnemyDeathInstructionReadGuard(rom);
        (ushort Variant, ushort Initial, int CallsUntilTail, ushort Sound)[] programs =
        [
            (0, EnemyDeathInstructionProgramDefinitions.SmallExplosion, 7, 0x0009),
            (1, EnemyDeathInstructionProgramDefinitions.KilledBySamusContact, 17, 0x000b),
            (2, EnemyDeathInstructionProgramDefinitions.NormalExplosion, 7, 0x0009),
            (3, EnemyDeathInstructionProgramDefinitions.MiniKraidExplosion, 17, 0x0024),
            (4, EnemyDeathInstructionProgramDefinitions.BigExplosion, 6, 0x0024),
        ];

        foreach (var program in programs)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0x1234);
            enemies.DeathsAndDrops._samusForEnemyDrops = new SamusState { Health = 99, MaxHealth = 99 };

            // A real header resolves the pickup's drop chances from ROM; a cleared header
            // ($0000) would make native read them from enemy WRAM, which this guard lacks.
            var enemy = new RoomEnemySlot(0)
            {
                EnemyDefinitionPointer = EnemyDefinitionId.HZoomer,
                Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0070)),
            };
            enemies.DeathsAndDrops.StartGenericEnemyDeath(enemy, program.Variant);
            RoomEnemyProjectileSlot death = enemies.Projectiles.EnemyProjectiles.Single(
                projectile => projectile.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion);
            AssertEqual(program.Initial, death.InstructionPointer,
                $"real death producer selects variant {program.Variant} named program");

            for (int call = 1; call <= program.CallsUntilTail; call++)
            {
                death.InstructionTimer = NativeCountdown.Start(1);
                Process(enemies, death);
                AssertTrue(death.IsActive,
                    $"death variant {program.Variant} remains active through call {call}");
                if (call < program.CallsUntilTail)
                {
                    AssertTrue(death.InstructionPointer !=
                        EnemyDeathInstructionProgramDefinitions.RespawnTail + 4,
                        $"death variant {program.Variant} does not convert early at call {call}");
                }
            }

            AssertEqual(
                unchecked((ushort)(EnemyDeathInstructionProgramDefinitions.RespawnTail + 4)),
                death.InstructionPointer,
                $"death variant {program.Variant} converts to the shared blank tail exactly");
            AssertEqual(program.Sound, enemies.DeathsAndDrops.LastEnemyDeathSoundEffectLibrary2!.Value,
                $"death variant {program.Variant} queues its authored sound");

            if (program.Variant is 3 or 4)
            {
                AssertTrue(enemies.RoomSpriteObjects.Any(sprite => sprite.IsActive),
                    $"death variant {program.Variant} executes its random sprite callbacks");
            }

            death.InstructionTimer = NativeCountdown.Start(1);
            Process(enemies, death);
            AssertTrue(!death.IsActive,
                $"death variant {program.Variant} executes the compiled respawn/delete tail");
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "EnemyDeath execution performs no live spritemap operand reads");
        AssertEqual(EnemyDeathInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "EnemyDeath executes every native visual operand");
        for (int index = 0; index < EnemyDeathInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = EnemyDeathInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"EnemyDeath executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "EnemyDeath has a compiled visual selector");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                "EnemyDeath compiled selector matches the cartridge");
        }
        AssertEqual(0, guard.ObservedWords.Count, "generic enemy-death presentation needs no live cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids all compiled generic enemy-death mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => EnemyDeathInstructionProgramDefinitions.ReadMechanicsWord(0xed4d),
            "generic enemy-death spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => EnemyDeathInstructionProgramDefinitions.ReadMechanicsWord(0xed87),
            "unused enemy-death list is not silently catalogued");

        _ = ProbeEnemyDeathInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeEnemyDeathInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "generic enemy-death allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed generic enemy-death mechanics lookups allocate no storage");

        Console.WriteLine(
            "Generic enemy-death instruction mechanics: all five real death variants, " +
            "the shared respawn tail, 66 compiled words, and 31 installed sprite frames match native OAM " +
            "with mechanics bytes forbidden.");

        void Process(RoomEnemySystem system, RoomEnemyProjectileSlot projectile)
        {
            system.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
            VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
        }
    }

    /// <summary>Consumes repeated compiled death-program lookups for the warmed allocation check.</summary>
    private static int ProbeEnemyDeathInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += EnemyDeathInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? EnemyDeathInstructionProgramDefinitions.SmallExplosion
                    : EnemyDeathInstructionProgramDefinitions.BigExplosion);
        }
        return checksum;
    }

    /// <summary>Rejects reads from compiled mechanics bytes and observes separate visual selector addresses.</summary>
    private static CartridgeReadGuard NewEnemyDeathInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(EnemyDeathInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled generic enemy-death mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, EnemyDeathInstructionProgramDefinitions.PresentationWordCount,
                EnemyDeathInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the generic death instruction suite through the verification command dispatcher.</summary>
    [VerificationCommand("--enemy-death-instruction-mechanics")]
    internal static int EnemyDeathInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyEnemyDeathInstructionProgramDefinitions), () => VerifyEnemyDeathInstructionProgramDefinitions());
        return 0;
    }
}
