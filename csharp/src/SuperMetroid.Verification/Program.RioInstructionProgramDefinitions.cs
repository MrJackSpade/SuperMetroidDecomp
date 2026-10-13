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
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

/// <summary>Checks Rio idle, swoop, and cooldown instruction programs against bank-$A2 data and live animation handoffs.</summary>
internal static class RioInstructionProgramDefinitionsVerification
{
    /// <summary>Loads retail cartridge data for Rio instruction verification.</summary>
    internal static void VerifyRioInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyRioInstructionProgramDefinitions), () => VerifyRioInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares mechanics and visual words with ROM, then runs the initializer and all four animation programs with compiled mechanics reads guarded.</summary>
    internal static void VerifyRioInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < RioInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                RioInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadRioInstructionWord(rom, definition.Address),
                $"Rio mechanics word $A2:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = NewRioInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        enemies.BindFixtureBus(guard);
        var initialize = new Action<RoomEnemySlot>(enemies.RioBehavior.InitializeRio);

        RoomEnemySlot rio = enemies.SlotTable.Slots[0];
        rio.EnemyDefinitionPointer = EnemyDefinitionId.Rio;
        rio.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        initialize(rio);
        AssertEqual(RioInstructionProgramDefinitions.Idle,
            rio.CurrentInstruction,
            "real Rio initializer installs compiled idle program");

        object?[] processArguments =
            [rio, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        ExecuteRioProgram(
            enemies,
            processArguments,
            rio,
            RioInstructionProgramDefinitions.Idle,
            callCount: 13);
        AssertEqual(unchecked((ushort)(RioInstructionProgramDefinitions.Idle + 4)),
            rio.CurrentInstruction,
            "idle fallthrough and post-swoop program return to first repeated idle frame");

        RioEnemyState state = enemies.RioStates[0]!;
        state.AnimationFinished = false;
        ExecuteRioProgram(
            enemies,
            processArguments,
            rio,
            RioInstructionProgramDefinitions.SwoopingPart1,
            callCount: 6);
        AssertEqual(unchecked((ushort)(RioInstructionProgramDefinitions.SwoopingPart1 + 22)),
            rio.CurrentInstruction,
            "first swoop program reaches terminal sleep");
        AssertTrue(state.AnimationFinished,
            "first swoop program publishes animation completion");

        ExecuteRioProgram(
            enemies,
            processArguments,
            rio,
            RioInstructionProgramDefinitions.SwoopingPart2,
            callCount: 3);
        AssertEqual(unchecked((ushort)(RioInstructionProgramDefinitions.SwoopingPart2 + 4)),
            rio.CurrentInstruction,
            "second swoop program loops and installs its first repeated frame");

        state.AnimationFinished = false;
        ExecuteRioProgram(
            enemies,
            processArguments,
            rio,
            RioInstructionProgramDefinitions.SwoopCooldown,
            callCount: 6);
        AssertEqual(unchecked((ushort)(RioInstructionProgramDefinitions.SwoopCooldown + 22)),
            rio.CurrentInstruction,
            "swoop cooldown reaches terminal sleep");
        AssertTrue(state.AnimationFinished,
            "swoop cooldown publishes animation completion");

        AssertEqual(0, guard.ObservedWords.Count,
            "Rio presentation selectors are compiled, not ROM reads");
        for (int index = 0;
             index < RioInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = RioInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertEqual(ReadRioInstructionWord(rom, address),
                EnemySpritemapDefinitions.RioFrameAt(address),
                $"compiled Rio selector $A2:{address:X4} matches ROM");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Rio mechanics byte");
        AssertThrows<InvalidDataException>(
            () => RioInstructionProgramDefinitions.ReadMechanicsWord(
                RioInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Rio spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => RioInstructionProgramDefinitions.ReadMechanicsWord(
                RioInstructionProgramDefinitions.FirstAdjacentMechanicsData),
            "adjacent Rio launch data is rejected as instruction mechanics");

        _ = ProbeRioInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeRioInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Rio allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Rio mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Rio instruction mechanics: thirty-two compiled words, the real initializer, " +
            "idle fallthrough/return, both swoop phases, cooldown completion, and twenty-four " +
            "compiled spritemap selectors pass with mechanics bytes forbidden.");
    }

    /// <summary>Starts a Rio instruction entry and forces the requested number of production interpreter calls.</summary>
    internal static void ExecuteRioProgram(
        RoomEnemySystem enemies,
        object?[] processArguments,
        RoomEnemySlot rio,
        ushort program,
        int callCount)
    {
        rio.CurrentInstruction = program;
        for (int call = 0; call < callCount; call++)
        {
            rio.InstructionTimer = 1;
            EnemyInstructionRunner.Process(enemies, processArguments);
        }
    }

    /// <summary>Alternates idle and cooldown mechanics lookups to check the warmed path allocates no memory.</summary>
    private static int ProbeRioInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += RioInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? RioInstructionProgramDefinitions.Idle
                    : unchecked((ushort)(RioInstructionProgramDefinitions.SwoopCooldown + 22)));
        }
        return checksum;
    }

    /// <summary>Reads a native bank-$A2 instruction word for exact mechanics and selector comparisons.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Rio instruction words")]
    internal static ushort ReadRioInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(0xa20000 | address);

    /// <summary>Forbids migrated mechanics reads and optionally forbids, otherwise observes, visual-selector reads.</summary>
    internal static CartridgeReadGuard NewRioInstructionReadGuard(
        ISnesAddressSpace source, bool forbidPresentation = false)
    {
        static ushort SelectorWord(int address) => Enumerable.Range(0, RioInstructionProgramDefinitions.PresentationWordCount)
            .Select(RioInstructionProgramDefinitions.PresentationWordAddress)
            .First(word => unchecked((ushort)address) == word || unchecked((ushort)address) == unchecked((ushort)(word + 1)));
        CartridgeReadGuard guard = new CartridgeReadGuard(source)
            .Forbid(RioInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Rio mechanics byte ${address:X6}.");
        return forbidPresentation
            ? guard.ForbidWords(0xa20000, RioInstructionProgramDefinitions.PresentationWordCount, RioInstructionProgramDefinitions.PresentationWordAddress,
                address => $"Installed Rio read visual selector $A2:{SelectorWord(address):X4}.")
            : guard.ObserveWords(0xa20000, RioInstructionProgramDefinitions.PresentationWordCount, RioInstructionProgramDefinitions.PresentationWordAddress);
    }

    /// <summary>Runs the Rio instruction mechanics verification command.</summary>
    [VerificationCommand("--rio-instruction-mechanics")]
    internal static int RioInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyRioInstructionProgramDefinitions), () => VerifyRioInstructionProgramDefinitions());
        return 0;
    }
}
