using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyWalkingSpacePirateInstructionProgramDefinitions()
    {
        VerifyWalkingSpacePirateInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyWalkingSpacePirateInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < WalkingSpacePirateInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            WalkingSpacePirateInstructionMechanicsWord definition =
                WalkingSpacePirateInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadWalkingPirateWord(rom, 0xb20000 | definition.Address),
                $"walking Pirate mechanics word $B2:{definition.Address:X4}");
        }

        var guard = new WalkingSpacePirateInstructionReadGuard(rom);
        VerifyProgram(
            WalkingSpacePirateInstructionProgramDefinitions.WalkingLeft,
            frames: 81,
            expectedCursor: 0xfb6c,
            expectedFunction: WalkingSpacePirateFunction.WalkingLeft);
        VerifyProgram(
            WalkingSpacePirateInstructionProgramDefinitions.WalkingRight,
            frames: 81,
            expectedCursor: 0xfbee,
            expectedFunction: WalkingSpacePirateFunction.WalkingRight);
        VerifyProgram(
            WalkingSpacePirateInstructionProgramDefinitions.FlinchFacingLeft,
            frames: 17,
            expectedCursor: 0xfb6c,
            expectedFunction: WalkingSpacePirateFunction.WalkingLeft);
        VerifyProgram(
            WalkingSpacePirateInstructionProgramDefinitions.FlinchFacingRight,
            frames: 17,
            expectedCursor: 0xfbee,
            expectedFunction: WalkingSpacePirateFunction.WalkingRight);
        VerifyProgram(
            WalkingSpacePirateInstructionProgramDefinitions.LookingFacingLeft,
            frames: 125,
            expectedCursor: 0xfbee,
            expectedFunction: WalkingSpacePirateFunction.WalkingRight);
        VerifyProgram(
            WalkingSpacePirateInstructionProgramDefinitions.LookingFacingRight,
            frames: 125,
            expectedCursor: 0xfb6c,
            expectedFunction: WalkingSpacePirateFunction.WalkingLeft);
        VerifyAttack(movingRight: false);
        VerifyAttack(movingRight: true);

        AssertEqual(
            WalkingSpacePirateInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all walking Pirate spritemap words remain live cartridge reads");
        for (int index = 0;
             index < WalkingSpacePirateInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                WalkingSpacePirateInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads walking Pirate presentation $B2:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled walking Pirate mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => WalkingSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(0xfb52),
            "walking Pirate spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => WalkingSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(0xfc68),
            "adjacent walking Pirate callback code is rejected as mechanics");

        _ = ProbeWalkingSpacePirateInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeWalkingSpacePirateInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "walking Pirate allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed walking Pirate mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Walking Space Pirate instruction mechanics: 92 compiled words, all eight " +
            "production programs, both three-laser attacks, and fifty live spritemap " +
            "reads pass with mechanics bytes forbidden.");

        void VerifyAttack(bool movingRight)
        {
            ushort program = movingRight
                ? WalkingSpacePirateInstructionProgramDefinitions.FireLasersRight
                : WalkingSpacePirateInstructionProgramDefinitions.FireLasersLeft;
            ushort expectedCursor = movingRight ? (ushort)0xfc16 : (ushort)0xfb94;
            (RoomEnemySystem enemies, RoomEnemySlot slot,
                WalkingSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            samus.XPosition = movingRight ? (ushort)0x0180 : (ushort)0x0080;
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            RunFrames(enemies, slot, samus, 113);
            AssertEqual(expectedCursor, slot.CurrentInstruction,
                $"walking Pirate {(movingRight ? "right" : "left")} attack loops");
            AssertEqual(3, state.SpawnedLaserCount,
                $"walking Pirate {(movingRight ? "right" : "left")} laser count");
            AssertEqual((ushort)0x0067, enemies.LastSpacePirateSoundEffect!.Value,
                $"walking Pirate {(movingRight ? "right" : "left")} laser sound");
        }

        void VerifyProgram(
            ushort program,
            int frames,
            ushort expectedCursor,
            WalkingSpacePirateFunction expectedFunction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot,
                WalkingSpacePirateEnemyState state, SamusState samus) = CreateSystem();
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            RunFrames(enemies, slot, samus, frames);
            AssertEqual(expectedCursor, slot.CurrentInstruction,
                $"walking Pirate program $B2:{program:X4} cursor");
            AssertEqual(expectedFunction, state.Function,
                $"walking Pirate program $B2:{program:X4} function");
        }

        (RoomEnemySystem Enemies, RoomEnemySlot Slot,
            WalkingSpacePirateEnemyState State, SamusState Samus) CreateSystem()
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.GreyWalkingSpacePirateDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xb2 };
            slot.XPosition = 0x0100;
            slot.YPosition = 0x0100;
            typeof(RoomEnemySystem).GetMethod("InitializeWalkingSpacePirate", flags)!
                .Invoke(enemies, [slot]);
            var samus = new SamusState
            {
                XPosition = 0x0080,
                YPosition = 0x0100,
            };
            return (enemies, slot, enemies.WalkingSpacePirateStates[0]!, samus);
        }

        static void RunFrames(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            SamusState samus,
            int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod(
                "ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, samus, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
        }
    }

    private static int ProbeWalkingSpacePirateInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += WalkingSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(
                WalkingSpacePirateInstructionProgramDefinitions.WalkingLeft);
        }
        return checksum;
    }

    private static ushort ReadWalkingPirateWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class WalkingSpacePirateInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (WalkingSpacePirateInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled walking Pirate mechanics ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xb20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < WalkingSpacePirateInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        WalkingSpacePirateInstructionProgramDefinitions.PresentationWordAddress(index);
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
