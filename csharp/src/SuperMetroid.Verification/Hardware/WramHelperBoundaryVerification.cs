using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
namespace SuperMetroid.Verification.Hardware;
using SuperMetroid.Verification.Harness;

/// <summary>Checks migrated WRAM helpers with an address space that has no cartridge.</summary>
internal static class WramHelperBoundaryVerification
{
    /// <summary>Checks the statically identified WRAM-only helpers without importing a cartridge or running gameplay.</summary>
    internal static void VerifyWramHelperBoundary()
    {
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        VerifyWramWordMapping(memory);

        Suite(nameof(VerifyWramCorpseScheduler), () => VerifyWramCorpseScheduler());
        Console.WriteLine("WRAM helper boundary: bank wrapping, source rejection and corpse scheduling pass without a cartridge.");
    }

    /// <summary>Checks word wrapping, system-bank mirrors, and rejection of non-WRAM addresses.</summary>
    [CpuBusBoundary("exact bus test: WRAM word bank wrap, system-bank mirror and non-WRAM source rejection")]
    private static void VerifyWramWordMapping(SuperMetroidAddressSpace memory)
    {
        memory.WriteByte(0x7effff, 0x12);
        memory.WriteByte(0x7e0000, 0x34);
        memory.WriteByte(0x7f0000, 0x56);
        AssertEqual((ushort)0x3412, memory.ReadWorkRamWordFixedBank(0x7effff),
            "WRAM word wraps its offset inside the same bank");
        AssertEqual((ushort)0x0034, memory.ReadWorkRamWordFixedBank(0x800000),
            "WRAM word accepts native system-bank mirrors");
        foreach (int address in new[] { 0x908000, 0x700000, 0x002140 })
            AssertThrows<ArgumentOutOfRangeException>(() => memory.ReadWorkRamWordFixedBank(address),
                "WRAM helper rejects cartridge, SRAM and peripheral sources");
    }

    /// <summary>Checks staggered corpse-row callbacks and sentinel writes in the rotting heap.</summary>
    private static void VerifyWramCorpseScheduler()
    {
        CorpseRotTable table = new EnemyStagingSurfaces().RotTable(0, 3);
        CorpseRottingTableProcessor.Initialize(table);
        for (int index = 0; index < 3; index++)
            AssertEqual(new CorpseRottingTableEntry((short)(2 - index), (ushort)(2 * index)),
                CorpseRottingTableProcessor.ReadEntry(table, index), "native rot table initialization");
        var rows = new List<WramHelperBoundaryRow>();
        var finished = new List<ushort>();
        bool active = CorpseRottingTableProcessor.Step(table, 2, 1,
            (row, operation) => rows.Add(new WramHelperBoundaryRow(row, operation == CorpseRowOperation.Move)), finished.Add);
        AssertTrue(active, "a finished non-final rot entry does not finish the scheduler");
        AssertTrue(rows.SequenceEqual(new[] { new WramHelperBoundaryRow(2, true), new WramHelperBoundaryRow(1, true), new WramHelperBoundaryRow(0, true) }),
            "rot callback order and late-move selection");
        AssertTrue(finished.SequenceEqual(new ushort[] { 0 }), "only the expired first row finishes");
        AssertEqual(new CorpseRottingTableEntry(-1, 0),
            CorpseRottingTableProcessor.ReadEntry(table, 0), "finished row receives native negative sentinel");
        AssertEqual(new CorpseRottingTableEntry(1, 1),
            CorpseRottingTableProcessor.ReadEntry(table, 1), "second row retains its decremented WRAM delay");
    }

    /// <summary>Runs the cartridge-free WRAM helper checks.</summary>
    [VerificationCommand("--wram-helper-boundary")]
    internal static int WramHelperBoundaryCommand()
    {
        Suite(nameof(VerifyWramHelperBoundary), () => VerifyWramHelperBoundary());
        return 0;
    }
}
