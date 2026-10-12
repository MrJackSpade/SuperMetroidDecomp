using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

internal static partial class Program
{
    /// <summary>Checks the statically identified PLM/speed WRAM conversions, never searches gameplay for cartridge reads.</summary>
    private static void VerifyMechanicsWorkRamBoundary()
    {
        const BindingFlags privateStatic = BindingFlags.NonPublic | BindingFlags.Static;
        AssertTrue(typeof(RoomPlmSystem).GetMethod("ReadNativeBankByte", privateStatic) is null &&
            typeof(RoomPlmSystem).GetMethod("ReadBank84Word", privateStatic) is null,
            "PLMs must not retain generic bank-data readers");
        AssertEqual(typeof(ISnesMutableMemory), typeof(RoomPlmSystem)
            .GetMethod("ReadPlmWorkRamWord", privateStatic)!.GetParameters()[0].ParameterType,
            "wrapped PLM words have a compile-time mutable-memory source");
        AssertTrue(typeof(SamusHorizontalSpeedState).GetMethod("ReadMappedByte", privateStatic) is null,
            "horizontal speed must not retain a mapped CPU-byte reader");

        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        {
            const int origin = 3 * 16 + 3;
            var words = new ushort[16 * 16];
            words[origin] = 0xc456;
            var level = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(words, new byte[words.Length], new ushort[words.Length]), new byte[1024 * 8]);
            var plms = new RoomPlmSystem();
            var streamer = level.CreateBackgroundStreamer();
            AssertTrue(plms.TrySpawnBreakableGrappleBlock(level, origin, 1), "allocate bounded mechanics fixture");
            WriteMirroredWorkRamWord(memory, 0x841100, (ushort)RoomPlmInstruction.DrawPlmBlockClone);
            WriteMirroredWorkRamWord(memory, 0x841102, (ushort)RoomPlmInstruction.Delete);
            plms.SetSoleInstructionPointerForVerification(0x1100);
            plms.Step(memory, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
            AssertEqual(0xc456,
                level.GetCollisionBlockByIndex(origin).LevelWord, "active WRAM drives the exact physical output");
            AssertEqual(1, plms.TilemapUpdates.Count, "WRAM-backed draw publishes its visible update");
            plms.Step(memory, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
            AssertEqual(0, plms.ActiveCount, "WRAM-backed instruction resumes and deletes on the next pass");
        }

        var readSpeed = typeof(SamusHorizontalSpeedState)
            .GetMethod("ReadEntryAtAddress", privateStatic)!
            .CreateDelegate<Func<ISnesAddressSpace, int, SpeedTableEntry>>();
        for (int index = 0; index < 6; index++)
            WriteMirroredWorkRamWord(memory, 0x900100 + index * 2, (ushort)(0x1200 + index));
        AssertEqual(new SpeedTableEntry(0x1200, 0x1201, 0x1202, 0x1203, 0x1204, 0x1205),
            readSpeed(memory, 0x900100), "wrapped speed reads consume all six live WRAM words");
        AssertThrows<ArgumentOutOfRangeException>(() => readSpeed(memory, 0x902000),
            "a peripheral offset is not WRAM physics data");
        AssertThrows<InvalidDataException>(() => readSpeed(memory, 0x90ffff),
            "an uncompiled cartridge offset is rejected without reading it");
        Console.WriteLine("Mechanics WRAM boundary: generic PLM/speed readers absent; RAM-only instruction/draw/slot timing and all six live speed words pass.");
    }

    /// <summary>Seeds a little-endian WRAM word through a system-bank low-window mirror address, low byte first.</summary>
    [CpuBusBoundary("exact bus test: seeds PLM instructions and speed rows through system-bank low-window WRAM mirrors")]
    private static void WriteMirroredWorkRamWord(SuperMetroidAddressSpace writer, int address, ushort value)
    {
        writer.WriteByte(address, (byte)value);
        writer.WriteByte(address + 1, SnesLittleEndian.HighByte(value));
    }
}
