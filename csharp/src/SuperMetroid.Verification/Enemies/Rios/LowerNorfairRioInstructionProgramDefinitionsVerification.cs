using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Rios;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled Lower Norfair Rio parent and flame programs against cartridge words and live instruction execution.</summary>
internal static class LowerNorfairRioInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge and runs Lower Norfair Rio instruction verification.</summary>
    internal static void VerifyLowerNorfairRioInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyLowerNorfairRioInstructionProgramDefinitions), () => VerifyLowerNorfairRioInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks seven instruction programs, initializer selections, and compiled visual operands without runtime mechanics reads.</summary>
    internal static void VerifyLowerNorfairRioInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        AssertEqual(51, LowerNorfairRioInstructionProgramDefinitionsTooling.MechanicsWordCount,
            "Lower Norfair Rio compiled mechanics word count");
        AssertEqual(32, LowerNorfairRioInstructionProgramDefinitionsTooling.PresentationWordCount,
            "Lower Norfair Rio live presentation word count");
        for (int index = 0;
             index < LowerNorfairRioInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                LowerNorfairRioInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadLowerNorfairRioInstructionWord(rom, definition.Address),
                $"Lower Norfair Rio mechanics word $A2:{definition.Address:X4}");
        }

        var guard = NewLowerNorfairRioInstructionReadGuard(rom);
        (ushort Entry, int Calls, ushort Final, bool Visible, bool Signal, string Name)[]
            programs =
        [
            ((ushort)LowerNorfairRioProgram.Idle,
                5,
                unchecked((ushort)((ushort)LowerNorfairRioProgram.Idle + 6)),
                false, false, "idle"),
            ((ushort)LowerNorfairRioProgram.PrepareToSwoop,
                10,
                unchecked((ushort)(
                    (ushort)LowerNorfairRioProgram.PrepareToSwoop + 40)),
                false, true, "prepare to swoop"),
            ((ushort)LowerNorfairRioProgram.Descending,
                2,
                unchecked((ushort)(
                    (ushort)LowerNorfairRioProgram.Descending + 6)),
                false, false, "descending"),
            ((ushort)LowerNorfairRioProgram.AscendingPart1,
                4,
                unchecked((ushort)(
                    (ushort)LowerNorfairRioProgram.AscendingPart1 + 16)),
                false, true, "ascending part one"),
            ((ushort)LowerNorfairRioProgram.AscendingPart2,
                4,
                unchecked((ushort)(
                    (ushort)LowerNorfairRioProgram.AscendingPart2 + 6)),
                true, false, "ascending part two"),
            ((ushort)LowerNorfairRioProgram.Cooldown,
                10,
                unchecked((ushort)(
                    (ushort)LowerNorfairRioProgram.Cooldown + 40)),
                true, true, "cooldown"),
            ((ushort)LowerNorfairRioProgram.Flames,
                4,
                unchecked((ushort)(
                    (ushort)LowerNorfairRioProgram.Flames + 4)),
                false, false, "flames"),
        ];
        foreach ((ushort entry, int calls, ushort final, bool visible, bool signal,
                  string name) in programs)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, LowerNorfairRioEnemyState state) =
                NewLowerNorfairRioInstructionSystem(guard, entry);
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            RunLowerNorfairRioInstructionFrames(
                enemies,
                arguments,
                slot,
                calls);
            AssertEqual(final, slot.CurrentInstruction,
                $"Lower Norfair Rio {name} completes its program");
            AssertEqual(visible, state.FollowerVisible,
                $"Lower Norfair Rio {name} publishes follower visibility");
            AssertEqual(signal, state.AnimationSignal,
                $"Lower Norfair Rio {name} animation signal");
        }

        Suite(nameof(VerifyLowerNorfairRioInitializerSelections), () => VerifyLowerNorfairRioInitializerSelections(guard));

        AssertEqual(0, guard.ObservedWords.Count,
            "Lower Norfair Rio presentation selectors are compiled, not ROM reads");
        for (int index = 0;
             index < LowerNorfairRioInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address =
                LowerNorfairRioInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertEqual(ReadLowerNorfairRioInstructionWord(rom, address),
                EnemySpritemapDefinitions.LowerNorfairRioFrameAt(address),
                $"compiled Lower Norfair Rio selector $A2:{address:X4} matches ROM");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Lower Norfair Rio mechanics byte");

        AssertThrows<InvalidDataException>(
            () => LowerNorfairRioInstructionProgramDefinitions.ReadMechanicsWord(
                LowerNorfairRioInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
            "Lower Norfair Rio spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => LowerNorfairRioInstructionProgramDefinitions.ReadMechanicsWord(
                LowerNorfairRioInstructionProgramDefinitions.AdjacentMovementDefinitions),
            "adjacent Lower Norfair Rio movement definitions are rejected as mechanics");

        _ = ProbeLowerNorfairRioInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeLowerNorfairRioInstructionMechanicsAllocation();
        AssertTrue(checksum != 0,
            "Lower Norfair Rio allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Lower Norfair Rio mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Lower Norfair Rio instruction mechanics: 51 compiled words, all seven " +
            "parent/flame programs, three callbacks, and 32 compiled spritemap selectors pass.");
    }

    /// <summary>Confirms that the parent starts idle while its follower starts the flame program.</summary>
    private static void VerifyLowerNorfairRioInitializerSelections(
        ISnesAddressSpace bus)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(bus);

        RoomEnemySlot parent = enemies.SlotTable.Slots[0];
        parent.EnemyDefinitionPointer = EnemyDefinitionId.Holtz;
        parent.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        enemies.LowerNorfairRioBehavior.InitializeLowerNorfairRio(parent);
        AssertEqual((ushort)LowerNorfairRioProgram.Idle,
            parent.CurrentInstruction,
            "Lower Norfair Rio parent initializer selects idle");

        RoomEnemySlot follower = enemies.SlotTable.Slots[1];
        follower.EnemyDefinitionPointer = EnemyDefinitionId.Holtz;
        follower.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        follower.Parameter1 = 0x8000;
        enemies.LowerNorfairRioBehavior.InitializeLowerNorfairRio(follower);
        AssertEqual((ushort)LowerNorfairRioProgram.Flames,
            follower.CurrentInstruction,
            "Lower Norfair Rio follower initializer selects flames");
    }

    /// <summary>Creates a guarded Rio instruction fixture with its selected entry installed in the first enemy slot.</summary>
    internal static (
        RoomEnemySystem Enemies,
        RoomEnemySlot Slot,
        LowerNorfairRioEnemyState State) NewLowerNorfairRioInstructionSystem(
            ISnesAddressSpace bus,
            ushort entry,
            EnemyTileArtworkCatalog? art = null)
    {
        var enemies = new RoomEnemySystem { TileArtwork = art };
        enemies.BindFixtureBus(bus);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Holtz;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        slot.CurrentInstruction = entry;
        slot.InstructionTimer = NativeCountdown.Start(1);
        var state = new LowerNorfairRioEnemyState(slot);
        var states = (LowerNorfairRioEnemyState?[])enemies.LowerNorfairRioBehavior._lowerNorfairRioStates;
        states[0] = state;
        return (enemies, slot, state);
    }

    /// <summary>Forces the instruction timer and advances the requested number of interpreter calls.</summary>
    internal static void RunLowerNorfairRioInstructionFrames(
        RoomEnemySystem enemies,
        object?[] arguments,
        RoomEnemySlot slot,
        int count)
    {
        for (int call = 0; call < count; call++)
        {
            slot.InstructionTimer = NativeCountdown.Start(1);
            EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    /// <summary>Alternates compiled idle and cooldown lookups to detect warmed per-frame allocations.</summary>
    private static int ProbeLowerNorfairRioInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += LowerNorfairRioInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? (ushort)LowerNorfairRioProgram.Idle
                    : (ushort)LowerNorfairRioProgram.Cooldown);
        }
        return checksum;
    }

    /// <summary>Reads one native bank-$A2 word solely as the retail comparison oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Lower Norfair Rio instruction words")]
    internal static ushort ReadLowerNorfairRioInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(0xa20000 | address);

    /// <summary>Rejects compiled mechanics reads and optionally rejects presentation-selector reads as well.</summary>
    internal static CartridgeReadGuard NewLowerNorfairRioInstructionReadGuard(
        ISnesAddressSpace source, bool forbidPresentation = false)
    {
        static ushort SelectorWord(int address) => Enumerable.Range(0, LowerNorfairRioInstructionProgramDefinitionsTooling.PresentationWordCount)
            .Select(LowerNorfairRioInstructionProgramDefinitionsTooling.PresentationWordAddress)
            .First(word => unchecked((ushort)address) == word || unchecked((ushort)address) == unchecked((ushort)(word + 1)));
        CartridgeReadGuard guard = new CartridgeReadGuard(source)
            .Forbid(LowerNorfairRioInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Lower Norfair Rio mechanics byte ${address:X6}.");
        return forbidPresentation
            ? guard.ForbidWords(0xa20000, LowerNorfairRioInstructionProgramDefinitionsTooling.PresentationWordCount, LowerNorfairRioInstructionProgramDefinitionsTooling.PresentationWordAddress,
                address => $"Installed Lower Norfair Rio read visual selector $A2:{SelectorWord(address):X4}.")
            : guard.ObserveWords(0xa20000, LowerNorfairRioInstructionProgramDefinitionsTooling.PresentationWordCount, LowerNorfairRioInstructionProgramDefinitionsTooling.PresentationWordAddress);
    }

    /// <summary>Runs the standalone Lower Norfair Rio instruction verification command.</summary>
    [VerificationCommand("--lower-norfair-rio-instruction-program-definitions")]
    internal static int LowerNorfairRioInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyLowerNorfairRioInstructionProgramDefinitions), () => VerifyLowerNorfairRioInstructionProgramDefinitions());
        return 0;
    }
}
