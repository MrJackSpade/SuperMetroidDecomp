using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static ImportedSpritemapsVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;

/// <summary>Checks Polyp-rock projectile instruction mechanics and installed frame selection against native data.</summary>
internal static class PolypRockInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail ROM and runs the production Polyp-rock program check.</summary>
    internal static void VerifyPolypRockInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyPolypRockInstructionProgramDefinitions), () => VerifyPolypRockInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks the real producer, authored frame and sleep, shared deletion, and forbidden runtime source reads.</summary>
    internal static void VerifyPolypRockInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < PolypRockInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                PolypRockInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadPolypRockInstructionWord(rom, definition.Address),
                $"Polyp-rock mechanics word $86:{definition.Address:X4}");
        }

        var guard = NewPolypRockInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(guard);

        var source = new RoomEnemySlot(0)
        {
            Position = new(new(128, 0x1234), new(112, 0x5678)),
            VramTilesIndex = 0x0200,
            PaletteIndex = 0x0c00,
        };
        enemies.PolypBehavior.SpawnPolypRock(source, (ushort)0x0010, (ushort)0xff00);
        RoomEnemyProjectileSlot rock = enemies.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.PolypRock);
        AssertEqual(PolypRockInstructionProgramDefinitions.Initial,
            rock.InstructionPointer,
            "real Polyp-rock producer selects the named single-frame program");

        RunForcedTick(rock);
        AssertEqual(PolypRockInstructionProgramDefinitions.PresentationWord,
            rock.PresentationOperandAddress,
            "Polyp rock selects its installed presentation binding without cartridge reads");
        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidOperationException("Polyp rock fixture requires installed projectile artwork.");
        Suite(nameof(VerifyExecutedProjectileFrame), () => VerifyExecutedProjectileFrame(rom, rock, spriteArtwork, new HashSet<ushort>()));
        AssertEqual(PolypRockInstructionProgramDefinitions.Sleep,
            rock.InstructionPointer,
            "Polyp rock reaches its terminal sleep after the authored frame");
        RunForcedTick(rock);

        AssertEqual(PolypRockInstructionProgramDefinitions.Sleep,
            rock.InstructionPointer,
            "Polyp rock remains at its authored terminal sleep");
        AssertEqual((ushort)0, rock.InstructionTimer.RawValue,
            "Polyp-rock sleep leaves the instruction timer stopped");

        rock.InstructionPointer = CommonEnemyProjectileInstructionProgramDefinitions.Delete;
        RunForcedTick(rock);

        AssertTrue(!rock.IsActive,
            "Polyp-rock shot reaction reaches the compiled shared delete program");

        AssertTrue(CompiledEnemyVisualSelectors.TryGet(0x86,
                PolypRockInstructionProgramDefinitions.PresentationWord, out ushort selector),
            "Polyp-rock selector is compiled");
        AssertEqual(ReadPolypRockInstructionWord(rom,
                PolypRockInstructionProgramDefinitions.PresentationWord), selector,
            "compiled Polyp-rock selector equals the native operand");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Polyp-rock and shared-delete mechanics byte");
        AssertThrows<InvalidDataException>(
            () => PolypRockInstructionProgramDefinitions.ReadMechanicsWord(
                PolypRockInstructionProgramDefinitions.PresentationWord),
            "Polyp-rock spritemap operand is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => PolypRockInstructionProgramDefinitions.ReadMechanicsWord(0xbbdb),
            "adjacent Polyp-rock initializer is rejected as mechanics");

        _ = ProbePolypRockInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbePolypRockInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Polyp-rock allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Polyp-rock mechanics lookups allocate no storage");

        Console.WriteLine(
            "Polyp-rock instruction mechanics: two compiled words, the real producer, " +
            "terminal sleep, shared shot deletion, and installed frame selection pass with " +
            "mechanics bytes forbidden.");

        void RunForcedTick(RoomEnemyProjectileSlot projectile)
        {
            projectile.InstructionTimer = NativeCountdown.Start(1);
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
        }
    }

    /// <summary>Repeatedly reads both compiled control words to expose per-frame lookup allocations.</summary>
    private static int ProbePolypRockInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += PolypRockInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? PolypRockInstructionProgramDefinitions.Initial
                    : PolypRockInstructionProgramDefinitions.Sleep);
        }
        return checksum;
    }

    /// <summary>Reads one bank-$86 instruction word for comparison with the compiled program.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Polyp rock instruction words")]
    internal static ushort ReadPolypRockInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | address);

    /// <summary>Blocks migrated Polyp-rock, shared-delete, and visual-selector operands during production execution.</summary>
    private static CartridgeReadGuard NewPolypRockInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => PolypRockInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) ||
                    CommonEnemyProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled Polyp-rock mechanics byte ${address:X6}.")
            .Forbid(address => address - (EnemyProjectileCodePointers.BankBase |
                    PolypRockInstructionProgramDefinitions.PresentationWord) is 0 or 1,
                _ => "Production read the compiled Polyp-rock visual operand.");

    /// <summary>Runs the focused Polyp-rock instruction comparison against the retail ROM.</summary>
    [VerificationCommand("--polyp-rock-instruction-mechanics")]
    internal static int PolypRockInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyPolypRockInstructionProgramDefinitions), () => VerifyPolypRockInstructionProgramDefinitions());
        return 0;
    }
}
