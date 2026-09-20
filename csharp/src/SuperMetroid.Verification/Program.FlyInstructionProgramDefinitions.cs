using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyFlyInstructionProgramDefinitions()
    {
        VerifyFlyInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyFlyInstructionProgramDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < FlyInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            FlyInstructionMechanicsWord definition =
                FlyInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadFlyInstructionWord(rom, 0xa20000 | definition.Address),
                $"fly-family instruction mechanics word $A2:{definition.Address:X4}");
        }

        var guard = new FlyInstructionProgramReadGuard(rom);
        foreach (ushort definition in new ushort[]
                 {
                     RoomEnemySystem.MellowDefinition,
                     RoomEnemySystem.MellaDefinition,
                     RoomEnemySystem.MemuDefinition,
                 })
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeFly", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = definition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            initialize(slot);
            AssertEqual(FlyInstructionProgramDefinitions.Flight, slot.CurrentInstruction,
                $"fly ${definition:X4} initializer program");

            MethodInfo process = typeof(RoomEnemySystem).GetMethod(
                "ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < 9; frame++)
                process.Invoke(enemies, arguments);
            AssertEqual(unchecked((ushort)(FlyInstructionProgramDefinitions.Flight + 4)),
                slot.CurrentInstruction,
                $"fly ${definition:X4} completes native animation loop");
        }

        AssertEqual(FlyInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live fly-family spritemap words remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled fly-family mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => FlyInstructionProgramDefinitions.ReadMechanicsWord(0xb015),
            "fly-family spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => FlyInstructionProgramDefinitions.ReadMechanicsWord(0xb027),
            "adjacent unused movement data is rejected as mechanics");

        _ = ProbeFlyInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeFlyInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "fly-family allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed fly-family mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Fly-family instruction mechanics: six compiled words, all three real " +
            "initializers, the complete loop, and four live spritemap reads pass with " +
            "mechanics bytes forbidden.");
    }

    private static int ProbeFlyInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
            checksum += FlyInstructionProgramDefinitions.ReadMechanicsWord(
                FlyInstructionProgramDefinitions.Flight);
        return checksum;
    }

    private static ushort ReadFlyInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class FlyInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (FlyInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled fly-family mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < FlyInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        FlyInstructionProgramDefinitions.PresentationWordAddress(index);
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
