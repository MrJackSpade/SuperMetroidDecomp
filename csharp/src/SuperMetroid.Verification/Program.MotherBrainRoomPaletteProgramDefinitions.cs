using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMotherBrainRoomPaletteProgramDefinitions()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
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
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
        MethodInfo run = typeof(RoomEnemySystem).GetMethod(
            "RunMotherBrainRoomPalette", flags)!;
        var state = new MotherBrainEnemyState(enemies.Slots[0])
        {
            RoomPaletteInstructionPointer =
                MotherBrainRoomPaletteProgramDefinitions.FlashStart,
        };

        for (int frame = 0; frame < 48; frame++)
            run.Invoke(enemies, [state]);

        AssertTrue(state.RoomPaletteInstructionPointer is >= 0xd046 and <= 0xd07e,
            "Mother Brain room-palette program loops within its authored control range");
        AssertEqual(0, guard.ForbiddenMechanicsReadAttempts,
            "Mother Brain room-palette execution avoids compiled mechanics bytes");
        AssertEqual(
            MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Mother Brain room-palette pointers remain live presentation reads");

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
            "live palette-pointer reads, complete production loop, strict rejection, " +
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
        ISnesAddressSpace
    {
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
