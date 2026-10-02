using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyVerticalShutterInstructionProgramDefinitions()
    {
        VerifyVerticalShutterInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyVerticalShutterInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        VerifyVerticalShutterMechanicsMapping(rom);
        VerifyVerticalShutterPresentationAddresses();
        VerifyKamerPlatformVisualSelectors(rom);

        var guard = new VerticalShutterInstructionProgramReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type enemySystemType = typeof(RoomEnemySystem);
        enemySystemType.GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = enemySystemType.GetMethod("InitializeVerticalShutter", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = enemySystemType.GetMethod("ProcessInstructions", flags)!;
        object?[] processArguments =
            [null, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];

        foreach (ushort definitionPointer in new ushort[]
                 {
                     RoomEnemySystem.ShootableVerticalShutterDefinition,
                     RoomEnemySystem.DestroyableVerticalShutterDefinition,
                 })
        {
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = definitionPointer;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            slot.CurrentInstruction = 0;
            slot.Parameter1 = 0x0002;
            initialize(slot);
            AssertEqual(VerticalShutterInstructionProgramDefinitions.Plain,
                slot.CurrentInstruction,
                $"vertical shutter ${definitionPointer:X4} installs plain program");
            processArguments[0] = slot;
            process.Invoke(enemies, processArguments);
            process.Invoke(enemies, processArguments);
            AssertEqual(unchecked((ushort)(
                    VerticalShutterInstructionProgramDefinitions.Plain + 4)),
                slot.CurrentInstruction,
                $"vertical shutter ${definitionPointer:X4} reaches terminal sleep");
        }

        RoomEnemySlot kamer = enemies.Slots[0];
        kamer.EnemyDefinitionPointer = RoomEnemySystem.KamerVerticalPlatformDefinition;
        kamer.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        kamer.CurrentInstruction = 0;
        kamer.Parameter1 = 0x0002;
        initialize(kamer);
        AssertEqual(VerticalShutterInstructionProgramDefinitions.KamerPlatform,
            kamer.CurrentInstruction,
            "Kamer vertical platform installs compiled loop");
        processArguments[0] = kamer;
        for (int frame = 0; frame < 5; frame++)
        {
            kamer.InstructionTimer = 1;
            process.Invoke(enemies, processArguments);
        }
        AssertEqual(unchecked((ushort)(
                VerticalShutterInstructionProgramDefinitions.KamerPlatform + 4)),
            kamer.CurrentInstruction,
            "Kamer vertical platform loops to its first timed frame");

        // Both Kamer and the plain shutters use compiled selectors; gameplay
        // timing still comes from their separately compiled instruction words.
        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "plain vertical-shutter selector uses compiled presentation data");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled vertical-shutter mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => VerticalShutterInstructionProgramDefinitions.ReadMechanicsWord(0xede9),
            "vertical-shutter spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => VerticalShutterInstructionProgramDefinitions.ReadMechanicsWord(0xedfb),
            "adjacent vertical-shutter initializer code is rejected as mechanics");

        _ = ProbeVerticalShutterInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeVerticalShutterInstructionMechanicsAllocation();
        AssertTrue(checksum != 0,
            "vertical-shutter allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed vertical-shutter mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Vertical-shutter instruction mechanics: eight compiled words, all three " +
            "real initializers, the Kamer loop, and one compiled plain-shutter selector pass with " +
            "mechanics bytes forbidden.");
    }

    private static void VerifyVerticalShutterMechanicsMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0xe9aa, 0xe9ae, 0xede7, 0xedeb, 0xedef, 0xedf3, 0xedf7, 0xedf9];
        AssertEqual(addresses.Length, VerticalShutterInstructionProgramDefinitions.MechanicsWordCount,
            "vertical shutter independent native word count");
        var bytes = new HashSet<int>();
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort address = addresses[index];
            ushort native = ReadVerticalShutterInstructionWord(rom, 0xa20000 | address);
            var word = VerticalShutterInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(address, word.Address, "vertical shutter native word address");
            AssertEqual(native, word.Value, "vertical shutter native enumerated word");
            AssertEqual(native, VerticalShutterInstructionProgramDefinitions.ReadMechanicsWord(address),
                "vertical shutter native direct word");
            bytes.Add(address);
            bytes.Add(address + 1);
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            AssertEqual(bytes.Contains(address), VerticalShutterInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa20000 | address),
                "vertical shutter full byte ownership");
            AssertEqual(bytes.Contains(address), VerticalShutterInstructionProgramDefinitions.IsCompiledMechanicsByte(0x1a20000 | address),
                "vertical shutter high-bit aliases");
            AssertTrue(!VerticalShutterInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa30000 | address),
                "vertical shutter other-bank rejection");
        }
        var words = addresses.ToHashSet();
        foreach (int start in new[] { 0xe9a8, 0xede5 })
            for (int address = start; address <= start + 24; address++)
                if (!words.Contains((ushort)address))
                    AssertThrows<InvalidDataException>(() => VerticalShutterInstructionProgramDefinitions.ReadMechanicsWord((ushort)address),
                        "vertical shutter rejects visual operands, odd addresses and neighboring instructions");
        foreach (int index in new[] { int.MinValue, -1, 8, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => VerticalShutterInstructionProgramDefinitions.MechanicsWord(index),
                "vertical shutter word ordinal bounds");
    }

    private static void VerifyVerticalShutterPresentationAddresses()
    {
        ushort[] addresses = [0xe9ac, 0xede9, 0xeded, 0xedf1, 0xedf5];
        AssertEqual(addresses.Length, VerticalShutterInstructionProgramDefinitions.PresentationWordCount,
            "vertical shutter independent presentation count");
        for (int index = 0; index < addresses.Length; index++)
            AssertEqual(addresses[index], VerticalShutterInstructionProgramDefinitions.PresentationWordAddress(index),
                "vertical shutter native presentation position");
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            AssertEqual(address == 0xe9ac, VerticalShutterInstructionProgramDefinitions.IsPlainShutterPresentationWord((ushort)address),
                "plain shutter exact presentation domain");
            AssertEqual(address is 0xede9 or 0xeded or 0xedf1 or 0xedf5,
                VerticalShutterInstructionProgramDefinitions.IsKamerPresentationWord((ushort)address),
                "Kamer exact presentation domain");
        }
        foreach (int index in new[] { int.MinValue, -1, 5, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => VerticalShutterInstructionProgramDefinitions.PresentationWordAddress(index),
                "vertical shutter presentation ordinal bounds");
    }

    private static void VerifyKamerPlatformVisualSelectors(SuperMetroidAddressSpace rom)
    {
        ushort[] operands = [0xede9, 0xeded, 0xedf1, 0xedf5];
        foreach (ushort operand in operands)
        {
            ushort native = ReadVerticalShutterInstructionWord(rom, 0xa20000 | operand);
            AssertEqual(native, EnemySpritemapDefinitions.KamerPlatformFrameAt(operand), "Kamer native visual pointer");
            AssertEqual((ushort)2, ReadVerticalShutterInstructionWord(rom, 0xa20000 | native), "Kamer native two-sprite map");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa2, operand, out ushort shared), "Kamer shared selector exists");
            AssertEqual(native, shared, "Kamer shared selector value");
            AssertTrue(CompiledEnemyVisualSelectors.IsCalculatedSelector(0xa20000 | operand), "Kamer excluded from literal regeneration");
        }
        var known = operands.ToHashSet();
        for (int address = 0xede5; address <= 0xedfc; address++)
            if (!known.Contains((ushort)address))
            {
                AssertThrows<InvalidDataException>(() => EnemySpritemapDefinitions.KamerPlatformFrameAt((ushort)address),
                    "Kamer visual resolver rejects controls and neighboring data");
                AssertTrue(!CompiledEnemyVisualSelectors.TryGet(0xa2, (ushort)address, out ushort missing), "Kamer shared holes rejected");
                AssertEqual((ushort)0, missing, "Kamer shared missing value cleared");
            }
    }
    private static void VerifyVerticalShutterInitialFunctionSelection(SuperMetroidAddressSpace rom)
    {
        var select = typeof(RoomEnemySystem).GetMethod("SelectInitialVerticalShutterFunction",
            BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Func<VerticalShutterEnemyState, VerticalShutterFunction>>();
        var state = new VerticalShutterEnemyState(new RoomEnemySlot(0))
        {
            Function = VerticalShutterFunction.Initial,
        };
        for (ushort offset = 0; offset <= 8; offset += 2)
        {
            state.InitialFunctionTableOffset = offset;
            ushort native = ReadVerticalShutterInstructionWord(rom, 0xa2edfb + offset);
            AssertEqual(native, (ushort)select(state), "vertical shutter native initial function pointer");
            AssertEqual(VerticalShutterFunction.Initial, state.Function,
                "vertical initial dispatch selects a function without installing it");
        }
        foreach (ushort offset in new ushort[] { 1, 3, 5, 7, 9, 10, 0x100, 0xfffe, 0xffff })
        {
            state.InitialFunctionTableOffset = offset;
            AssertThrows<InvalidDataException>(() => select(state), "vertical shutter invalid initial offset");
            AssertEqual(VerticalShutterFunction.Initial, state.Function, "invalid vertical selector preserves state");
        }
    }

    private static void VerifyHorizontalShutterInitialFunctionSelection(SuperMetroidAddressSpace rom)
    {
        var select = typeof(RoomEnemySystem).GetMethod("SelectInitialHorizontalShutterFunction",
            BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<HorizontalShutterEnemyState>>();
        var state = new HorizontalShutterEnemyState(new RoomEnemySlot(0));
        for (ushort offset = 0; offset <= 8; offset += 2)
        {
            state.Function = HorizontalShutterFunction.Initial;
            state.InitialFunctionTableOffset = offset;
            ushort native = ReadVerticalShutterInstructionWord(rom, 0xa2f107 + offset);
            select(state);
            AssertEqual(native, (ushort)state.Function, "horizontal shutter installs native initial function pointer");
        }
        foreach (ushort offset in new ushort[] { 1, 3, 5, 7, 9, 10, 0x100, 0xfffe, 0xffff })
        {
            state.Function = HorizontalShutterFunction.Initial;
            state.InitialFunctionTableOffset = offset;
            AssertThrows<InvalidDataException>(() => select(state), "horizontal shutter invalid initial offset");
            AssertEqual(HorizontalShutterFunction.Initial, state.Function, "invalid horizontal selector preserves state");
        }
    }
    private static int ProbeVerticalShutterInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += VerticalShutterInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? VerticalShutterInstructionProgramDefinitions.Plain
                    : VerticalShutterInstructionProgramDefinitions.KamerPlatform);
        }
        return checksum;
    }

    private static ushort ReadVerticalShutterInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class VerticalShutterInstructionProgramReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (VerticalShutterInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled vertical-shutter mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < VerticalShutterInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        VerticalShutterInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                    }
                }
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
