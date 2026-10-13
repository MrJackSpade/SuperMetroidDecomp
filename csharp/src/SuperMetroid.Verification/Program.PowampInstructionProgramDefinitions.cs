using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static ImportedSpritemapsVerification;
using static SuiteTimingVerification;

internal static class PowampInstructionProgramDefinitionsVerification
{
    /// <summary>Loads retail data and runs the Powamp instruction program suite.</summary>
    internal static void VerifyPowampInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyPowampInstructionProgramDefinitions), () => VerifyPowampInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks compiled words, body and balloon programs, all visual operands, and forbidden source reads.</summary>
    internal static void VerifyPowampInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < PowampInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                PowampInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa80000 | definition.Address),
                $"Powamp mechanics word $A8:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = NewPowampInstructionReadGuard(rom);
        var executedOperands = new HashSet<ushort>();
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        enemies.BindFixtureBus(guard);
        var initialize = new Action<RoomEnemySlot>(enemies.PowampBehavior.InitializePowamp);

        RoomEnemySlot balloon = enemies.SlotTable.Slots[0];
        balloon.EnemyDefinitionPointer = EnemyDefinitionId.Powamp;
        balloon.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        balloon.Parameter1 = 1;
        initialize(balloon);
        AssertEqual(PowampInstructionProgramDefinitions.BalloonDeflated,
            balloon.CurrentInstruction,
            "real Powamp balloon initializer installs compiled deflated program");

        RoomEnemySlot body = enemies.SlotTable.Slots[1];
        body.EnemyDefinitionPointer = EnemyDefinitionId.Powamp;
        body.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        initialize(body);
        AssertEqual(PowampInstructionProgramDefinitions.BodySlow,
            body.CurrentInstruction,
            "real Powamp body initializer installs compiled slow program");

        object?[] bodyArguments =
            [body, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        ExecutePowampInstructionCalls(rom, executedOperands, enemies, bodyArguments, body, 4);
        AssertEqual(unchecked((ushort)(PowampInstructionProgramDefinitions.BodySlow + 4)),
            body.CurrentInstruction,
            "Powamp slow body loop completes its native goto and first repeated frame");

        body.CurrentInstruction = PowampInstructionProgramDefinitions.BodyFast;
        ExecutePowampInstructionCalls(rom, executedOperands, enemies, bodyArguments, body, 4);
        AssertEqual(unchecked((ushort)(PowampInstructionProgramDefinitions.BodyFast + 4)),
            body.CurrentInstruction,
            "Powamp fast body loop completes its native goto and first repeated frame");

        object?[] balloonArguments =
            [balloon, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        balloon.CurrentInstruction = PowampInstructionProgramDefinitions.BalloonInflate0;
        ExecutePowampInstructionCalls(rom, executedOperands, enemies, balloonArguments, balloon, 4);
        AssertEqual(unchecked((ushort)(PowampInstructionProgramDefinitions.BalloonInflate2 + 4)),
            balloon.CurrentInstruction,
            "Powamp inflate program reaches terminal sleep");

        balloon.CurrentInstruction = PowampInstructionProgramDefinitions.BalloonStartSinking;
        ExecutePowampInstructionCalls(rom, executedOperands, enemies, balloonArguments, balloon, 4);
        AssertEqual(unchecked((ushort)(PowampInstructionProgramDefinitions.BalloonDeflated + 4)),
            balloon.CurrentInstruction,
            "Powamp deflate program falls through to shared deflated sleep");

        AssertEqual(0, guard.ObservedWords.Count,
            "Enemy presentation performs zero live cartridge reads");
        AssertEqual(PowampInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "Every native visual operand executes");
        for (int index = 0; index < PowampInstructionProgramDefinitions.PresentationWordCount; index++)
            AssertTrue(executedOperands.Contains(PowampInstructionProgramDefinitions.PresentationWordAddress(index)),
                "every Powamp visual operand executes");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Powamp mechanics byte");
        AssertThrows<InvalidDataException>(
            () => PowampInstructionProgramDefinitions.ReadMechanicsWord(
                PowampInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Powamp spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => PowampInstructionProgramDefinitions.ReadMechanicsWord(
                PowampInstructionProgramDefinitions.FirstAdjacentConstant),
            "adjacent Powamp constants are rejected as instruction mechanics");

        _ = ProbePowampInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbePowampInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Powamp allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Powamp mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Powamp instruction mechanics: 18 compiled words, both body loops, the " +
            "inflate/deflate fallthrough programs, and 12 native sprite selections pass " +
            "with mechanics bytes forbidden.");
    }

    /// <summary>Runs a bounded Powamp instruction segment and checks each emitted visual selector.</summary>
    private static void ExecutePowampInstructionCalls(
        ISnesAddressSpace rom, HashSet<ushort> executedOperands,
        RoomEnemySystem enemies,
        object?[] arguments,
        RoomEnemySlot slot,
        int calls)
    {
        for (int call = 0; call < calls; call++)
        {
            slot.InstructionTimer = 1;
            EnemyInstructionRunner.Process(enemies, arguments);
            VerifyExecutedEnemySelector(rom, slot, executedOperands);
        }
    }

    /// <summary>Alternates body and balloon word lookups to measure warmed allocation.</summary>
    private static int ProbePowampInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += PowampInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? PowampInstructionProgramDefinitions.BodyFast
                    : PowampInstructionProgramDefinitions.BalloonDeflatedSleep);
        }
        return checksum;
    }

    /// <summary>Forbids compiled Powamp mechanics bytes and observes remaining presentation reads.</summary>
    internal static CartridgeReadGuard NewPowampInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(PowampInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Powamp mechanics byte ${address:X6}.")
            .ObserveWords(0xa80000, PowampInstructionProgramDefinitions.PresentationWordCount,
                PowampInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the Powamp instruction mechanics verification command.</summary>
    [VerificationCommand("--powamp-instruction-mechanics")]
    internal static int PowampInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyPowampInstructionProgramDefinitions), () => VerifyPowampInstructionProgramDefinitions());
        return 0;
    }
}
