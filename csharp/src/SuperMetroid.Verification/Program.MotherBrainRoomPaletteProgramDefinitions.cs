using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMotherBrainRoomPaletteProgramDefinitions()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        for (int index = 0;
             index < MotherBrainRoomPaletteProgramDefinitions.MechanicsWordCount;
             index++)
        {
            MotherBrainRoomPaletteMechanicsWord definition =
                MotherBrainRoomPaletteProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                ReadMotherBrainRoomPaletteWord(rom, 0xa90000 | definition.Address),
                definition.Value,
                $"Mother Brain room-palette mechanics $A9:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new MotherBrainRoomPaletteReadGuard(rom);
        var enemies = new RoomEnemySystem { MotherBrainRoomColors = RetailPresentationFixture().MotherBrainRoomColors };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        var cgram = new SnesCgram();
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, cgram);
        MethodInfo run = typeof(RoomEnemySystem).GetMethod(
            "RunMotherBrainRoomPalette", flags)!;
        var state = new MotherBrainEnemyState(enemies.Slots[0])
        {
            RoomPaletteInstructionPointer =
                MotherBrainRoomPaletteProgramDefinitions.FlashStart,
        };

        var executedOperands = new HashSet<ushort>();
        for (int frame = 0; frame < 48; frame++)
        {
            run.Invoke(enemies, [state]);
            ushort operand = unchecked((ushort)(state.RoomPaletteInstructionPointer + 2));
            executedOperands.Add(operand);
            ushort source = ReadMotherBrainRoomPaletteWord(rom, 0xa90000 | operand);
            for (int color = 0; color < MotherBrainRoomColorRomData.SliceColors; color++)
            {
                ushort first = ReadMotherBrainRoomPaletteWord(rom, 0xa90000 | (source + color * 2));
                ushort second = ReadMotherBrainRoomPaletteWord(rom, 0xa90000 |
                    (source + (MotherBrainRoomColorRomData.SliceColors + color) * 2));
                AssertEqual(first, cgram.Colors[MotherBrainRoomColorRomData.FirstColor + color],
                    "Mother Brain flash first color slice matches cartridge");
                AssertEqual(second, cgram.Colors[MotherBrainRoomColorRomData.SecondColor + color],
                    "Mother Brain flash second color slice matches cartridge");
                AssertEqual(second, cgram.Colors[MotherBrainRoomColorRomData.MirroredSecondColor + color],
                    "Mother Brain flash mirrored color slice matches cartridge");
            }
        }

        AssertTrue(state.RoomPaletteInstructionPointer is >= 0xd046 and <= 0xd07e,
            "Mother Brain room-palette program loops within its authored control range");
        AssertEqual(0, guard.ForbiddenMechanicsReadAttempts,
            "Mother Brain room-palette execution avoids compiled mechanics bytes");
        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Mother Brain room palette performs zero live presentation reads");
        AssertEqual(MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount,
            executedOperands.Count, "Mother Brain flash executes every palette selection");
        for (int index = 0; index < MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount; index++)
            AssertTrue(executedOperands.Contains(MotherBrainRoomPaletteProgramDefinitions.PresentationWordAddress(index)),
                "Mother Brain flash selects every native palette row");

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
            "native color selections, zero live reads, complete production loop, strict rejection, " +
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
