using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyHorizontalShutterInstructionProgramDefinitions()
    {
        VerifyHorizontalShutterInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyHorizontalShutterInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        VerifyHorizontalShutterMechanicsMapping(rom);
        VerifyHorizontalShutterPresentationMapping();

        var guard = new HorizontalShutterInstructionProgramReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type enemySystemType = typeof(RoomEnemySystem);
        enemySystemType.GetField("_bus", flags)!.SetValue(enemies, guard);
        MethodInfo initialize = enemySystemType.GetMethod(
            "InitializeHorizontalShutter", flags)!;
        MethodInfo process = enemySystemType.GetMethod("ProcessInstructions", flags)!;
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.ShootableHorizontalShutterDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        slot.CurrentInstruction = 0x0100;
        slot.ExtraProperties = 0x0101;
        slot.Parameter1 = 0x1002;
        slot.Parameter2 = 24;
        initialize.Invoke(enemies, [slot, null]);
        AssertEqual(HorizontalShutterInstructionProgramDefinitions.Stationary,
            slot.CurrentInstruction,
            "horizontal-shutter real initializer program");

        object?[] processArguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        process.Invoke(enemies, processArguments);
        AssertEqual(unchecked((ushort)(
                HorizontalShutterInstructionProgramDefinitions.Stationary + 4)),
            slot.CurrentInstruction,
            "horizontal-shutter timed frame advances to terminal sleep");
        AssertEqual((ushort)1, slot.InstructionTimer,
            "horizontal-shutter stationary frame duration");
        process.Invoke(enemies, processArguments);
        AssertEqual(unchecked((ushort)(
                HorizontalShutterInstructionProgramDefinitions.Stationary + 4)),
            slot.CurrentInstruction,
            "horizontal-shutter terminal sleep remains installed");

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "horizontal-shutter spritemap word uses a compiled visual selector");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled horizontal-shutter mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => HorizontalShutterInstructionProgramDefinitions.ReadMechanicsWord(0xe9d6),
            "horizontal-shutter spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => HorizontalShutterInstructionProgramDefinitions.ReadMechanicsWord(0xe9da),
            "adjacent growing-shutter initializer is rejected as mechanics");

        _ = ProbeHorizontalShutterInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeHorizontalShutterInstructionMechanicsAllocation();
        AssertTrue(checksum != 0,
            "horizontal-shutter allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed horizontal-shutter mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Horizontal-shutter instruction mechanics: two compiled words, the real " +
            "initializer, terminal sleep, and one compiled visual selector pass with mechanics " +
            "bytes forbidden.");
    }

    private static void VerifyHorizontalShutterMechanicsMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0xe9d4, 0xe9d8];
        AssertEqual(addresses.Length, HorizontalShutterInstructionProgramDefinitions.MechanicsWordCount, "Horizontal shutter native mechanics count");
        var bytes = new HashSet<int>();
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort address = addresses[index];
            ushort native = ReadHorizontalShutterInstructionWord(rom, 0xa20000 | address);
            var word = HorizontalShutterInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(address, word.Address, "Horizontal shutter native word address");
            AssertEqual(native, word.Value, "Horizontal shutter enumerated native word");
            AssertEqual(native, HorizontalShutterInstructionProgramDefinitions.ReadMechanicsWord(address), "Horizontal shutter direct native word");
            bytes.Add(address);
            bytes.Add(address + 1);
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            AssertEqual(bytes.Contains(address), HorizontalShutterInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa20000 | address), "Horizontal shutter full byte ownership");
            AssertEqual(bytes.Contains(address), HorizontalShutterInstructionProgramDefinitions.IsCompiledMechanicsByte(0x1a20000 | address), "Horizontal shutter bank mask aliases");
            AssertTrue(!HorizontalShutterInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa30000 | address), "Horizontal shutter rejects other bank");
        }
        var words = addresses.ToHashSet();
        for (int address = addresses[0] - 2; address <= addresses[^1] + 4; address++)
            if (!words.Contains((ushort)address))
                AssertThrows<InvalidDataException>(() => HorizontalShutterInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "Horizontal shutter rejects visual words, odd addresses and adjacent programs");
        foreach (int index in new[] { int.MinValue, -1, addresses.Length, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => HorizontalShutterInstructionProgramDefinitions.MechanicsWord(index), "Horizontal shutter mechanics bounds");
    }

    private static void VerifyHorizontalShutterPresentationMapping()
    {
        ushort[] addresses = [0xe9d6];
        AssertEqual(addresses.Length, HorizontalShutterInstructionProgramDefinitions.PresentationWordCount, "Horizontal shutter visual count");
        for (int index = 0; index < addresses.Length; index++)
            AssertEqual(addresses[index], HorizontalShutterInstructionProgramDefinitions.PresentationWordAddress(index), "Horizontal shutter native visual position");
        var expected = addresses.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(expected.Contains((ushort)address), HorizontalShutterInstructionProgramDefinitions.IsPresentationWord((ushort)address), "Horizontal shutter full visual membership domain");
        foreach (int index in new[] { int.MinValue, -1, addresses.Length, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => HorizontalShutterInstructionProgramDefinitions.PresentationWordAddress(index), "Horizontal shutter visual bounds");
    }
    private static int ProbeHorizontalShutterInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += HorizontalShutterInstructionProgramDefinitions.ReadMechanicsWord(
                HorizontalShutterInstructionProgramDefinitions.Stationary);
        }
        return checksum;
    }

    private static ushort ReadHorizontalShutterInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class HorizontalShutterInstructionProgramReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (HorizontalShutterInstructionProgramDefinitions.IsCompiledMechanicsByte(
                address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled horizontal-shutter mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort presentation =
                    HorizontalShutterInstructionProgramDefinitions.PresentationWordAddress(0);
                ushort bankAddress = unchecked((ushort)address);
                if (bankAddress == presentation ||
                    bankAddress == unchecked((ushort)(presentation + 1)))
                {
                    ObservedPresentationWords.Add(presentation);
                }
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
