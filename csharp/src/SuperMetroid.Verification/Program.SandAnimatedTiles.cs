using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>ROM-free bank-$83 selection, bank-$87 cadence, and NMI transfer regression for sand.</summary>
    static void VerifySandAnimatedTiles()
    {
        var bus = new TestAddressSpace();
        var vram = new SnesVram();
        var queue = new VramWriteQueue();
        var sand = new RoomSandAnimatedTilesState();
        WriteTestWord(bus, RoomFxRomData.Tables.AreaAnimatedTileObjectListPointers + 8, 0x9000);
        WriteTestWord(bus, 0x839000, AnimatedTileObjectPointers.MaridiaSandCeiling);
        WriteTestWord(bus, 0x839002, AnimatedTileObjectPointers.MaridiaSandFalling);
        bus.WriteBytes(0x839100, new byte[16]);
        bus.WriteBytes(0x83910e, [3]);
        ushort[] definitions = [AnimatedTileObjectPointers.MaridiaSandCeiling, AnimatedTileObjectPointers.MaridiaSandFalling];
        for (int slot = 0; slot < 2; slot++)
        {
            int header = RoomFxRomData.Banks.AnimatedTiles | definitions[slot];
            ushort list = (ushort)(0xa000 + slot * 0x100);
            WriteTestWord(bus, header, list);
            WriteTestWord(bus, header + 2, 32);
            WriteTestWord(bus, header + 4, (ushort)(0x1000 + slot * 16));
            for (int frame = 0; frame < 2; frame++)
            {
                ushort source = (ushort)(0xb000 + slot * 64 + frame * 32);
                WriteTestWord(bus, 0x870000 | (list + frame * 4), 2);
                WriteTestWord(bus, 0x870000 | (list + frame * 4 + 2), source);
                bus.WriteBytes(0x870000 | source, Enumerable.Repeat((byte)(1 + slot * 2 + frame), 32).ToArray());
            }
            WriteTestWord(bus, 0x870000 | (list + 8), AnimatedTileInstructionCodes.Goto);
            WriteTestWord(bus, 0x870000 | (list + 10), list);
        }
        sand.LoadRoom(bus, 0x9100, 0, AreaId.Maridia);
        AssertEqual(2, sand.Count, "both FX sand bits create objects");
        for (int tick = 0; tick < 12; tick++)
        {
            byte before = vram.ReadByte(0x2000);
            sand.Step(bus, vram, queue);
            AssertEqual(before, vram.ReadByte(0x2000), "sand animation does not bypass NMI");
            AssertEqual(tick % 2 == 0 ? 2 : 0, queue.Entries.Count, "sand source changes follow ROM durations");
            queue.DrainTo(vram, bus);
            for (int slot = 0; slot < 2; slot++)
                for (int i = 0; i < 32; i++)
                    AssertEqual((byte)(1 + slot * 2 + tick / 2 % 2), vram.ReadByte(0x2000 + slot * 32 + i),
                        "sand ceiling/fall graphics loop independently at exact destinations");
        }
        sand.LoadRoom(bus, 0, 0, AreaId.Maridia);
        AssertEqual(0, sand.Count, "next room without FX clears sand owners");
        sand.Step(bus, vram, queue);
        AssertEqual(0, queue.Entries.Count, "departed room cannot continue writing sand graphics");
        Console.WriteLine("  Sand animation: FX selection, timed loops, NMI visibility, and room reset agree.");
    }
}
