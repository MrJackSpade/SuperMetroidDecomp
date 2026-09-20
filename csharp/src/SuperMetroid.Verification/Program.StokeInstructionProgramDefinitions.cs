using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyStokeInstructionProgramDefinitions()
    {
        VerifyStokeInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyStokeInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (int index = 0;
             index < StokeInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            StokeInstructionMechanicsWord definition =
                StokeInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadStokeInstructionWord(rom, 0xa20000 | definition.Address),
                $"Stoke instruction mechanics word $A2:{definition.Address:X4}");
        }

        var guard = new StokeInstructionProgramReadGuard(rom);
        VerifyWalking(
            StokeInstructionProgramDefinitions.MovingLeft,
            StokeDirection.Left,
            StokeAiFunction.MovingLeft,
            0x8938,
            "left");
        VerifyWalking(
            StokeInstructionProgramDefinitions.MovingRight,
            StokeDirection.Right,
            StokeAiFunction.MovingRight,
            0x895e,
            "right");
        VerifyAttack(
            StokeInstructionProgramDefinitions.AttackingLeft,
            StokeDirection.Left,
            StokeAiFunction.MovingLeft,
            0x8938,
            expectedProjectileDirection: 0,
            direction: "left");
        VerifyAttack(
            StokeInstructionProgramDefinitions.AttackingRight,
            StokeDirection.Right,
            StokeAiFunction.MovingRight,
            0x895e,
            expectedProjectileDirection: 1,
            direction: "right");

        AssertEqual(StokeInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Stoke spritemap words remain cartridge reads");
        for (int index = 0;
             index < StokeInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = StokeInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Stoke presentation word $A2:{address:X4}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Stoke mechanics byte");
        AssertThrows<InvalidDataException>(
            () => StokeInstructionProgramDefinitions.ReadMechanicsWord(0x8936),
            "interleaved Stoke spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => StokeInstructionProgramDefinitions.ReadMechanicsWord(0x897e),
            "adjacent Stoke callback code is rejected as mechanics");

        _ = ProbeStokeInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeStokeInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Stoke allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Stoke mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Stoke instruction mechanics: twenty-six compiled words, both walking " +
            "loops, both attacks and real directional projectile spawns, and twelve " +
            "live spritemap reads pass with mechanics bytes forbidden.");

        void VerifyWalking(
            ushort program,
            StokeDirection expectedDirection,
            StokeAiFunction expectedFunction,
            ushort expectedLoopCursor,
            string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, StokeEnemyState state) =
                CreateStoke(expectedDirection);
            Install(slot, program);
            RunStokeProgram(enemies, slot, 41);
            AssertEqual(expectedDirection, state.Direction,
                $"Stoke {direction} walking callback direction");
            AssertEqual(expectedFunction, state.Function,
                $"Stoke {direction} walking callback function");
            AssertEqual(expectedLoopCursor, slot.CurrentInstruction,
                $"Stoke {direction} walking program returns through native goto");
        }

        void VerifyAttack(
            ushort program,
            StokeDirection expectedDirection,
            StokeAiFunction expectedFunction,
            ushort expectedLoopCursor,
            ushort expectedProjectileDirection,
            string direction)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, StokeEnemyState state) =
                CreateStoke(expectedDirection);
            state.Function = StokeAiFunction.IdleDuringAttack;
            Install(slot, program);
            RunStokeProgram(enemies, slot, 33);

            AssertEqual(expectedDirection, state.Direction,
                $"Stoke {direction} attack restores direction");
            AssertEqual(expectedFunction, state.Function,
                $"Stoke {direction} attack restores walking function");
            AssertEqual(expectedLoopCursor, slot.CurrentInstruction,
                $"Stoke {direction} attack enters walking loop");
            RoomEnemyProjectileSlot projectile = enemies.EnemyProjectiles.Single(
                candidate => candidate.Kind == RoomEnemyProjectileKind.StokeProjectile);
            AssertEqual(expectedProjectileDirection, projectile.DirectionParameter,
                $"Stoke {direction} attack projectile direction");
        }

        (RoomEnemySystem Enemies, RoomEnemySlot Slot, StokeEnemyState State) CreateStoke(
            StokeDirection direction)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeStoke", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.StokeDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
            slot.Parameter1 = (ushort)direction;
            slot.XPosition = 0x0080;
            slot.YPosition = 0x0080;
            initialize(slot);
            return (enemies, slot, enemies.StokeStates[0]!);
        }

        static void Install(RoomEnemySlot slot, ushort program)
        {
            slot.CurrentInstruction = program;
            slot.InstructionTimer = 1;
            slot.Timer = 0;
        }

        static void RunStokeProgram(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            int frames)
        {
            MethodInfo process = typeof(RoomEnemySystem).GetMethod(
                "ProcessInstructions", flags)!;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
        }
    }

    private static int ProbeStokeInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += StokeInstructionProgramDefinitions.ReadMechanicsWord(
                StokeInstructionProgramDefinitions.MovingLeft);
        }
        return checksum;
    }

    private static ushort ReadStokeInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class StokeInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (StokeInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Stoke mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < StokeInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        StokeInstructionProgramDefinitions.PresentationWordAddress(index);
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
