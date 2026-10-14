using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;
using static SuiteTimingVerification;

/// <summary>Checks Tourian access crumble and clear PLM spawn identities and slot initialization.</summary>
internal static class TourianAccessPlmDefinitionsVerification
{
    /// <summary>Compares native spawn operands, then confirms both states allocate the highest free slot at the fixed access block.</summary>
    internal static void VerifyTourianAccessPlmDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyTourianAccessHeaderSelection), () => VerifyTourianAccessHeaderSelection(rom));
        Suite(nameof(VerifyTourianAccessInstructionSelection), () => VerifyTourianAccessInstructionSelection(rom));
        Suite(nameof(VerifyDefinition), () => VerifyDefinition(clear: false));
        Suite(nameof(VerifyDefinition), () => VerifyDefinition(clear: true));

        Console.WriteLine(
            "Tourian access PLMs: both native header/list identities and real highest-slot spawns pass without runtime header reads.");

        void VerifyDefinition(bool clear)
        {
            TourianAccessPlmDefinition definition = TourianAccessPlmDefinitions.ForState(clear);
            const int width = 16;
            const int height = 16;
            int blockCount = width * height;
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(new ushort[blockCount], new byte[blockCount], new ushort[blockCount]), new byte[8]);
            var plms = new RoomPlmSystem();

            AssertTrue(
                plms.Tourian.TrySpawnTourianAccess(level, clear),
                $"Tourian access {(clear ? "clear" : "crumble")} actor allocates");
            AssertEqual(1, plms.SlotTable.ActiveCount, "Tourian access spawn occupies one PLM slot");
            RoomPlmSlotSnapshot slot = plms.PopulationSlots.Single();
            AssertEqual(39, slot.NativeSlotIndex, "Tourian access uses the highest free native slot");
            AssertEqual(definition.HeaderPointer, slot.HeaderPointer, "Tourian access header identity");
            AssertEqual(12 * width + 6, slot.BlockIndex, "Tourian access fixed block coordinate");
            AssertEqual(
                definition.InstructionListPointer,
                slot.InstructionPointer,
                "Tourian access compiled initial instruction list");
            AssertEqual(1, slot.InstructionTimer, "Tourian access starts on timer one");
        }
    }

    /// <summary>Compares both native spawn header operands with the compiled state selection.</summary>
    private static void VerifyTourianAccessHeaderSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyTourianAccessSpawnField), () => VerifyTourianAccessSpawnField(rom, false));

    /// <summary>Compares the initial instruction pointers attached to both native headers.</summary>
    private static void VerifyTourianAccessInstructionSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyTourianAccessSpawnField), () => VerifyTourianAccessSpawnField(rom, true));

    /// <summary>Reads the native crumble and already-unlocked spawn operands, including their fixed block coordinates.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Tourian access spawn field")]
    private static void VerifyTourianAccessSpawnField(SuperMetroidAddressSpace rom, bool instruction)
    {
        // Native spawn operands: completed descent crumbles; already-unlocked room clears.
        (bool Clear, int SpawnOperand)[] cases = [(false,0x88dc9f), (true,0x88db99)];
        foreach (var item in cases)
        {
            ushort header = CartridgeImportSource.Require(rom).ReadCartridgeWord(item.SpawnOperand);
            var actual = TourianAccessPlmDefinitions.ForState(item.Clear);
            AssertEqual(instruction ? CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | (header + 2))) : header,
                instruction ? actual.InstructionListPointer : (ushort)actual.HeaderPointer,
                $"Tourian native spawn selection clear={item.Clear} instruction={instruction}");
            AssertEqual((byte)6, rom.ReadCartridgeByte(item.SpawnOperand - 2), "Tourian native spawn column");
            AssertEqual((byte)12, rom.ReadCartridgeByte(item.SpawnOperand - 1), "Tourian native spawn row");
        }
    }

    /// <summary>Runs the Tourian access spawn definition comparison with a retail cartridge oracle.</summary>
    [VerificationCommand("--tourian-access-definitions")]
    internal static int TourianAccessDefinitionsCommand()
    {
        Suite(nameof(VerifyTourianAccessPlmDefinitions), () => VerifyTourianAccessPlmDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
