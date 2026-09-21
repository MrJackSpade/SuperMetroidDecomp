using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyBotwoonInstructionProgramDefinitions()
    {
        VerifyBotwoonInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyBotwoonInstructionDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;
        ushort Word(int address) =>
            (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

        var guarded = new BotwoonInstructionReadGuard(rom);
        var animateHead = typeof(RoomEnemySystem).GetMethod(
            "AnimateBotwoonHeadFromMovement", flags)!
            .CreateDelegate<Action<RoomEnemySlot, BotwoonEnemyState>>();
        var aimHead = typeof(RoomEnemySystem).GetMethod("AimBotwoonAtSamus", flags)!
            .CreateDelegate<Action<RoomEnemySystem, RoomEnemySlot, BotwoonEnemyState, SamusState>>();
        var animateBody = typeof(RoomEnemySystem).GetMethod(
            "AnimateBotwoonBodySegment", flags)!
            .CreateDelegate<Action<RoomEnemySystem, RoomEnemyProjectileSlot, byte>>();
        FieldInfo busField = typeof(RoomEnemySystem).GetField("_bus", flags)!;

        (short X, short Y)[] vectors =
        [
            (0, -100),
            (100, -100),
            (100, 0),
            (100, 100),
            (0, 100),
            (-100, 100),
            (-100, 0),
            (-100, -100),
        ];

        for (int octant = 0; octant < vectors.Length; octant++)
        {
            BotwoonHeadInstructionDefinition definition =
                BotwoonInstructionDefinitions.HeadForOctant(octant);
            AssertEqual(Word(0xb3946b + octant * 2), definition.MovementInstruction,
                $"Botwoon head movement octant {octant}");
            AssertEqual(Word(0xb3948b + octant * 2), definition.SpitInstruction,
                $"Botwoon head spit octant {octant}");
            AssertEqual(Word(0xb3947b + octant * 2),
                BotwoonInstructionDefinitions.HiddenHeadInstruction,
                $"Botwoon hidden-head duplicate {octant}");

            (short dx, short dy) = vectors[octant];
            var movementEnemies = new RoomEnemySystem();
            busField.SetValue(movementEnemies, guarded);
            RoomEnemySlot movementHead = movementEnemies.Slots[0];
            var movementState = new BotwoonEnemyState(movementHead);
            movementHead.XPosition = unchecked((ushort)(1000 + dx));
            movementHead.YPosition = unchecked((ushort)(1000 + dy));
            movementState.HeadHistoryX[3] = 1000;
            movementState.HeadHistoryY[3] = 1000;
            animateHead(movementHead, movementState);
            AssertEqual(definition.MovementInstruction, movementHead.CurrentInstruction,
                $"production Botwoon moving-head octant {octant}");

            var spitEnemies = new RoomEnemySystem();
            busField.SetValue(spitEnemies, guarded);
            RoomEnemySlot spitHead = spitEnemies.Slots[0];
            var spitState = new BotwoonEnemyState(spitHead)
            {
                Function = BotwoonEnemyFunction.SpitWhileHidden,
            };
            spitHead.XPosition = 1000;
            spitHead.YPosition = 1000;
            var samus = new SamusState
            {
                XPosition = unchecked((ushort)(1000 + dx)),
                YPosition = unchecked((ushort)(1000 + dy)),
            };
            aimHead(spitEnemies, spitHead, spitState, samus);
            AssertEqual(definition.SpitInstruction, spitHead.CurrentInstruction,
                $"production Botwoon spit-head octant {octant}");
        }

        for (ushort byteOffset = 0; byteOffset < 64; byteOffset += 2)
        {
            ushort expected = Word(0x86e9f1 + byteOffset);
            AssertEqual(expected, BotwoonInstructionDefinitions.BodyInstruction(byteOffset),
                $"Botwoon body selector ${byteOffset:X2}");

            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            var segment = new RoomEnemyProjectileSlot(0)
            {
                DirectionParameter = byteOffset,
                Variable0 = ushort.MaxValue,
            };
            animateBody(enemies, segment, 0);
            AssertEqual(expected, segment.InstructionPointer,
                $"production Botwoon body selector ${byteOffset:X2}");
            AssertEqual(expected, segment.Variable0,
                $"production Botwoon body installed selector ${byteOffset:X2}");
            AssertEqual((ushort)1, segment.InstructionTimer,
                $"production Botwoon body timer ${byteOffset:X2}");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => BotwoonInstructionDefinitions.HeadForOctant(8),
            "Botwoon head octant past definitions");
        AssertThrows<ArgumentOutOfRangeException>(
            () => BotwoonInstructionDefinitions.BodyInstruction(1),
            "Botwoon odd body byte offset");
        AssertThrows<ArgumentOutOfRangeException>(
            () => BotwoonInstructionDefinitions.BodyInstruction(64),
            "Botwoon body byte offset past definitions");

        Console.WriteLine(
            "Botwoon instruction definitions: all 56 native selector words and 48 real head/body selections pass with all source tables forbidden.");
    }

    private static void VerifyBotwoonInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (int index = 0;
             index < BotwoonInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            BotwoonInstructionMechanicsWord definition =
                BotwoonInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadBotwoonInstructionWord(rom, 0xb30000 | definition.Address),
                $"Botwoon instruction mechanics word $B3:{definition.Address:X4}");
        }

        var guard = new BotwoonInstructionReadGuard(rom);
        ushort[] movementPrograms =
        [
            BotwoonInstructionProgramDefinitions.MovingUpLeft,
            BotwoonInstructionProgramDefinitions.MovingLeft,
            BotwoonInstructionProgramDefinitions.MovingDownLeft,
            BotwoonInstructionProgramDefinitions.MovingDown,
            BotwoonInstructionProgramDefinitions.MovingDownRight,
            BotwoonInstructionProgramDefinitions.MovingRight,
            BotwoonInstructionProgramDefinitions.MovingUpRight,
            BotwoonInstructionProgramDefinitions.MovingUp,
        ];
        ushort[] spitPrograms =
        [
            BotwoonInstructionProgramDefinitions.SpittingUpLeft,
            BotwoonInstructionProgramDefinitions.SpittingLeft,
            BotwoonInstructionProgramDefinitions.SpittingDownLeft,
            BotwoonInstructionProgramDefinitions.SpittingDown,
            BotwoonInstructionProgramDefinitions.SpittingDownRight,
            BotwoonInstructionProgramDefinitions.SpittingRight,
            BotwoonInstructionProgramDefinitions.SpittingUpRight,
            BotwoonInstructionProgramDefinitions.SpittingUp,
        ];

        foreach (ushort program in movementPrograms)
        {
            (RoomEnemySystem enemies, RoomEnemySlot head, _) =
                CreateBotwoonProgramSystem(guard, program);
            RunBotwoonProgram(enemies, head, frames: 3);
            AssertTrue(head.XRadius != 0 && head.YRadius != 0,
                $"Botwoon movement program ${program:X4} installs a physical radius");
            AssertEqual(CommonEnemyInstructionCodes.Sleep,
                BotwoonInstructionProgramDefinitions.ReadMechanicsWord(
                    head.CurrentInstruction),
                $"Botwoon movement program ${program:X4} reaches sleep");
        }

        {
            (RoomEnemySystem enemies, RoomEnemySlot head, _) =
                CreateBotwoonProgramSystem(
                    guard,
                    BotwoonInstructionProgramDefinitions.Hidden);
            RunBotwoonProgram(enemies, head, frames: 3);
            AssertEqual(CommonEnemyInstructionCodes.Sleep,
                BotwoonInstructionProgramDefinitions.ReadMechanicsWord(
                    head.CurrentInstruction),
                "Botwoon hidden program reaches sleep");
        }

        foreach (ushort program in spitPrograms)
        {
            (RoomEnemySystem enemies, RoomEnemySlot head, BotwoonEnemyState state) =
                CreateBotwoonProgramSystem(guard, program);
            RunBotwoonProgram(enemies, head, frames: 70);
            AssertTrue(state.SpitFrameReached,
                $"Botwoon spit program ${program:X4} publishes its attack frame");
            AssertEqual((ushort?)0x007c, enemies.LastBotwoonSoundEffect,
                $"Botwoon spit program ${program:X4} publishes its sound");
            AssertTrue(head.XRadius != 0 && head.YRadius != 0,
                $"Botwoon spit program ${program:X4} installs a physical radius");
            AssertEqual(CommonEnemyInstructionCodes.Sleep,
                BotwoonInstructionProgramDefinitions.ReadMechanicsWord(
                    head.CurrentInstruction),
                $"Botwoon spit program ${program:X4} reaches sleep");
        }

        AssertEqual(BotwoonInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all live Botwoon head spritemap words remain cartridge reads");
        for (int index = 0;
             index < BotwoonInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                BotwoonInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Botwoon presentation word $B3:{address:X4}");
            AssertThrows<InvalidDataException>(
                () => BotwoonInstructionProgramDefinitions.ReadMechanicsWord(address),
                $"Botwoon spritemap $B3:{address:X4} is rejected as mechanics");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Botwoon mechanics byte");

        AssertThrows<InvalidDataException>(
            () => BotwoonInstructionProgramDefinitions.ReadMechanicsWord(
                BotwoonInstructionProgramDefinitions.UnusedMovingHorizontal),
            "selector-skipped Botwoon movement program is rejected");
        AssertThrows<InvalidDataException>(
            () => BotwoonInstructionProgramDefinitions.ReadMechanicsWord(
                BotwoonInstructionProgramDefinitions.UnusedSpittingHorizontal),
            "selector-skipped Botwoon spit program is rejected");
        AssertThrows<InvalidDataException>(
            () => BotwoonInstructionProgramDefinitions.ReadMechanicsWord(
                BotwoonInstructionProgramDefinitions.FirstAdjacentProgram),
            "adjacent Botwoon program is rejected as head mechanics");

        _ = ProbeBotwoonInstructionMechanicsAllocation();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeBotwoonInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Botwoon allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - allocatedBefore,
            "warmed Botwoon mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Botwoon head instruction mechanics: 74 compiled words, all seventeen " +
            "selector-reachable programs and 25 live spritemap reads pass with mechanics " +
            "bytes forbidden.");

        static (RoomEnemySystem Enemies, RoomEnemySlot Head, BotwoonEnemyState State)
            CreateBotwoonProgramSystem(
                BotwoonInstructionReadGuard guard,
                ushort program)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            RoomEnemySlot head = enemies.Slots[0];
            head.EnemyDefinitionPointer = RoomEnemySystem.BotwoonDefinition;
            head.Definition = default(RoomEnemyDefinition) with { Bank = 0xb3 };
            head.CurrentInstruction = program;
            head.InstructionTimer = 1;
            var state = new BotwoonEnemyState(head);
            typeof(RoomEnemySystem).GetField("_botwoonState", flags)!
                .SetValue(enemies, state);
            return (enemies, head, state);
        }

        static void RunBotwoonProgram(
            RoomEnemySystem enemies,
            RoomEnemySlot head,
            int frames)
        {
            MethodInfo process =
                typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
            object?[] arguments =
                [head, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < frames; frame++)
                process.Invoke(enemies, arguments);
        }
    }

    private static int ProbeBotwoonInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += BotwoonInstructionProgramDefinitions.ReadMechanicsWord(
                BotwoonInstructionProgramDefinitions.Hidden);
        }
        return checksum;
    }

    private static ushort ReadBotwoonInstructionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class BotwoonInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (BotwoonInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Botwoon mechanics byte ${address:X6}.");
            }
            if (address is >= 0xb3946b and < 0xb3949b or >= 0x86e9f1 and < 0x86ea31)
            {
                throw new InvalidOperationException(
                    $"Botwoon attempted migrated instruction-table read ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xb30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < BotwoonInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        BotwoonInstructionProgramDefinitions.PresentationWordAddress(index);
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
