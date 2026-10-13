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

/// <summary>Checks compiled Norfair Rio parent and flame instruction programs.</summary>
internal static class NorfairRioInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for Norfair Rio instruction comparison.</summary>
    internal static void VerifyNorfairRioInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyNorfairRioInstructionProgramDefinitions), () => VerifyNorfairRioInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks native words, seven guarded programs, callbacks, visual selectors, and allocation-free reads.</summary>
    internal static void VerifyNorfairRioInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        AssertEqual(65, NorfairRioInstructionProgramDefinitions.MechanicsWordCount,
            "Norfair Rio compiled mechanics word count");
        AssertEqual(34, NorfairRioInstructionProgramDefinitions.PresentationWordCount,
            "Norfair Rio live presentation word count");
        for (int index = 0;
             index < NorfairRioInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                NorfairRioInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadNorfairRioInstructionWord(rom, definition.Address),
                $"Norfair Rio mechanics word $A2:{definition.Address:X4}");
        }

        var guard = NewNorfairRioInstructionReadGuard(rom);
        (ushort Entry, int Calls, ushort Final, ushort Offset, bool Signal, string Name)[]
            programs =
        [
            (NorfairRioInstructionProgramDefinitions.Idle,
                5,
                unchecked((ushort)(NorfairRioInstructionProgramDefinitions.Idle + 6)),
                8, false, "idle"),
            (NorfairRioInstructionProgramDefinitions.StartDescending,
                7,
                unchecked((ushort)(
                    NorfairRioInstructionProgramDefinitions.StartDescending + 38)),
                unchecked((ushort)-16), true, "start descending"),
            (NorfairRioInstructionProgramDefinitions.Descending,
                5,
                unchecked((ushort)(NorfairRioInstructionProgramDefinitions.Descending + 6)),
                unchecked((ushort)-12), false, "descending"),
            (NorfairRioInstructionProgramDefinitions.StartAscending,
                9,
                unchecked((ushort)(
                    NorfairRioInstructionProgramDefinitions.StartAscending + 50)),
                12, true, "start ascending"),
            (NorfairRioInstructionProgramDefinitions.Ascending,
                5,
                unchecked((ushort)(NorfairRioInstructionProgramDefinitions.Ascending + 6)),
                12, false, "ascending"),
            (NorfairRioInstructionProgramDefinitions.FlamesAscending,
                5,
                unchecked((ushort)(
                    NorfairRioInstructionProgramDefinitions.FlamesAscending + 4)),
                0, false, "ascending flames"),
            (NorfairRioInstructionProgramDefinitions.FlamesDescending,
                5,
                unchecked((ushort)(
                    NorfairRioInstructionProgramDefinitions.FlamesDescending + 4)),
                0, false, "descending flames"),
        ];
        foreach ((ushort entry, int calls, ushort final, ushort offset, bool signal,
                  string name) in programs)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, NorfairRioEnemyState state) =
                NewNorfairRioInstructionSystem(guard, entry);
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            RunNorfairRioInstructionFrames(
                enemies,
                arguments,
                slot,
                calls);
            AssertEqual(final, slot.CurrentInstruction,
                $"Norfair Rio {name} completes its program");
            AssertEqual(offset, state.FollowerYOffset,
                $"Norfair Rio {name} publishes its final follower offset");
            AssertEqual(signal, state.AnimationSignal,
                $"Norfair Rio {name} animation signal");
        }

        Suite(nameof(VerifyNorfairRioInitializerSelections), () => VerifyNorfairRioInitializerSelections(guard));

        AssertEqual(0, guard.ObservedWords.Count,
            "Norfair Rio presentation selectors are compiled, not ROM reads");
        for (int index = 0;
             index < NorfairRioInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                NorfairRioInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertEqual(ReadNorfairRioInstructionWord(rom, address),
                EnemySpritemapDefinitions.NorfairRioFrameAt(address),
                $"compiled Norfair Rio selector $A2:{address:X4} matches ROM");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Norfair Rio mechanics byte");

        AssertThrows<InvalidDataException>(
            () => NorfairRioInstructionProgramDefinitions.ReadMechanicsWord(
                NorfairRioInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Norfair Rio spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => NorfairRioInstructionProgramDefinitions.ReadMechanicsWord(
                NorfairRioInstructionProgramDefinitions.AdjacentMovementDefinitions),
            "adjacent Norfair Rio movement definitions are rejected as mechanics");

        _ = ProbeNorfairRioInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeNorfairRioInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Norfair Rio allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Norfair Rio mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Norfair Rio instruction mechanics: 65 compiled words, all seven parent/" +
            "flame programs, eleven callbacks, and 34 compiled spritemap selectors pass.");
    }

    /// <summary>Checks that parent and follower initialization choose idle and ascending-flames programs respectively.</summary>
    private static void VerifyNorfairRioInitializerSelections(
        ISnesAddressSpace bus)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(bus);

        RoomEnemySlot parent = enemies.SlotTable.Slots[0];
        parent.EnemyDefinitionPointer = EnemyDefinitionId.Geruta;
        parent.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        enemies.NorfairRioBehavior.InitializeNorfairRio(parent);
        AssertEqual(NorfairRioInstructionProgramDefinitions.Idle,
            parent.CurrentInstruction,
            "Norfair Rio parent initializer selects idle");

        RoomEnemySlot follower = enemies.SlotTable.Slots[1];
        follower.EnemyDefinitionPointer = EnemyDefinitionId.Geruta;
        follower.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        follower.Parameter1 = 0x8000;
        enemies.NorfairRioBehavior.InitializeNorfairRio(follower);
        AssertEqual(NorfairRioInstructionProgramDefinitions.FlamesAscending,
            follower.CurrentInstruction,
            "Norfair Rio follower initializer selects ascending flames");
    }

    /// <summary>Seeds a Geruta-backed Norfair Rio actor with an instruction entry and optional installed artwork.</summary>
    internal static (
        RoomEnemySystem Enemies,
        RoomEnemySlot Slot,
        NorfairRioEnemyState State) NewNorfairRioInstructionSystem(
            ISnesAddressSpace bus,
            ushort entry,
            EnemyTileArtworkCatalog? art = null)
    {
        var enemies = new RoomEnemySystem { TileArtwork = art };
        enemies.BindFixtureBus(bus);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Geruta;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        slot.CurrentInstruction = entry;
        slot.InstructionTimer = 1;
        var state = new NorfairRioEnemyState(slot);
        var states = (NorfairRioEnemyState?[])enemies.NorfairRioBehavior._norfairRioStates;
        states[0] = state;
        return (enemies, slot, state);
    }

    /// <summary>Processes the selected instruction entry on each requested actor update.</summary>
    internal static void RunNorfairRioInstructionFrames(
        RoomEnemySystem enemies,
        object?[] arguments,
        RoomEnemySlot slot,
        int count)
    {
        for (int call = 0; call < count; call++)
        {
            slot.InstructionTimer = 1;
            EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    /// <summary>Repeatedly consumes idle and ascending control words for steady-state allocation measurement.</summary>
    private static int ProbeNorfairRioInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += NorfairRioInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? NorfairRioInstructionProgramDefinitions.Idle
                    : NorfairRioInstructionProgramDefinitions.StartAscending);
        }
        return checksum;
    }

    /// <summary>Reads a native bank-$A2 instruction word for the independent cartridge comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Norfair Rio instruction words")]
    internal static ushort ReadNorfairRioInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(0xa20000 | address);

    /// <summary>Forbids compiled control reads and observes or forbids native spritemap selectors.</summary>
    internal static CartridgeReadGuard NewNorfairRioInstructionReadGuard(
        ISnesAddressSpace source, bool forbidPresentation = false)
    {
        // Find the visual-selector word containing either byte of a forbidden read.
        static ushort SelectorWord(int address) => Enumerable.Range(0, NorfairRioInstructionProgramDefinitions.PresentationWordCount)
            .Select(NorfairRioInstructionProgramDefinitions.PresentationWordAddress)
            .First(word => unchecked((ushort)address) == word || unchecked((ushort)address) == unchecked((ushort)(word + 1)));
        CartridgeReadGuard guard = new CartridgeReadGuard(source)
            .Forbid(NorfairRioInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Norfair Rio mechanics byte ${address:X6}.");
        return forbidPresentation
            ? guard.ForbidWords(0xa20000, NorfairRioInstructionProgramDefinitions.PresentationWordCount, NorfairRioInstructionProgramDefinitions.PresentationWordAddress,
                address => $"Installed Norfair Rio read visual selector $A2:{SelectorWord(address):X4}.")
            : guard.ObserveWords(0xa20000, NorfairRioInstructionProgramDefinitions.PresentationWordCount, NorfairRioInstructionProgramDefinitions.PresentationWordAddress);
    }

    /// <summary>Runs the retail-cartridge Norfair Rio instruction verification.</summary>
    [VerificationCommand("--norfair-rio-instruction-program-definitions")]
    internal static int NorfairRioInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyNorfairRioInstructionProgramDefinitions), () => VerifyNorfairRioInstructionProgramDefinitions());
        return 0;
    }
}
