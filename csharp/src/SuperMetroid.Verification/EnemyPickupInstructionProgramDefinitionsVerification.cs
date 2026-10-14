using System.Reflection;
using SuperMetroid.Core.Hardware;
using static DoorDefinitionsVerification;
using static EnemyProjectileProgramOracleVerification;
using static ImportedSpritemapsVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;

/// <summary>Checks five live enemy-drop pickup programs and their installed sprite selectors against bank-$86 ROM data.</summary>
internal static class EnemyPickupInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge and starts the enemy-pickup instruction suite.</summary>
    internal static void VerifyEnemyPickupInstructionProgramDefinitions() =>
        Suite(nameof(VerifyEnemyPickupInstructionProgramDefinitions), () => VerifyEnemyPickupInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Compares mechanics words, executes each pickup loop and converted death actor, and guards against runtime ROM reads.</summary>
    internal static void VerifyEnemyPickupInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < EnemyPickupInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                EnemyPickupInstructionProgramDefinitionsTooling.MechanicsWord(index);
            ushort native = ReadNativeEnemyProjectileProgramWord(rom, definition.Address);
            AssertEqual(definition.Value, native,
                $"enemy-pickup mechanics word $86:{definition.Address:X4}");
        }

        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NewEnemyPickupInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);

        (EnemyPickupKind Kind, ushort Initial, int FrameCount, ushort Branch, ushort Sleep)[]
            programs =
            [
                (EnemyPickupKind.SmallEnergy,
                    EnemyPickupInstructionProgramDefinitions.SmallEnergy,
                    4, 0xed9d, 0xeda1),
                (EnemyPickupKind.BigEnergy,
                    EnemyPickupInstructionProgramDefinitions.BigEnergy,
                    4, 0xedb3, 0xedb7),
                (EnemyPickupKind.Missile,
                    EnemyPickupInstructionProgramDefinitions.Missiles,
                    2, 0xedc1, 0xedc5),
                (EnemyPickupKind.SuperMissile,
                    EnemyPickupInstructionProgramDefinitions.SuperMissiles,
                    2, 0xede5, 0xede9),
                (EnemyPickupKind.PowerBomb,
                    EnemyPickupInstructionProgramDefinitions.PowerBombs,
                    4, 0xedfb, 0),
            ];

        foreach (var program in programs)
        {
            var pickup = new RoomEnemyProjectileSlot(1)
            {
                Kind = RoomEnemyProjectileKind.EnemyDeathPickup,
            };
            EnemyDeathsAndDrops.BeginEnemyPickup(pickup, program.Kind);
            AssertEqual(program.Initial, pickup.InstructionPointer,
                $"real {program.Kind} pickup initializer selects its named program");

            for (int frame = 0; frame < program.FrameCount; frame++)
            {
                pickup.InstructionTimer = NativeCountdown.Start(1);
                Process(pickup);
                AssertTrue(pickup.IsActive,
                    $"{program.Kind} remains active through animation frame {frame + 1}");
            }

            AssertEqual(program.Branch, pickup.InstructionPointer,
                $"{program.Kind} reaches its authored loop branch");
            pickup.InstructionTimer = NativeCountdown.Start(1);
            Process(pickup);
            AssertEqual(unchecked((ushort)(program.Initial + 4)), pickup.InstructionPointer,
                $"{program.Kind} branch displays its first frame in the same tick");

            if (program.Sleep != 0)
            {
                pickup.InstructionPointer = program.Sleep;
                pickup.InstructionTimer = NativeCountdown.Start(1);
                Process(pickup);
                AssertEqual(program.Sleep, pickup.InstructionPointer,
                    $"{program.Kind} terminal sleep retains its own opcode");
                AssertEqual((ushort)0, pickup.InstructionTimer.RawValue,
                    $"{program.Kind} terminal sleep leaves the timer dormant");
            }
        }

        var convertedDeath = new RoomEnemyProjectileSlot(2)
        {
            Kind = RoomEnemyProjectileKind.EnemyDeathExplosion,
        };
        EnemyDeathsAndDrops.BeginEnemyPickup(convertedDeath, EnemyPickupKind.SmallEnergy);
        convertedDeath.InstructionTimer = NativeCountdown.Start(1);
        Process(convertedDeath);
        AssertEqual(
            unchecked((ushort)(EnemyPickupInstructionProgramDefinitions.SmallEnergy + 4)),
            convertedDeath.InstructionPointer,
            "death actor converted in place uses the same compiled pickup program");

        AssertEqual(0, guard.ObservedWords.Count,
            "EnemyPickup execution performs no live spritemap operand reads");
        AssertEqual(EnemyPickupInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "EnemyPickup executes every native visual operand");
        for (int index = 0; index < EnemyPickupInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = EnemyPickupInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"EnemyPickup executes native presentation operand {address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86, address, out ushort selector),
                "EnemyPickup has a compiled visual selector");
            AssertEqual(ReadVerificationWord(rom, 0x860000 | address), selector,
                "EnemyPickup compiled selector matches the cartridge");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids all compiled enemy-pickup mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => EnemyPickupInstructionProgramDefinitions.ReadMechanicsWord(0xed8f),
            "enemy-pickup spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => EnemyPickupInstructionProgramDefinitions.ReadMechanicsWord(0xedc7),
            "unused bomb-pickup program is not silently catalogued");

        _ = ProbeEnemyPickupInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeEnemyPickupInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "enemy-pickup allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed enemy-pickup mechanics lookups allocate no storage");

        Console.WriteLine(
            "Enemy-pickup instruction mechanics: thirty compiled words, all five live " +
            "programs, direct and converted-death owners, and sixteen installed sprite frames " +
            "match native OAM with zero live operand reads.");

        void Process(RoomEnemyProjectileSlot projectile)
        {
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
            VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands);
        }
    }

    /// <summary>Exercises the compiled small-energy and power-bomb lookups repeatedly to detect warmed allocations.</summary>
    private static int ProbeEnemyPickupInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += EnemyPickupInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? EnemyPickupInstructionProgramDefinitions.SmallEnergy
                    : EnemyPickupInstructionProgramDefinitions.PowerBombs);
        }
        return checksum;
    }

    /// <summary>Forbids compiled pickup mechanics reads and observes reads of migrated presentation operands.</summary>
    private static CartridgeReadGuard NewEnemyPickupInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(EnemyPickupInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled enemy-pickup mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, EnemyPickupInstructionProgramDefinitions.PresentationWordCount,
                EnemyPickupInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the enemy pickup instruction mechanics verification command.</summary>
    [VerificationCommand("--enemy-pickup-instruction-mechanics")]
    internal static int EnemyPickupInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyEnemyPickupInstructionProgramDefinitions), () => VerifyEnemyPickupInstructionProgramDefinitions());
        return 0;
    }
}
