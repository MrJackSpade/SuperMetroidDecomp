using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyLowerNorfairRioInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyLowerNorfairRioInstructionProgramDefinitions), () => VerifyLowerNorfairRioInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    private static void VerifyLowerNorfairRioInstructionProgramDefinitions(
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

        var guard = new LowerNorfairRioInstructionReadGuard(rom);
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
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0];
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

        AssertEqual(0, guard.ObservedPresentationWords.Count,
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

    private static (
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

    private static void RunLowerNorfairRioInstructionFrames(
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

    private static ushort ReadLowerNorfairRioInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa20000 | address) |
            source.ReadByte(0xa20000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class LowerNorfairRioInstructionReadGuard(
        ISnesAddressSpace source, bool forbidPresentation = false) : ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (LowerNorfairRioInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Lower Norfair Rio mechanics byte " +
                    $"${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < LowerNorfairRioInstructionProgramDefinitionsTooling.PresentationWordCount;
                     index++)
                {
                    ushort presentation = LowerNorfairRioInstructionProgramDefinitionsTooling
                        .PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        if (forbidPresentation)
                            throw new InvalidOperationException(
                                $"Installed Lower Norfair Rio read visual selector " +
                                $"$A2:{presentation:X4}.");
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
