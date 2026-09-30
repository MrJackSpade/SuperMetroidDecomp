using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks the statically identified PLM/speed WRAM conversions, never searches gameplay for cartridge reads.</summary>
    private static void VerifyMechanicsWorkRamBoundary()
    {
        static void WriteMemoryWord(ISnesAddressSpace writer, int address, ushort value)
        {
            writer.WriteByte(address, (byte)value);
            writer.WriteByte(address + 1, (byte)(value >> 8));
        }
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
        foreach (bool instructionInWorkRam in new[] { false, true })
        {
            const int origin = 3 * 16 + 3;
            var words = new ushort[16 * 16];
            words[origin] = 0xc456;
            var level = new RoomLevelData(16, 16, words, new byte[words.Length],
                new ushort[words.Length], new byte[1024 * 8]);
            var plms = new RoomPlmSystem();
            var streamer = level.CreateBackgroundStreamer();
            AssertTrue(plms.TrySpawnBreakableGrappleBlock(level, origin, 1), "allocate bounded mechanics fixture");
            if (instructionInWorkRam)
            {
                WriteMemoryWord(memory, 0x841100, RoomPlmInstructionCodes.DrawPlmBlockClone);
                WriteMemoryWord(memory, 0x841102, RoomPlmInstructionCodes.Delete);
                plms.SetSoleInstructionPointerForVerification(0x1100);
            }
            else
            {
                WriteMemoryWord(memory, 0x841000, 1);
                WriteMemoryWord(memory, 0x841002, 0xc321);
                WriteMemoryWord(memory, 0x841004, 0);
                plms.SetSoleInstructionPointerForVerification(0xf300,
                    [1, 0x1000, RoomPlmInstructionCodes.Delete]);
            }
            plms.Step(memory, level, streamer, 0, 0, 0);
            AssertEqual(instructionInWorkRam ? 0xc456 : 0xc321,
                level.GetCollisionBlockByIndex(origin).LevelWord, "active WRAM drives the exact physical output");
            AssertEqual(1, plms.TilemapUpdates.Count, "WRAM-backed draw publishes its visible update");
            plms.Step(memory, level, streamer, 0, 0, 0);
            AssertEqual(0, plms.ActiveCount, "WRAM-backed instruction resumes and deletes on the next pass");
        }

        var readSpeed = typeof(SamusHorizontalSpeedState)
            .GetMethod("ReadEntryAtAddress", privateStatic)!
            .CreateDelegate<Func<ISnesAddressSpace, int, SpeedTableEntry>>();
        for (int index = 0; index < 6; index++)
            WriteMemoryWord(memory, 0x900100 + index * 2, (ushort)(0x1200 + index));
        AssertEqual(new SpeedTableEntry(0x1200, 0x1201, 0x1202, 0x1203, 0x1204, 0x1205),
            readSpeed(memory, 0x900100), "wrapped speed reads consume all six live WRAM words");
        AssertThrows<ArgumentOutOfRangeException>(() => readSpeed(memory, 0x902000),
            "a peripheral offset is not WRAM physics data");
        AssertThrows<InvalidDataException>(() => readSpeed(memory, 0x90ffff),
            "an uncompiled cartridge offset is rejected without reading it");
        Console.WriteLine("Mechanics WRAM boundary: generic PLM/speed readers absent; RAM-only instruction/draw/slot timing and all six live speed words pass.");
    }
}
