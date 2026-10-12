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

internal static class LowerNorfairRioInstructionProgramDefinitionsVerification
{
    internal static void VerifyLowerNorfairRioInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyLowerNorfairRioInstructionProgramDefinitions), () => VerifyLowerNorfairRioInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    internal static void VerifyLowerNorfairRioInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
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
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
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
                NewLowerNorfairRioInstructionSystem(guard, flags, entry);
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            RunLowerNorfairRioInstructionFrames(
                process,
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

        Suite(nameof(VerifyLowerNorfairRioInitializerSelections), () => VerifyLowerNorfairRioInitializerSelections(guard, flags));

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

    private static void VerifyLowerNorfairRioInitializerSelections(
        ISnesAddressSpace bus,
        BindingFlags flags)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeLowerNorfairRio",
            flags)!;

        RoomEnemySlot parent = enemies.Slots[0];
        parent.EnemyDefinitionPointer = EnemyDefinitionId.Holtz;
        parent.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        initialize.Invoke(enemies, [parent]);
        AssertEqual((ushort)LowerNorfairRioProgram.Idle,
            parent.CurrentInstruction,
            "Lower Norfair Rio parent initializer selects idle");

        RoomEnemySlot follower = enemies.Slots[1];
        follower.EnemyDefinitionPointer = EnemyDefinitionId.Holtz;
        follower.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        follower.Parameter1 = 0x8000;
        initialize.Invoke(enemies, [follower]);
        AssertEqual((ushort)LowerNorfairRioProgram.Flames,
            follower.CurrentInstruction,
            "Lower Norfair Rio follower initializer selects flames");
    }

    internal static (
        RoomEnemySystem Enemies,
        RoomEnemySlot Slot,
        LowerNorfairRioEnemyState State) NewLowerNorfairRioInstructionSystem(
            ISnesAddressSpace bus,
            BindingFlags flags,
            ushort entry,
            EnemyTileArtworkCatalog? art = null)
    {
        var enemies = new RoomEnemySystem { TileArtwork = art };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Holtz;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        slot.CurrentInstruction = entry;
        slot.InstructionTimer = 1;
        var state = new LowerNorfairRioEnemyState(slot);
        var states = (LowerNorfairRioEnemyState?[])typeof(RoomEnemySystem)
            .GetField("_lowerNorfairRioStates", flags)!
            .GetValue(enemies)!;
        states[0] = state;
        return (enemies, slot, state);
    }

    internal static void RunLowerNorfairRioInstructionFrames(
        MethodInfo process,
        RoomEnemySystem enemies,
        object?[] arguments,
        RoomEnemySlot slot,
        int count)
    {
        for (int call = 0; call < count; call++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
    }

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

    [CpuBusBoundary("exact bus test: retail ROM oracle for Lower Norfair Rio instruction words")]
    internal static ushort ReadLowerNorfairRioInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(0xa20000 | address);

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

    [VerificationCommand("--lower-norfair-rio-instruction-program-definitions")]
    internal static int LowerNorfairRioInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyLowerNorfairRioInstructionProgramDefinitions), () => VerifyLowerNorfairRioInstructionProgramDefinitions());
        return 0;
    }
}
