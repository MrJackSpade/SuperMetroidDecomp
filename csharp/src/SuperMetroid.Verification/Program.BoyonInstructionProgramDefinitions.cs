using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;

internal static partial class Program
{
    private static ushort[] BoyonPresentationOracle() =>
        [0x86ad,0x86b1,0x86b5,0x86b9,0x86c5,0x86c9,0x86cd,0x86d1,0x86d5,0x86d9];

    private static void VerifyBoyonMechanicsMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0x86a7,0x86a9,0x86ab,0x86af,0x86b3,0x86b7,0x86bb,0x86bd,
            0x86bf,0x86c1,0x86c3,0x86c7,0x86cb,0x86cf,0x86d3,0x86d7,0x86db,0x86dd];
        AssertEqual(addresses.Length, BoyonInstructionProgramDefinitions.MechanicsWordCount, "Boyon word count");
        var bytes = new HashSet<int>();
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort address = addresses[index];
            ushort expected = ReadBoyonInstructionWord(rom, 0xa20000 | address);
            var actual = BoyonInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(address, actual.Address, "Boyon native word position");
            AssertEqual(expected, actual.Value, "Boyon native enumerated word");
            AssertEqual(expected, BoyonInstructionProgramDefinitions.ReadMechanicsWord(address), "Boyon direct native word");
            bytes.Add(address);
            bytes.Add(address + 1);
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool expected = bytes.Contains(address);
            AssertEqual(expected, BoyonInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa20000 | address), "Boyon full bank byte ownership");
            AssertEqual(expected, BoyonInstructionProgramDefinitions.IsCompiledMechanicsByte(0x1a20000 | address), "Boyon high-bit alias preserved");
            AssertTrue(!BoyonInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa30000 | address), "Boyon wrong bank rejected");
        }
        var words = addresses.ToHashSet();
        for (int address = 0x86a5; address <= 0x86e1; address++)
            if (!words.Contains((ushort)address))
                AssertThrows<InvalidDataException>(() => BoyonInstructionProgramDefinitions.ReadMechanicsWord((ushort)address),
                    "Boyon presentation, misaligned and adjacent words rejected");
        foreach (ushort address in new ushort[] { 0, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => BoyonInstructionProgramDefinitions.ReadMechanicsWord(address), "Boyon far invalid word");
        foreach (int index in new[] { int.MinValue, -1, 18, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => BoyonInstructionProgramDefinitions.MechanicsWord(index), "Boyon mechanics ordinal bounds");
    }

    private static void VerifyBoyonPresentationAddresses()
    {
        ushort[] addresses = BoyonPresentationOracle();
        AssertEqual(addresses.Length, BoyonInstructionProgramDefinitions.PresentationWordCount, "Boyon visual count");
        for (int index = 0; index < addresses.Length; index++)
            AssertEqual(addresses[index], BoyonInstructionProgramDefinitions.PresentationWordAddress(index), "Boyon original visual position");
        var valid = addresses.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(valid.Contains((ushort)address), BoyonInstructionProgramDefinitions.IsPresentationWord((ushort)address), "Boyon full visual membership");
        foreach (int index in new[] { int.MinValue, -1, 10, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => BoyonInstructionProgramDefinitions.PresentationWordAddress(index), "Boyon presentation ordinal bounds");
    }

    private static void VerifyBoyonVisualSelectors(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = BoyonPresentationOracle();
        foreach (ushort address in addresses)
        {
            ushort expected = ReadBoyonInstructionWord(rom, 0xa20000 | address);
            AssertEqual(expected, EnemySpritemapDefinitions.BoyonFrameAt(address), "Boyon native visual value");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa2, address, out ushort shared), "Boyon shared visual selection");
            AssertEqual(expected, shared, "Boyon shared native visual value");
        }
        var valid = addresses.ToHashSet();
        for (int address = 0x86a5; address <= 0x86e1; address++)
            if (!valid.Contains((ushort)address))
            {
                AssertThrows<InvalidDataException>(() => EnemySpritemapDefinitions.BoyonFrameAt((ushort)address), "Boyon invalid visual operand");
                AssertTrue(!CompiledEnemyVisualSelectors.TryGet(0xa2, (ushort)address, out ushort missing), "Boyon shared hole rejected");
                AssertEqual((ushort)0, missing, "Boyon shared miss clears output");
            }
        foreach (ushort address in new ushort[] { 0, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => EnemySpritemapDefinitions.BoyonFrameAt(address), "Boyon distant visual rejected");
    }
    private static void VerifyBoyonInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyBoyonInstructionProgramDefinitions), () => VerifyBoyonInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    private static void VerifyBoyonInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        Suite(nameof(VerifyBoyonMechanicsMapping), () => VerifyBoyonMechanicsMapping(rom));
        Suite(nameof(VerifyBoyonPresentationAddresses), () => VerifyBoyonPresentationAddresses());
        Suite(nameof(VerifyBoyonVisualSelectors), () => VerifyBoyonVisualSelectors(rom));

        var guard = new BoyonInstructionProgramReadGuard(rom);
        RoomEnemySystem idleSystem = CreateBoyonProgramSystem(guard, out RoomEnemySlot idle);
        AssertEqual(BoyonInstructionProgramDefinitions.Idle, idle.CurrentInstruction,
            "Boyon initializer selects idle program");
        RunBoyonProgram(idleSystem, idle, frames: 50);
        AssertTrue(!idle.Properties.HasAny(EnemyProperties.ProcessOffScreen),
            "Boyon idle program disables off-screen processing");

        RoomEnemySystem bouncingSystem =
            CreateBoyonProgramSystem(guard, out RoomEnemySlot bouncing);
        BoyonEnemyState bouncingState = bouncingSystem.BoyonStates[0]!;
        var samus = new SamusState
        {
            XPosition = bouncing.XPosition,
            YPosition = bouncing.YPosition,
        };
        var runMain = typeof(RoomEnemySystem).GetMethod("RunBoyonMain", flags)!
            .CreateDelegate<Action<RoomEnemySlot, BoyonEnemyState, SamusState?>>();
        runMain(bouncing, bouncingState, samus);
        runMain(bouncing, bouncingState, samus);
        AssertEqual(BoyonInstructionProgramDefinitions.Bouncing, bouncing.CurrentInstruction,
            "Boyon proximity path selects bouncing program");
        RunBoyonProgram(bouncingSystem, bouncing, frames: 40);
        AssertTrue(bouncing.Properties.HasAny(EnemyProperties.ProcessOffScreen),
            "Boyon bouncing program enables off-screen processing");
        AssertEqual((ushort)0x000e, bouncingSystem.LastBoyonSoundEffect!.Value,
            "Boyon bouncing callback publishes native sound");
        AssertTrue(!bouncingState.BounceDisabled,
            "Boyon bouncing callback permits the movement arc");

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Boyon mechanics and visual selector bytes");

        _ = ProbeBoyonInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeBoyonInstructionMechanicsAllocation();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Boyon allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Boyon mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Boyon instruction mechanics: eighteen compiled words, idle and bouncing " +
            "production loops, bounce callback/property changes, and ten compiled " +
            "visual selectors pass with source bytes forbidden.");

        static RoomEnemySystem CreateBoyonProgramSystem(
            BoyonInstructionProgramReadGuard guard,
            out RoomEnemySlot slot)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeBoyon", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.BoyonDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            slot.Parameter1 = 0;
            slot.Parameter2 = 0x0040;
            slot.XPosition = 0x0100;
            slot.YPosition = 0x0200;
            initialize(slot);
            return enemies;
        }

        static void RunBoyonProgram(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
        }
    }

    private static int ProbeBoyonInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += BoyonInstructionProgramDefinitions.ReadMechanicsWord(
                BoyonInstructionProgramDefinitions.Idle);
        }
        return checksum;
    }

    private static ushort ReadBoyonInstructionWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class BoyonInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (BoyonInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                IsCompiledPresentationByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Boyon instruction byte ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        private static bool IsCompiledPresentationByte(int address)
        {
            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < BoyonInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        BoyonInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
