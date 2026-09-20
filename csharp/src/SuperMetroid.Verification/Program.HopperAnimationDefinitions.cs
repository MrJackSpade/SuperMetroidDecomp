using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyHopperAnimationDefinitions(SuperMetroidAddressSpace rom)
    {
        int[] tables = [0xa3aac2, 0xa3aaca, 0xa3aad2, 0xa3aada];
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;

        for (int index = 0;
             index < HopperInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            HopperInstructionMechanicsWord definition =
                HopperInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadHopperAnimationWord(rom, 0xa30000 | definition.Address),
                $"hopper mechanics word $A3:{definition.Address:X4}");
        }

        for (ushort variant = 0; variant < 4; variant++)
        for (int selector = 0; selector < tables.Length; selector++)
        {
            bool upsideDown = (selector & 1) != 0;
            bool jumping = (selector & 2) != 0;
            ushort native = ReadHopperAnimationWord(rom, tables[selector] + variant * 2);
            AssertEqual(native,
                HopperAnimationDefinitions.InstructionList(variant, upsideDown, jumping),
                $"hopper animation variant {variant}, selector {selector}");
        }

        var guard = new HopperAnimationReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        ushort[] definitions =
        [
            RoomEnemySystem.SidehopperDefinition,
            RoomEnemySystem.LargeSidehopperDefinition,
            RoomEnemySystem.LargeDessgeegaDefinition,
            RoomEnemySystem.DessgeegaDefinition,
        ];

        for (ushort variant = 0; variant < 4; variant++)
        for (int orientation = 0; orientation < 2; orientation++)
        {
            bool upsideDown = orientation != 0;
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                enemies, guard);
            typeof(RoomEnemySystem).GetField("_setRandomNumber", flags)!.SetValue(
                enemies, (Action<ushort>)(_ => { }));
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
                enemies, (Func<ushort>)(() => 0));

            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeHopper", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            var startJump = typeof(RoomEnemySystem).GetMethod("StartHopperJump", flags)!
                .CreateDelegate<Action<RoomEnemySlot, HopperEnemyState, bool, bool>>();
            var land = typeof(RoomEnemySystem).GetMethod("LandHopper", flags)!
                .CreateDelegate<Action<RoomEnemySlot, HopperEnemyState>>();

            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = definitions[variant];
            slot.Definition = default(RoomEnemyDefinition) with
            {
                Bank = 0xa3,
                VariantIndex = variant,
            };
            slot.Parameter1 = upsideDown ? (ushort)1 : (ushort)0;
            initialize(slot);
            HopperEnemyState state = enemies.HopperStates[0]!;
            AssertEqual(
                HopperAnimationDefinitions.InstructionList(variant, upsideDown, jumping: false),
                slot.CurrentInstruction,
                $"hopper production initial landed list {variant}/{orientation}");

            state.XVelocity = 3;
            startJump(slot, state, upsideDown, false);
            AssertEqual(
                HopperAnimationDefinitions.InstructionList(variant, upsideDown, jumping: true),
                slot.CurrentInstruction,
                $"hopper production jumping list {variant}/{orientation}");

            ExecuteHopperInstructionProgram(enemies, process, slot, callCount: 1);
            AssertTrue(slot.Properties.HasAny(EnemyProperties.ProcessOffScreen),
                $"hopper jumping program {variant}/{orientation} enables off-screen processing");
            AssertEqual(
                variant is 0 or 1 ? (ushort?)0x005d : null,
                enemies.LastHopperSoundEffect,
                $"hopper jumping sound {variant}/{orientation}");
            ExecuteHopperInstructionProgram(enemies, process, slot, callCount: 1);

            land(slot, state);
            AssertEqual(
                HopperAnimationDefinitions.InstructionList(variant, upsideDown, jumping: false),
                slot.CurrentInstruction,
                $"hopper production landed handoff {variant}/{orientation}");

            ExecuteHopperInstructionProgram(enemies, process, slot, callCount: 5);
            AssertTrue(!slot.Properties.HasAny(EnemyProperties.ProcessOffScreen),
                $"hopper landed program {variant}/{orientation} disables off-screen processing");
            AssertTrue(state.ReadyToHop,
                $"hopper landed program {variant}/{orientation} publishes ready-to-hop");
            AssertEqual(
                variant is 0 or 1 ? (ushort?)0x005e : null,
                enemies.LastHopperSoundEffect,
                $"hopper landed sound {variant}/{orientation}");
        }

        VerifyTourianSidehopperInstructionAlias(rom, flags, process);

        AssertEqual(HopperInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all hopper spritemap operands remain cartridge reads");
        for (int index = 0;
             index < HopperInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                HopperInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads hopper presentation word $A3:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled hopper mechanics byte");

        AssertThrows<InvalidDataException>(
            () => HopperInstructionProgramDefinitions.ReadMechanicsWord(
                HopperInstructionProgramDefinitions.PresentationWordAddress(0)),
            "hopper spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => HopperInstructionProgramDefinitions.ReadMechanicsWord(
                HopperInstructionProgramDefinitions.LastAdjacentPhysicsWord),
            "adjacent hopper physics data is rejected as mechanics");

        _ = ProbeHopperInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeHopperInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "hopper allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed hopper mechanics lookups allocate no per-frame storage");

        AssertThrows<InvalidDataException>(
            () => HopperAnimationDefinitions.InstructionList(4, false, false),
            "hopper animation variant beyond authored table");
        AssertThrows<InvalidDataException>(
            () => HopperAnimationDefinitions.InstructionList(ushort.MaxValue, true, true),
            "restored hopper animation variant does not wrap into authored table");
        Console.WriteLine(
            "Hopper instruction mechanics: ninety-six compiled words, sixteen native " +
            "floor/ceiling programs, five production definitions, sound/processing/ready " +
            "side effects, and forty live spritemap reads pass with mechanics bytes forbidden.");
    }

    private static void VerifyTourianSidehopperInstructionAlias(
        SuperMetroidAddressSpace rom,
        BindingFlags flags,
        MethodInfo process)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies, new HopperAnimationReadGuard(rom));
        typeof(RoomEnemySystem).GetField("_setRandomNumber", flags)!.SetValue(
            enemies, (Action<ushort>)(_ => { }));
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
            enemies, (Func<ushort>)(() => 0));
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod("InitializeHopper", flags)!;

        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.TourianSidehopperDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3, VariantIndex = 1 };
        initialize.Invoke(enemies, [slot]);
        ExecuteHopperInstructionProgram(enemies, process, slot, callCount: 1);
        AssertEqual((ushort?)0x005e, enemies.LastHopperSoundEffect,
            "Tourian Sidehopper shares compiled large-Sidehopper landed program");
    }

    private static void ExecuteHopperInstructionProgram(
        RoomEnemySystem enemies,
        MethodInfo process,
        RoomEnemySlot slot,
        int callCount)
    {
        object?[] arguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int call = 0; call < callCount; call++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
    }

    private static int ProbeHopperInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += HopperInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? HopperInstructionProgramDefinitions.SidehopperJumpingFloor
                    : HopperInstructionProgramDefinitions.LargeDessgeegaLandedCeiling);
        }
        return checksum;
    }

    private static ushort ReadHopperAnimationWord(SuperMetroidAddressSpace source, int address) =>
        (ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8);

    private sealed class HopperAnimationReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address is >= 0xa3aac2 and < 0xa3aae2)
            {
                throw new InvalidOperationException(
                    $"Hopper attempted migrated animation-selector read ${address:X6}.");
            }
            if (HopperInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled hopper mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < HopperInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        HopperInstructionProgramDefinitions.PresentationWordAddress(index);
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
