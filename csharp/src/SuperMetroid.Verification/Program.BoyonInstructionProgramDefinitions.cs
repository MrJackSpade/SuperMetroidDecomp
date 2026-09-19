using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyBoyonInstructionProgramDefinitions()
    {
        VerifyBoyonInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyBoyonInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < BoyonInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            BoyonInstructionMechanicsWord definition =
                BoyonInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadBoyonInstructionWord(rom, 0xa20000 | definition.Address),
                $"Boyon instruction mechanics word $A2:{definition.Address:X4}");
        }

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

        AssertEqual(
            BoyonInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Boyon spritemap words remain cartridge reads");
        for (int index = 0;
             index < BoyonInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = BoyonInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Boyon presentation word $A2:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Boyon mechanics byte");

        AssertThrows<InvalidDataException>(
            () => BoyonInstructionProgramDefinitions.ReadMechanicsWord(0x86ad),
            "interleaved Boyon spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => BoyonInstructionProgramDefinitions.ReadMechanicsWord(0x86df),
            "adjacent Boyon definition data is rejected as mechanics");

        _ = ProbeBoyonInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeBoyonInstructionMechanicsAllocation();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Boyon allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Boyon mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Boyon instruction mechanics: eighteen compiled words, idle and bouncing " +
            "production loops, bounce callback/property changes, and ten live " +
            "spritemap reads pass with mechanics bytes forbidden.");

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
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (BoyonInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Boyon mechanics byte ${address:X6}.");
            }

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
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
