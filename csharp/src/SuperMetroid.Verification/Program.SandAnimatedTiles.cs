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
        bus.WriteBytes(0x839100, new byte[16]);
        bus.WriteBytes(0x83910e, [0x0c]);
        ushort[] definitions = [AnimatedTileObjectPointers.MaridiaSandCeiling, AnimatedTileObjectPointers.MaridiaSandFalling];
        for (int slot = 0; slot < 2; slot++)
        {
            AssertTrue(RoomFxAnimatedTileMechanicsDefinitions.TryResolve(
                    definitions[slot], out RoomFxAnimatedTileObjectDefinition definition),
                $"sand object {slot} resolves compiled mechanics");
            for (int frame = 0; frame < definition.Frames.Count; frame++)
            {
                ushort source = (ushort)(0xb000 + slot * 0x200 +
                    frame * definition.TransferByteCount);
                WriteTestWord(bus,
                    RoomFxRomData.Banks.AnimatedTiles |
                        definition.Frames[frame].SourceOperandPointer,
                    source);
                bus.WriteBytes(RoomFxRomData.Banks.AnimatedTiles | source,
                    Enumerable.Repeat((byte)(1 + slot * 4 + frame),
                        definition.TransferByteCount).ToArray());
            }
        }
        sand.LoadRoom(bus, 0x9100, 0, AreaId.Maridia);
        AssertEqual(2, sand.Count, "both FX sand bits create objects");
        for (int tick = 0; tick < 31; tick++)
        {
            byte before = vram.ReadByte(0x2000);
            sand.Step(bus, vram, queue);
            AssertEqual(before, vram.ReadByte(0x2000), "sand animation does not bypass NMI");
            AssertEqual(tick % 10 == 0 ? 2 : 0, queue.Entries.Count,
                "sand source changes follow compiled cartridge durations");
            queue.DrainTo(vram, bus);
            for (int slot = 0; slot < 2; slot++)
            {
                AssertTrue(RoomFxAnimatedTileMechanicsDefinitions.TryResolve(
                        definitions[slot], out RoomFxAnimatedTileObjectDefinition definition),
                    $"sand object {slot} remains catalogued");
                int destination = definition.EncodedVramDestination * 2;
                for (int i = 0; i < definition.TransferByteCount; i++)
                {
                    AssertEqual((byte)(1 + slot * 4 + tick / 10 % 4),
                        vram.ReadByte(destination + i),
                        "sand ceiling/fall graphics loop independently at exact destinations");
                }
            }
        }
        sand.LoadRoom(bus, 0, 0, AreaId.Maridia);
        AssertEqual(0, sand.Count, "next room without FX clears sand owners");
        sand.Step(bus, vram, queue);
        AssertEqual(0, queue.Entries.Count, "departed room cannot continue writing sand graphics");
        Console.WriteLine("  Sand animation: FX selection, timed loops, NMI visibility, and room reset agree.");
    }
}
