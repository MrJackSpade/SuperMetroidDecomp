using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyBotwoonInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyBotwoonInstructionProgramDefinitions), () => VerifyBotwoonInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    private static void VerifyBotwoonInstructionDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;
        ushort Word(int address) =>
            (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

        Suite(nameof(VerifyBotwoonHeadMovementSelection), () => VerifyBotwoonHeadMovementSelection(rom));
        Suite(nameof(VerifyBotwoonHeadSpitSelection), () => VerifyBotwoonHeadSpitSelection(rom));
        Suite(nameof(VerifyBotwoonBodyTailSelection), () => VerifyBotwoonBodyTailSelection(rom));

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
             index < BotwoonInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                BotwoonInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadBotwoonInstructionWord(rom, 0xb30000 | definition.Address),
                $"Botwoon instruction mechanics word $B3:{definition.Address:X4}");
        }

        Suite(nameof(VerifyBotwoonControlMapping), () => VerifyBotwoonControlMapping(rom));
        Suite(nameof(VerifyBotwoonOperandMapping), () => VerifyBotwoonOperandMapping());
        Suite(nameof(VerifyBotwoonVisualMapping), () => VerifyBotwoonVisualMapping(rom));
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

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "all live Botwoon head spritemap selectors use compiled presentation words");
        for (int index = 0;
             index < BotwoonInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                BotwoonInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.BotwoonDefinition, address, out ushort frame),
                $"production execution resolves Botwoon presentation word $B3:{address:X4}");
            AssertEqual(ReadBotwoonInstructionWord(rom, 0xb30000 | address), frame,
                $"compiled Botwoon presentation word $B3:{address:X4} matches ROM");
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
            "selector-reachable programs and 25 calculated spritemap selectors pass with mechanics " +
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
                [head, null, null, (ushort)0, (ushort)0, (ushort)0];
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
        ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (BotwoonInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address))
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
    private static void VerifyBotwoonControlMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] movement = [0x9341,0x9349,0x9351,0x9361,0x9369,0x9371,0x9379,0x9381];
        ushort[] spit = [0x939f,0x93af,0x93bf,0x93df,0x93ef,0x93ff,0x940f,0x941f];
        var addresses = new List<ushort>();
        foreach (ushort start in movement)
            foreach (int offset in new[] {0,2,6}) addresses.Add((ushort)(start + offset));
        addresses.Add(0x9389); addresses.Add(0x938d);
        foreach (ushort start in spit)
            foreach (int offset in new[] {0,4,6,8,10,14}) addresses.Add((ushort)(start + offset));
        AssertEqual(addresses.Count, BotwoonInstructionProgramDefinitionsTooling.MechanicsWordCount, "Botwoon native control count");
        var bytes = new HashSet<int>();
        for (int i = 0; i < addresses.Count; i++)
        {
            ushort address = addresses[i];
            var actual = BotwoonInstructionProgramDefinitionsTooling.MechanicsWord(i);
            ushort native = ReadBotwoonInstructionWord(rom, 0xb30000 | address);
            AssertEqual(address, actual.Address, "Botwoon native control order");
            AssertEqual(native, actual.Value, "Botwoon enumerated control including distinct radius callbacks and left-spit hold");
            AssertEqual(native, BotwoonInstructionProgramDefinitions.ReadMechanicsWord(address), "Botwoon direct native control");
            bytes.Add(address); bytes.Add(address + 1);
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            AssertEqual(bytes.Contains(address), BotwoonInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xb30000 | address), "Botwoon full byte ownership");
            AssertEqual(bytes.Contains(address), BotwoonInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x1b30000 | address), "Botwoon existing high-bit alias");
            AssertTrue(!BotwoonInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xb40000 | address), "Botwoon other bank rejected");
        }
        var words = addresses.ToHashSet();
        for (int address = 0x933f; address <= 0x9431; address++)
            if (!words.Contains((ushort)address))
                AssertThrows<InvalidDataException>(() => BotwoonInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "Botwoon rejects visual operands, odd bytes, unused gaps and adjacent programs");
        foreach (ushort address in new ushort[] {0,0x7fff,0xffff})
            AssertThrows<InvalidDataException>(() => BotwoonInstructionProgramDefinitions.ReadMechanicsWord(address), "Botwoon distant invalid word");
        foreach (int index in new[] {int.MinValue,-1,74,int.MaxValue})
            AssertThrows<IndexOutOfRangeException>(() => BotwoonInstructionProgramDefinitionsTooling.MechanicsWord(index), "Botwoon mechanics ordinal bounds");
    }
    private static void VerifyBotwoonHeadMovementSelection(SuperMetroidAddressSpace rom)
    {
        for (int angle = 0; angle <= byte.MaxValue; angle++)
            AssertEqual(ReadBotwoonInstructionWord(rom, 0xb3946b + 2 * (angle / 32)),
                BotwoonInstructionDefinitions.HeadMovementInstruction((byte)angle), "Botwoon movement all byte angles");
        foreach (int octant in new[] {int.MinValue,-1,8,int.MaxValue})
            AssertThrows<ArgumentOutOfRangeException>(() => BotwoonInstructionDefinitions.HeadForOctant(octant), "Botwoon invalid octant domain");
    }

    private static void VerifyBotwoonHeadSpitSelection(SuperMetroidAddressSpace rom)
    {
        for (int angle = 0; angle <= byte.MaxValue; angle++)
            AssertEqual(ReadBotwoonInstructionWord(rom, 0xb3948b + 2 * (((angle + 16) % 256) / 32)),
                BotwoonInstructionDefinitions.HeadSpitInstruction((byte)angle), "Botwoon spit bias and byte wrap for all angles");
    }

    private static void VerifyBotwoonBodyTailSelection(SuperMetroidAddressSpace rom)
    {
        for (int offset = 0; offset <= ushort.MaxValue; offset++)
            if (offset < 64 && offset % 2 == 0)
                AssertEqual(ReadBotwoonInstructionWord(rom, 0x86e9f1 + offset),
                    BotwoonInstructionDefinitions.BodyInstruction((ushort)offset), "Botwoon visible/hidden body/tail selectors");
            else
                AssertThrows<ArgumentOutOfRangeException>(() => BotwoonInstructionDefinitions.BodyInstruction((ushort)offset), "Botwoon body selector rejects odd and high offsets");
    }

    private static void VerifyBotwoonVisualMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] operands = [0x9345,0x934d,0x9355,0x9365,0x936d,0x9375,0x937d,0x9385,0x938b,
            0x93a1,0x93ab,0x93b1,0x93bb,0x93c1,0x93cb,0x93e1,0x93eb,0x93f1,0x93fb,
            0x9401,0x940b,0x9411,0x941b,0x9421,0x942b];
        foreach (ushort operand in operands)
        {
            ushort native = ReadBotwoonInstructionWord(rom, 0xb30000 | operand);
            AssertEqual(native, BotwoonVisualDefinitions.FrameAt(operand), "Botwoon native head sprite selector");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xb3, operand, out ushort shared), "Botwoon shared selector found");
            AssertEqual(native, shared, "Botwoon shared native sprite pointer");
            AssertTrue(CompiledEnemyVisualSelectors.IsCalculatedSelector(0xb30000 | operand), "Botwoon excluded from literal regeneration");
        }
        // Include the unused records that still occupy space between live maps.
        foreach (ushort pointer in new ushort[] {0xe31d,0xe329,0xe335,0xe341,0xe34d,0xe359,0xe365,0xe371,0xe37d,0xe389,
            0xe395,0xe3b2,0xe3cf,0xe3db,0xe3f8,0xe415})
            AssertEqual((ushort)2, ReadBotwoonInstructionWord(rom, 0xb30000 | pointer), "Botwoon two-piece native map");
        foreach (ushort pointer in new ushort[] {0xe3a1,0xe3be,0xe3e7,0xe404})
            AssertEqual((ushort)3, ReadBotwoonInstructionWord(rom, 0xb30000 | pointer), "Botwoon diagonal open-mouth third piece");
        AssertEqual((ushort)0, ReadBotwoonInstructionWord(rom, 0xb3804d), "Botwoon hidden empty map");
        ushort[] exports = [0xe329,0xe335,0xe341,0xe359,0xe365,0xe371,0xe37d,0xe389,
            0xe3a1,0xe3b2,0xe3be,0xe3db,0xe3e7,0xe3f8,0xe404,0xe415];
        var frames = BotwoonVisualDefinitions.Frames();
        AssertEqual(exports.Length, frames.Length, "Botwoon export count");
        for (int i = 0; i < exports.Length; i++)
        {
            AssertEqual(exports[i], frames[i].Pointer, "Botwoon export order and pointer");
            AssertEqual((byte)0xb3, frames[i].Bank, "Botwoon export bank");
            AssertEqual($"botwoon_head_{i:D2}", frames[i].Name, "Botwoon stable export identity");
        }
        var known = operands.ToHashSet();
        for (int address = 0x933f; address <= 0x9431; address++)
            if (!known.Contains((ushort)address))
            {
                AssertThrows<InvalidDataException>(() => BotwoonVisualDefinitions.FrameAt((ushort)address), "Botwoon controls and unused gaps rejected as visuals");
                AssertTrue(!CompiledEnemyVisualSelectors.TryGet(0xb3, (ushort)address, out ushort missing), "Botwoon shared visual hole");
                AssertEqual((ushort)0, missing, "Botwoon missing output cleared");
            }
        foreach (ushort address in new ushort[] {0,0x7fff,0xffff})
            AssertThrows<InvalidDataException>(() => BotwoonVisualDefinitions.FrameAt(address), "Botwoon distant invalid visual");
    }

    private static void VerifyBotwoonOperandMapping()
    {
        ushort[] expected = [0x9345,0x934d,0x9355,0x9365,0x936d,0x9375,0x937d,0x9385,0x938b,
            0x93a1,0x93ab,0x93b1,0x93bb,0x93c1,0x93cb,0x93e1,0x93eb,0x93f1,0x93fb,
            0x9401,0x940b,0x9411,0x941b,0x9421,0x942b];
        AssertEqual(expected.Length, BotwoonInstructionProgramDefinitions.PresentationWordCount, "Botwoon native operand count");
        for (int i = 0; i < expected.Length; i++)
            AssertEqual(expected[i], BotwoonInstructionProgramDefinitions.PresentationWordAddress(i), "Botwoon native operand order");
        var words = expected.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(words.Contains((ushort)address), BotwoonInstructionProgramDefinitions.IsPresentationWord((ushort)address), "Botwoon full operand membership");
        foreach (int index in new[] {int.MinValue,-1,25,int.MaxValue})
            AssertThrows<IndexOutOfRangeException>(() => BotwoonInstructionProgramDefinitions.PresentationWordAddress(index), "Botwoon operand ordinal bounds");
    }
}
