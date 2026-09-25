using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Compares the entire bounded cartridge list, then drives its real PLM
    /// dispatcher through every glass-break threshold without permitting a ROM
    /// read of the compiled control bytes.
    /// </summary>
    private static void VerifyMotherBrainGlassPlmProgram(SuperMetroidAddressSpace rom)
    {
        for (int address = MotherBrainGlassPlmProgramDefinitions.FirstAddress;
             address <= MotherBrainGlassPlmProgramDefinitions.LastAddress; address++)
        {
            AssertTrue(MotherBrainGlassPlmProgramDefinitions.TryReadMechanicsByte(
                checked((ushort)address), out byte compiled),
                $"glass PLM claims authored byte $84:{address:X4}");
            AssertEqual(rom.ReadByte(0x840000 | address), compiled,
                $"glass PLM byte $84:{address:X4} matches retail ROM");
            if (address == MotherBrainGlassPlmProgramDefinitions.LastAddress)
                continue;
            AssertTrue(MotherBrainGlassPlmProgramDefinitions.TryReadMechanicsWord(
                checked((ushort)address), out ushort compiledWord),
                $"glass PLM claims authored word at $84:{address:X4}");
            ushort nativeWord = (ushort)(rom.ReadByte(0x840000 | address) |
                rom.ReadByte(0x840000 | (address + 1)) << 8);
            AssertEqual(nativeWord, compiledWord,
                $"glass PLM word $84:{address:X4} matches retail ROM");
        }
        AssertTrue(!MotherBrainGlassPlmProgramDefinitions.TryReadMechanicsByte(0xd2f9, out _),
            "glass list does not claim the adjacent threshold callback code");
        AssertTrue(!MotherBrainGlassPlmProgramDefinitions.TryReadMechanicsWord(0xd2f8, out _),
            "glass list refuses a word crossing into callback code");

        var source = new TestAddressSpace();
        var bank84 = new byte[0x8000];
        for (int index = 0; index < bank84.Length; index++)
            bank84[index] = rom.ReadByte(0x848000 + index);
        source.WriteBytes(0x848000, bank84);
        source.WriteBytes(0x8f9000,
        [
            unchecked((byte)RoomPlmHeaders.MotherBrainGlass),
            unchecked((byte)(RoomPlmHeaders.MotherBrainGlass >> 8)),
            9, 5, 0, 0x80, 0, 0,
        ]);
        var guarded = new MotherBrainGlassProgramReadGuard(source);
        const int width = 32;
        var level = new RoomLevelData(width, 16,
            new ushort[width * 16], new byte[width * 16],
            new ushort[width * 16], new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        bool eventSet = false;
        var plms = new RoomPlmSystem();
        AssertEqual(1, plms.LoadRoomPopulation(guarded, level, streamer,
            new SnesVram(), 0x9000, new Bank80SystemState(), AreaId.Tourian,
            () => new SamusState(), () => false,
            hasAreaBossBit: _ => false,
            hasEvent: _ => eventSet,
            setEvent: eventNumber =>
            {
                AssertEqual(EventNumber.MotherBrainGlassDestroyed, eventNumber,
                    "glass completion sets the native break event");
                eventSet = true;
            }), "glass PLM loads for complete program test");

        // With no head hits, the first one-frame draw must loop at the $0002
        // threshold instead of shattering early. Both passes use the guarded
        // production interpreter, not an independent table reader.
        plms.Step(guarded, level, streamer, 0, 0, 0);
        AssertEqual(0xd215, plms.MotherBrainGlassInstructionPointer,
            "intact glass first draw advances to the hit threshold");
        plms.Step(guarded, level, streamer, 0, 0, 0);
        AssertEqual(0xd215, plms.MotherBrainGlassInstructionPointer,
            "zero head hits loop at the first glass stage");
        AssertTrue(!eventSet && plms.MotherBrainGlassProjectileRequests.Count == 0,
            "unhit glass emits neither event nor shards");

        // Head-shot AI can increment the native room argument before a PLM
        // pass. Advance it through the final $12 threshold, then let each
        // one-frame draw and each authored burst execute in production order.
        for (int shot = 0; shot < 18; shot++)
            plms.IncrementMotherBrainGlassRoomArgument();
        int emitted = 0;
        for (int frame = 0; frame < 128 && !plms.MotherBrainGlassWasDeleted; frame++)
        {
            plms.Step(guarded, level, streamer, 0, 0, 0);
            emitted += plms.MotherBrainGlassProjectileRequests.Count;
        }
        AssertTrue(eventSet && plms.MotherBrainGlassWasDeleted,
            $"all glass stages end by setting the event and deleting the PLM; " +
            $"pointer=${plms.MotherBrainGlassInstructionPointer:X4}, " +
            $"timer={plms.MotherBrainGlassInstructionTimer}");
        AssertEqual(40, emitted,
            "five authored shatter stages emit four shards twice each");
        AssertEqual(18, plms.MotherBrainGlassRoomArgument,
            "head-shot count survives PLM deletion");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "complete glass shatter never rereads compiled program bytes");
        Console.WriteLine("  Mother Brain glass PLM: all program bytes match ROM; full shatter runs without ROM instruction reads.");
    }

    private sealed class MotherBrainGlassProgramReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address >= 0x84d202 && address <= 0x84d2f8)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Mother Brain glass reread compiled program byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
