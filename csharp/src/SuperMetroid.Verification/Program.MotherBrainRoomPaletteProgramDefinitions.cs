using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMotherBrainRoomPaletteProgramDefinitions()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        for (int index = 0;
             index < MotherBrainRoomPaletteProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                MotherBrainRoomPaletteProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                ReadMotherBrainRoomPaletteWord(rom, 0xa90000 | definition.Address),
                definition.Value,
                $"Mother Brain room-palette mechanics $A9:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new MotherBrainRoomPaletteReadGuard(rom);
        var enemies = new RoomEnemySystem
        {
            MotherBrainRoomColors = MotherBrainRoomColorPresentation.Load(new MemoryStream(
                SuperMetroid.AssetExtraction.MotherBrainRoomColorExtractor.Extract(rom))),
        };
        var output = new SnesCgram();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, output);
        MethodInfo run = typeof(RoomEnemySystem).GetMethod(
            "RunMotherBrainRoomPalette", flags)!;
        var state = new MotherBrainEnemyState(enemies.Slots[0])
        {
            RoomPaletteInstructionPointer =
                MotherBrainRoomPaletteProgramDefinitions.FlashStart,
        };

        var expected = new SnesCgram();
        ushort nativePointer = 0xd046;
        ushort nativeTimer = 0;
        var nativeOperands = new HashSet<ushort>();
        for (int frame = 0; frame < 48; frame++)
        {
            ushort duration = ReadMotherBrainRoomPaletteWord(rom, 0xa90000 | nativePointer);
            if (nativeTimer == duration)
            {
                nativePointer += 4;
                nativeTimer = 0;
                if (ReadMotherBrainRoomPaletteWord(rom, 0xa90000 | nativePointer) == 0x9b0f)
                    nativePointer = ReadMotherBrainRoomPaletteWord(rom, 0xa90000 | (nativePointer + 2));
            }
            nativeTimer++;
            nativeOperands.Add((ushort)(nativePointer + 2));
            ushort palette = ReadMotherBrainRoomPaletteWord(rom, 0xa90000 | (nativePointer + 2));
            for (int color = 0; color < 12; color++)
            {
                expected.SetColor(0x34 + color, ReadMotherBrainRoomPaletteWord(rom, 0xa90000 | (palette + color * 2)));
                ushort second = ReadMotherBrainRoomPaletteWord(rom, 0xa90000 | (palette + (12 + color) * 2));
                expected.SetColor(0x53 + color, second);
                expected.SetColor(0x73 + color, second);
            }
            run.Invoke(enemies, [state]);
            AssertEqual(nativePointer, state.RoomPaletteInstructionPointer, "native room flash pointer and loop");
            AssertEqual(nativeTimer, state.RoomPaletteInstructionTimer, "native room flash two-tick cadence");
            AssertTrue(expected.Colors.SequenceEqual(output.Colors), "native room flash full CGRAM and mirrored colors");
        }
        AssertEqual(14, nativeOperands.Count, "native oracle traverses every room flash operand");

        AssertTrue(state.RoomPaletteInstructionPointer is >= 0xd046 and <= 0xd07e,
            "Mother Brain room-palette program loops within its authored control range");
        AssertEqual(0, guard.ForbiddenMechanicsReadAttempts,
            "Mother Brain room-palette execution avoids compiled mechanics bytes");
        AssertEqual(
            0,
            guard.ObservedPresentationWords.Count,
            "room flash uses installed colors without runtime presentation reads");

        AssertThrows<InvalidDataException>(
            () => MotherBrainRoomPaletteProgramDefinitions.ReadMechanicsWord(
                MotherBrainRoomPaletteProgramDefinitions.PresentationWordAddress(0)),
            "Mother Brain palette pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MotherBrainRoomPaletteProgramDefinitions.ReadMechanicsWord(0xd082),
            "Mother Brain palette payload is outside the control program");

        _ = ProbeMotherBrainRoomPaletteAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMotherBrainRoomPaletteAllocation();
        AssertTrue(checksum != 0,
            "Mother Brain room-palette allocation probe consumes mechanics data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Mother Brain room-palette mechanics lookups allocate no storage");

        Console.WriteLine(
            "Mother Brain room-palette mechanics: sixteen control words, fourteen " +
            "installed palettes, native CGRAM/pointer/timer loop, zero presentation reads, strict rejection, " +
            "and allocation-free lookup pass.");
    }

    private static ushort ReadMotherBrainRoomPaletteWord(
        SuperMetroidAddressSpace source,
        int address) =>
        unchecked((ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8));

    private static int ProbeMotherBrainRoomPaletteAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MotherBrainRoomPaletteProgramDefinitions.ReadMechanicsWord(
                MotherBrainRoomPaletteProgramDefinitions.FlashStart);
        }
        return checksum;
    }

    private sealed class MotherBrainRoomPaletteReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenMechanicsReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (MotherBrainRoomPaletteProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenMechanicsReadAttempts++;
                throw new InvalidOperationException(
                    $"Mother Brain room palette read compiled mechanics ${address:X6}.");
            }
            if (MotherBrainRoomPaletteProgramDefinitions.TryGetPresentationWord(
                    address, out ushort presentation))
            {
                ObservedPresentationWords.Add(presentation);
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
