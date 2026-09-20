using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyRipperInstructionProgramDefinitions()
    {
        VerifyRipperInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyRipperInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < RipperInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            RipperInstructionMechanicsWord definition =
                RipperInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadRipperInstructionWord(rom, 0xa20000 | definition.Address),
                $"Ripper-family instruction mechanics word $A2:{definition.Address:X4}");
        }

        var guard = new RipperInstructionProgramReadGuard(rom);
        VerifyGRipper(1,
            RipperInstructionProgramDefinitions.GRipperMovingRight,
            "GRipper right");
        VerifyGRipperReversal();
        VerifyRipper2(0,
            RipperInstructionProgramDefinitions.Ripper2MovingRight,
            "Ripper II native-right label");
        VerifyRipper2(1,
            RipperInstructionProgramDefinitions.Ripper2MovingLeft,
            "Ripper II native-left label");
        VerifyRipper(1,
            RipperInstructionProgramDefinitions.RipperMovingRight,
            "Ripper right");
        VerifyRipper(0,
            RipperInstructionProgramDefinitions.RipperMovingLeft,
            "Ripper left");

        AssertEqual(RipperInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Ripper-family spritemap words remain cartridge reads");
        for (int index = 0;
             index < RipperInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                RipperInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Ripper-family presentation $A2:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Ripper-family mechanics byte");
        AssertThrows<InvalidDataException>(
            () => RipperInstructionProgramDefinitions.ReadMechanicsWord(0xe19d),
            "interleaved GRipper spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => RipperInstructionProgramDefinitions.ReadMechanicsWord(0xe1c3),
            "unused frozen GRipper program is rejected as production mechanics");
        AssertThrows<InvalidDataException>(
            () => RipperInstructionProgramDefinitions.ReadMechanicsWord(0xe49f),
            "adjacent Ripper initializer code is rejected as mechanics");

        _ = ProbeRipperInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeRipperInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Ripper-family allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Ripper-family mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Ripper-family instruction mechanics: thirty-six compiled words, all six " +
            "production-installed animation loops, and twenty-four live spritemap reads " +
            "pass with mechanics bytes forbidden.");

        void VerifyGRipper(ushort packedSelector, ushort expectedProgram, string context)
        {
            RoomEnemySystem enemies = CreateSystem();
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeGRipper", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.GRipperDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            slot.CurrentInstruction = packedSelector;
            slot.Parameter1 = 0;
            slot.Parameter2 = ushort.MaxValue;
            initialize(slot);
            VerifyInstalledLoop(enemies, slot, expectedProgram, context);
        }

        void VerifyGRipperReversal()
        {
            RoomEnemySystem enemies = CreateSystem();
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeGRipper", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            MethodInfo reverse = typeof(RoomEnemySystem).GetMethod(
                "ReverseRipperVariant", flags)!;
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.GRipperDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            slot.CurrentInstruction = 1;
            slot.Parameter1 = 0;
            slot.Parameter2 = ushort.MaxValue;
            initialize(slot);
            reverse.Invoke(null,
            [
                slot,
                RipperInstructionProgramDefinitions.GRipperMovingRight,
                RipperInstructionProgramDefinitions.GRipperMovingLeft,
            ]);
            VerifyInstalledLoop(
                enemies,
                slot,
                RipperInstructionProgramDefinitions.GRipperMovingLeft,
                "GRipper left reversal");
        }

        void VerifyRipper2(ushort initialDirection, ushort expectedProgram, string context)
        {
            RoomEnemySystem enemies = CreateSystem();
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeRipper2", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.Ripper2Definition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            slot.Parameter1 = 1;
            slot.Parameter2 = initialDirection;
            initialize(slot);
            VerifyInstalledLoop(enemies, slot, expectedProgram, context);
        }

        void VerifyRipper(ushort initialDirection, ushort expectedProgram, string context)
        {
            RoomEnemySystem enemies = CreateSystem();
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeRipper", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>();
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.RipperDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            slot.Parameter1 = 1;
            slot.Parameter2 = initialDirection;
            initialize(slot);
            VerifyInstalledLoop(enemies, slot, expectedProgram, context);
        }

        RoomEnemySystem CreateSystem()
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            return enemies;
        }

        static void VerifyInstalledLoop(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            ushort expectedProgram,
            string context)
        {
            AssertEqual(expectedProgram, slot.CurrentInstruction,
                $"{context} initializer-selected program");
            slot.InstructionTimer = 1;
            slot.Timer = 0;
            MethodInfo process = typeof(RoomEnemySystem).GetMethod(
                "ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < 31; frame++)
                process.Invoke(enemies, arguments);
            AssertEqual(unchecked((ushort)(expectedProgram + 4)), slot.CurrentInstruction,
                $"{context} completes 8/7/8/7 loop and native goto");
        }
    }

    private static int ProbeRipperInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += RipperInstructionProgramDefinitions.ReadMechanicsWord(
                RipperInstructionProgramDefinitions.RipperMovingRight);
        }
        return checksum;
    }

    private static ushort ReadRipperInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class RipperInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (RipperInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Ripper-family mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < RipperInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        RipperInstructionProgramDefinitions.PresentationWordAddress(index);
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
