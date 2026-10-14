using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Ridley's compiled instruction families and extended-sprite selectors for both facings.</summary>
internal static class RidleyInstructionProgramDefinitionsVerification
{
    /// <summary>
    /// Proves that every production Ridley mechanics word matches the pinned cartridge and
    /// that both facing paths execute while those cartridge bytes are inaccessible.
    /// </summary>
    internal static void VerifyRidleyInstructionProgramDefinitions()
    {
        const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.NonPublic;
        EnemyDefinitionId ceresRidleyDefinition = EnemyDefinitionId.RidleyCeres;
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        for (int index = 0;
             index < RidleyInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                RidleyInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa60000 | definition.Address)),
                definition.Value,
                $"Ridley mechanics word $A6:{definition.Address:X4}");
        }

        var executedOperands = new HashSet<ushort>();
        var guard = NewRidleyInstructionReadGuard(rom);
        ushort[] programs =
        [
            RidleyInstructionProgramDefinitions.Initial,
            RidleyInstructionProgramDefinitions.CeresLunge,
            RidleyInstructionProgramDefinitions.RetrieveBabyMetroid,
            RidleyInstructionProgramDefinitions.OpeningRoar,
            RidleyInstructionProgramDefinitions.DeathRoar,
            RidleyInstructionProgramDefinitions.TurnFromLeftToRight,
            RidleyInstructionProgramDefinitions.TurnFromRightToLeft,
            RidleyInstructionProgramDefinitions.Fireballing,
        ];
        foreach (ushort program in programs)
        {
            RunProgram(rom, executedOperands, guard, program, facingDirection: RidleyFacing.Left, ceresRidleyDefinition);
            RunProgram(rom, executedOperands, guard, program, facingDirection: RidleyFacing.Right, ceresRidleyDefinition);
        }

        RunProgram(rom, executedOperands, guard,
            RidleyInstructionProgramDefinitions.TransitionToFlying,
            facingDirection: RidleyFacing.Left,
            ceresRidleyDefinition);
        RunProgram(rom, executedOperands, guard,
            RidleyInstructionProgramDefinitions.TransitionToFlying,
            facingDirection: RidleyFacing.Right,
            EnemyDefinitionId.Ridley);

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Ridley execution avoids compiled mechanics bytes");
        AssertEqual(0,
            guard.ObservedWords.Count,
            "Ridley extended-spritemap selection performs zero live cartridge reads");
        AssertEqual(RidleyInstructionProgramDefinitionsTooling.PresentationWordCount, executedOperands.Count,
            "Ridley programs execute every native visual operand");
        for (int index = 0;
             index < RidleyInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address =
                RidleyInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"production execution selects Ridley presentation $A6:{address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa6, address, out ushort selector),
                "Ridley presentation has a compiled selector");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa60000 | address)), selector,
                "Ridley compiled selector matches the cartridge");
        }

        AssertThrows<InvalidDataException>(
            () => RidleyInstructionProgramDefinitions.ReadMechanicsWord(0xe53e),
            "Ridley extended-spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => RidleyInstructionProgramDefinitions.ReadMechanicsWord(0xe828),
            "unused Ridley projectile code is outside the compiled program family");

        _ = ProbeRidleyInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeRidleyInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Ridley allocation probe consumes live mechanics data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Ridley mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            $"Ridley instruction mechanics: " +
            $"{RidleyInstructionProgramDefinitionsTooling.MechanicsWordCount} compiled words, " +
            "nine production entry programs, both facing paths, and " +
            $"{RidleyInstructionProgramDefinitionsTooling.PresentationWordCount} native " +
            "extended-sprite selections pass with zero live operand reads.");
    }

    /// <summary>Runs one Ridley entry to sleep while checking every selected native sprite operand.</summary>
    private static void RunProgram(
        SuperMetroidAddressSpace rom,
        HashSet<ushort> executedOperands,
        CartridgeReadGuard guard,
        ushort program,
        RidleyFacing facingDirection,
        EnemyDefinitionId enemyDefinition)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem
        {
            TileArtwork = InstalledFixtures.EnemyTiles,
            Presentation = { CeresRidleyColors = RetailPresentationFixture().CeresRidleyColors },
        };
        enemies.BindFixtureBus(guard);
        enemies.Presentation._cgram = new SnesCgram();

        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        const BindingFlags staticFlags = BindingFlags.Static | BindingFlags.NonPublic;
        slot.EnemyDefinitionPointer = EnemyDefinitionId.RidleyCeres;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
        enemies.CeresRidleyBehavior.InitializeCeresRidley(slot);

        slot.EnemyDefinitionPointer = enemyDefinition;
        slot.CurrentInstruction = program;
        slot.InstructionTimer = NativeCountdown.Start(1);
        (enemies.RidleyEncounter.Ridley ?? throw new InvalidOperationException("Verification requires enemies.RidleyEncounter.Ridley.")).FacingDirection = facingDirection;
        var samus = new SamusState { Health = 99 };
        object?[] arguments =
            [slot, samus, null, Position16<RoomPixels>.Zero, (ushort)0];

        for (int frame = 0; frame < 1000; frame++)
        {
            EnemyInstructionRunner.Process(enemies, arguments);
            if (!slot.InstructionTimer.IsZero)
            {
                ushort operand = unchecked((ushort)(slot.CurrentInstruction - 2));
                executedOperands.Add(operand);
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa60000 | operand)), slot.SpritemapPointer,
                    $"executed Ridley frame $A6:{operand:X4} matches the native extended sprite selector");
            }
            if (slot.InstructionTimer.IsZero &&
                RidleyInstructionProgramDefinitions.ReadMechanicsWord(
                    slot.CurrentInstruction) == (ushort)CommonEnemyInstruction.Sleep)
            {
                return;
            }
        }

        throw new InvalidDataException(
            $"Ridley program $A6:{program:X4}, facing {facingDirection}, did not sleep.");
    }

    /// <summary>Repeatedly reads the fireballing entry to detect allocation on warmed lookups.</summary>
    private static int ProbeRidleyInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += RidleyInstructionProgramDefinitions.ReadMechanicsWord(
                RidleyInstructionProgramDefinitions.Fireballing);
        }
        return checksum;
    }

    /// <summary>Rejects live mechanics reads and records cartridge visual selector accesses.</summary>
    private static CartridgeReadGuard NewRidleyInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(RidleyInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Ridley mechanics ${address:X6}.")
            .ObserveWords(0xa60000, RidleyInstructionProgramDefinitionsTooling.PresentationWordCount,
                RidleyInstructionProgramDefinitionsTooling.PresentationWordAddress);

    /// <summary>Runs Ridley instruction comparison from the command dispatcher.</summary>
    [VerificationCommand("--ridley-instruction-mechanics")]
    internal static int RidleyInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyRidleyInstructionProgramDefinitions), () => VerifyRidleyInstructionProgramDefinitions());
        return 0;
    }
}
