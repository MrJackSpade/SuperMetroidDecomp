using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyYappingMawInstructionProgramDefinitions()
    {
        VerifyYappingMawInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyYappingMawInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        AssertEqual(96, YappingMawInstructionProgramDefinitions.MechanicsWordCount,
            "Yapping Maw compiled mechanics word count");
        AssertEqual(52, YappingMawInstructionProgramDefinitions.PresentationWordCount,
            "Yapping Maw live presentation word count");
        for (int index = 0;
             index < YappingMawInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            YappingMawInstructionMechanicsWord definition =
                YappingMawInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadYappingMawInstructionWord(rom, definition.Address),
                $"Yapping Maw mechanics word $A8:{definition.Address:X4}");
        }

        var guard = new YappingMawInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        for (int direction = 0; direction < 8; direction++)
        {
            ushort entry = YappingMawInstructionProgramDefinitions
                .AttackForDirection(direction);
            (RoomEnemySystem enemies, RoomEnemySlot slot, _) =
                NewYappingMawInstructionSystem(guard, flags, entry);
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            RunYappingMawInstructionFrames(process, enemies, arguments, slot, count: 5);
            AssertEqual(unchecked((ushort)(entry + 4)), slot.CurrentInstruction,
                $"Yapping Maw attack direction {direction} completes its loop");
            AssertEqual((ushort?)0x002f, enemies.LastYappingMawSoundEffect,
                $"Yapping Maw attack direction {direction} publishes its sound");
        }

        (ushort Entry, int Calls, ushort Final, short X, short Y, string Name)[] cooldowns =
        [
            (YappingMawInstructionProgramDefinitions.CooldownFacingUpRight,
                6,
                unchecked((ushort)(
                    YappingMawInstructionProgramDefinitions.CooldownFacingUp + 6)),
                8, -8, "up-right"),
            (YappingMawInstructionProgramDefinitions.CooldownFacingUp,
                5,
                unchecked((ushort)(
                    YappingMawInstructionProgramDefinitions.CooldownFacingUp + 6)),
                0, -16, "up"),
            (YappingMawInstructionProgramDefinitions.CooldownFacingUpLeft,
                6,
                unchecked((ushort)(
                    YappingMawInstructionProgramDefinitions.CooldownFacingUpLeft + 12)),
                -8, -8, "up-left"),
            (YappingMawInstructionProgramDefinitions.CooldownFacingDownRight,
                6,
                unchecked((ushort)(
                    YappingMawInstructionProgramDefinitions.CooldownFacingDown + 6)),
                8, 8, "down-right"),
            (YappingMawInstructionProgramDefinitions.CooldownFacingDown,
                5,
                unchecked((ushort)(
                    YappingMawInstructionProgramDefinitions.CooldownFacingDown + 6)),
                0, 16, "down"),
            (YappingMawInstructionProgramDefinitions.CooldownFacingDownLeft,
                6,
                unchecked((ushort)(
                    YappingMawInstructionProgramDefinitions.CooldownFacingDownLeft + 12)),
                -8, 8, "down-left"),
        ];
        foreach ((ushort entry, int calls, ushort final, short x, short y, string name)
                 in cooldowns)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, YappingMawEnemyState state) =
                NewYappingMawInstructionSystem(guard, flags, entry);
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            RunYappingMawInstructionFrames(process, enemies, arguments, slot, count: 1);
            AssertEqual(unchecked((ushort)x), state.HeldSamusXOffset,
                $"Yapping Maw {name} callback X offset");
            AssertEqual(unchecked((ushort)y), state.HeldSamusYOffset,
                $"Yapping Maw {name} callback Y offset");
            RunYappingMawInstructionFrames(
                process,
                enemies,
                arguments,
                slot,
                count: calls - 1);
            AssertEqual(final, slot.CurrentInstruction,
                $"Yapping Maw {name} cooldown completes its loop");
            AssertEqual((ushort?)0x002f, enemies.LastYappingMawSoundEffect,
                $"Yapping Maw {name} cooldown publishes its sound");
        }

        VerifyYappingMawInitializerSelections(guard, flags);

        AssertEqual(YappingMawInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Yapping Maw spritemap operands remain cartridge reads");
        for (int index = 0;
             index < YappingMawInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                YappingMawInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Yapping Maw presentation word $A8:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Yapping Maw mechanics byte");

        AssertThrows<InvalidDataException>(
            () => YappingMawInstructionProgramDefinitions.ReadMechanicsWord(
                YappingMawInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Yapping Maw spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => YappingMawInstructionProgramDefinitions.ReadMechanicsWord(
                YappingMawInstructionProgramDefinitions.AdjacentAttackSelectorTable),
            "adjacent Yapping Maw direction table is rejected as mechanics");

        _ = ProbeYappingMawInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeYappingMawInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Yapping Maw allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Yapping Maw mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Yapping Maw instruction mechanics: 96 compiled words, all fourteen attack/" +
            "cooldown entries, seven callbacks, and 52 live spritemap reads pass.");
    }

    private static void VerifyYappingMawInitializerSelections(
        ISnesAddressSpace bus,
        BindingFlags flags)
    {
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeYappingMaw",
            flags)!;
        foreach ((ushort parameter2, ushort expected, string name) in new[]
        {
            ((ushort)0, YappingMawInstructionProgramDefinitions.AttackingFacingDown,
                "alternate/down"),
            ((ushort)1, YappingMawInstructionProgramDefinitions.AttackingFacingUp,
                "ordinary/up"),
        })
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.YappingMawDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
            slot.XPosition = 128;
            slot.YPosition = 128;
            slot.Parameter1 = 128;
            slot.Parameter2 = parameter2;
            initialize.Invoke(enemies, [slot]);
            AssertEqual(expected, slot.CurrentInstruction,
                $"Yapping Maw {name} initializer program");
            YappingMawEnemyState state = enemies.YappingMawStates[0] ??
                throw new InvalidOperationException(
                    $"Yapping Maw {name} initializer did not publish typed state.");
            AssertEqual(4, state.BodyProjectiles.Count(projectile => projectile is not null),
                $"Yapping Maw {name} initializer body links");
            AssertTrue(state.RootSpriteObject is not null,
                $"Yapping Maw {name} initializer root sprite object");
        }
    }

    private static (
        RoomEnemySystem Enemies,
        RoomEnemySlot Slot,
        YappingMawEnemyState State) NewYappingMawInstructionSystem(
            ISnesAddressSpace bus,
            BindingFlags flags,
            ushort entry)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.YappingMawDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        slot.CurrentInstruction = entry;
        slot.InstructionTimer = 1;
        var state = new YappingMawEnemyState(slot);
        var states = (YappingMawEnemyState?[])typeof(RoomEnemySystem)
            .GetField("_yappingMawStates", flags)!
            .GetValue(enemies)!;
        states[0] = state;
        return (enemies, slot, state);
    }

    private static void RunYappingMawInstructionFrames(
        MethodInfo process,
        RoomEnemySystem enemies,
        object?[] arguments,
        RoomEnemySlot slot,
        int count)
    {
        for (int call = 0; call < count; call++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
    }

    private static int ProbeYappingMawInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += YappingMawInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? YappingMawInstructionProgramDefinitions.AttackingFacingUp
                    : YappingMawInstructionProgramDefinitions.CooldownFacingDownLeft);
        }
        return checksum;
    }

    private static ushort ReadYappingMawInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa80000 | address) |
            source.ReadByte(0xa80000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class YappingMawInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (YappingMawInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Yapping Maw mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < YappingMawInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = YappingMawInstructionProgramDefinitions
                        .PresentationWordAddress(index);
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
