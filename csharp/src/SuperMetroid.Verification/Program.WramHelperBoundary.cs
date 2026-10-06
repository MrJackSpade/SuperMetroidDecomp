using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the statically identified WRAM-only helpers without importing a cartridge or running gameplay.</summary>
    private static void VerifyWramHelperBoundary()
    {
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        memory.WriteByte(0x7effff, 0x12);
        memory.WriteByte(0x7e0000, 0x34);
        memory.WriteByte(0x7f0000, 0x56);
        AssertEqual((ushort)0x3412, SnesWorkRam.ReadWord(memory, 0x7effff),
            "WRAM word wraps its offset inside the same bank");
        AssertEqual((ushort)0x0034, SnesWorkRam.ReadWord(memory, 0x800000),
            "WRAM word accepts native system-bank mirrors");
        foreach (int address in new[] { 0x908000, 0x700000, 0x002140 })
            AssertThrows<ArgumentOutOfRangeException>(() => SnesWorkRam.ReadWord(memory, address),
                "WRAM helper rejects cartridge, SRAM and peripheral sources");

        Suite(nameof(VerifyWramCorpseScheduler), () => VerifyWramCorpseScheduler(memory));
        Suite(nameof(VerifyWramProjectileInheritance), () => VerifyWramProjectileInheritance(memory));
        FirefleaRoomFx.Initialize(memory);
        ushort[] shades = [0, 1, 2, 3, 4, 5, 6, 5, 4, 3, 2, 1];
        for (int frame = 1; frame <= 144; frame++)
        {
            FirefleaRoomFx.Step(memory, memory, frozen: false, darkness: 0);
            AssertEqual((byte)(shades[frame / 6 % shades.Length] | 0x20),
                memory.ReadWorkRamByte(PpuFixedColorMirrors.Red), "Fireflea native six-frame shade cadence");
        }
        ushort retainedTimer = SnesWorkRam.ReadWord(memory, FirefleaFxRomData.Timer);
        FirefleaRoomFx.Step(memory, memory, frozen: true, darkness: 2);
        AssertEqual(retainedTimer, SnesWorkRam.ReadWord(memory, FirefleaFxRomData.Timer),
            "frozen Fireflea leaves the live WRAM timer unchanged");
        AssertEqual((ushort)2, SnesWorkRam.ReadWord(memory, FirefleaFxRomData.Darkness),
            "frozen Fireflea still publishes the enemy-owned darkness counter");
        Console.WriteLine("WRAM helper boundary: bank wrapping, source rejection, corpse scheduling, 50 projectile-inheritance records and Fireflea cadence pass without a cartridge.");
    }

    private static void VerifyWramCorpseScheduler(SuperMetroidAddressSpace memory)
    {
        const int table = 0x7e1800;
        CorpseRottingTableProcessor.Initialize(memory, table, 3);
        for (int index = 0; index < 3; index++)
            AssertEqual(new CorpseRottingTableEntry((short)(2 - index), (ushort)(2 * index)),
                CorpseRottingTableProcessor.ReadEntry(memory, table, 3, index), "native rot table initialization");
        var rows = new List<(ushort Row, bool Move)>();
        var finished = new List<ushort>();
        bool active = CorpseRottingTableProcessor.Step(memory, memory, table, 3, 2, 1,
            (row, move) => rows.Add((row, move)), finished.Add);
        AssertTrue(active, "a finished non-final rot entry does not finish the scheduler");
        AssertTrue(rows.SequenceEqual(new[] { ((ushort)2, true), ((ushort)1, true), ((ushort)0, true) }),
            "rot callback order and late-move selection");
        AssertTrue(finished.SequenceEqual(new ushort[] { 0 }), "only the expired first row finishes");
        AssertEqual(new CorpseRottingTableEntry(-1, 0),
            CorpseRottingTableProcessor.ReadEntry(memory, table, 3, 0), "finished row receives native negative sentinel");
        AssertEqual(new CorpseRottingTableEntry(1, 1),
            CorpseRottingTableProcessor.ReadEntry(memory, table, 3, 1), "second row retains its decremented WRAM delay");
    }

    private static void VerifyWramProjectileInheritance(SuperMetroidAddressSpace memory)
    {
        ushort[][] snapshots =
        [
            [0, 0, 0, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 3, 0, 0, 0, 0, 0],
            [0, 0xfffd, 0, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0, 0xfffc, 0, 0, 0],
            [0xabcd, 0, 0x1234, 0, 0x5678, 0, 0x9abc, 0, 0],
        ];
        foreach (ushort[] snapshot in snapshots)
        {
            for (int index = 0; index < snapshot.Length; index++)
            {
                int address = SamusProjectileInheritanceAddresses.CameraYSubspeed + index * 2;
                memory.WriteByte(address, (byte)snapshot[index]);
                memory.WriteByte(address + 1, (byte)(snapshot[index] >> 8));
            }
            for (ushort direction = 0; direction < 10; direction++)
            {
                short speed = (short)(direction is 1 or 3 or 6 or 8 ? 0x02ab : 0x0400);
                // These overlapping words are a native load across adjacent fields, not net displacement.
                int upward = snapshot[4] >> 8 | snapshot[5] << 8;
                int up = (upward & 0xff00) == 0 ? 0 : ((upward & 0xffff) >> 2) | 0xc000;
                short x = unchecked((short)(direction switch
                {
                    1 or 2 or 3 => speed + (snapshot[2] >> 8 | snapshot[3] << 8),
                    6 or 7 or 8 => -speed + (snapshot[0] >> 8 | snapshot[1] << 8),
                    _ => 0,
                }));
                short y = unchecked((short)(direction switch
                {
                    0 or 1 or 8 or 9 => -speed + up,
                    3 or 4 or 5 or 6 => speed + (snapshot[6] >> 8 | snapshot[7] << 8),
                    _ => 0,
                }));
                AssertEqual((x, y), SamusProjectileInheritance.ReadVelocity(memory, direction, speed),
                    "projectile inheritance preserves each directional overlapping word");
            }
        }
    }
}
