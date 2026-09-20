using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyEvirInstructionProgramDefinitions()
    {
        VerifyEvirInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyEvirInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        (ushort Entry, int Frames, ushort Definition, string Name)[] loops =
        [
            (EvirInstructionProgramDefinitions.BodyFacingLeft,
                EvirInstructionProgramDefinitions.BodyFrameCount,
                RoomEnemySystem.EvirDefinition,
                "left body"),
            (EvirInstructionProgramDefinitions.ArmsFacingLeft,
                EvirInstructionProgramDefinitions.ArmsFrameCount,
                RoomEnemySystem.EvirDefinition,
                "left arms"),
            (EvirInstructionProgramDefinitions.BodyFacingRight,
                EvirInstructionProgramDefinitions.BodyFrameCount,
                RoomEnemySystem.EvirDefinition,
                "right body"),
            (EvirInstructionProgramDefinitions.ArmsFacingRight,
                EvirInstructionProgramDefinitions.ArmsFrameCount,
                RoomEnemySystem.EvirDefinition,
                "right arms"),
        ];

        AssertEqual(67, EvirInstructionProgramDefinitions.MechanicsWordCount,
            "Evir compiled mechanics word count");
        AssertEqual(49, EvirInstructionProgramDefinitions.PresentationWordCount,
            "Evir live presentation word count");
        for (int index = 0;
             index < EvirInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            EvirInstructionMechanicsWord definition =
                EvirInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadEvirInstructionWord(rom, definition.Address),
                $"Evir mechanics word $A8:{definition.Address:X4}");
        }

        var guard = new EvirInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        foreach ((ushort entry, int frameCount, ushort definition, string name) in loops)
        {
            var enemies = NewEvirInstructionSystem(guard, flags);
            RoomEnemySlot slot = enemies.Slots[0];
            PrepareEvirInstructionSlot(slot, definition, entry);
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int call = 0; call < frameCount + 1; call++)
            {
                slot.InstructionTimer = 1;
                process.Invoke(enemies, arguments);
            }
            AssertEqual(unchecked((ushort)(entry + 4)), slot.CurrentInstruction,
                $"Evir {name} completes its loop and loads the first frame");
        }

        {
            var enemies = NewEvirInstructionSystem(guard, flags);
            RoomEnemySlot slot = enemies.Slots[0];
            PrepareEvirInstructionSlot(
                slot,
                RoomEnemySystem.EvirProjectileDefinition,
                EvirInstructionProgramDefinitions.ProjectileNormal);
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            process.Invoke(enemies, arguments);
            process.Invoke(enemies, arguments);
            AssertEqual(unchecked((ushort)(
                    EvirInstructionProgramDefinitions.ProjectileNormal + 4)),
                slot.CurrentInstruction,
                "normal Evir projectile reaches terminal sleep");
        }

        {
            var enemies = NewEvirInstructionSystem(guard, flags);
            RoomEnemySlot slot = enemies.Slots[0];
            PrepareEvirInstructionSlot(
                slot,
                RoomEnemySystem.EvirProjectileDefinition,
                EvirInstructionProgramDefinitions.ProjectileRegenerating);
            var state = new EvirEnemyState(slot)
            {
                FacingDirection = 0,
                MovingFlag = 1,
                RegenerationFlag = 1,
                Function = EvirAiFunction.ProjectileRegenerating,
            };
            var states = (EvirEnemyState?[])typeof(RoomEnemySystem)
                .GetField("_evirStates", flags)!
                .GetValue(enemies)!;
            states[0] = state;
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int call = 0; call < 10; call++)
            {
                slot.InstructionTimer = 1;
                process.Invoke(enemies, arguments);
            }
            AssertEqual(unchecked((ushort)(
                    EvirInstructionProgramDefinitions.ProjectileRegenerationLoop + 16)),
                slot.CurrentInstruction,
                "regenerating Evir projectile reaches terminal sleep");
            AssertEqual((ushort)0, state.RegenerationXOffset,
                "regeneration loop advances the projectile mouth offset eight times");
            AssertEqual((ushort)0, state.RegenerationFlag,
                "regeneration finish callback clears regeneration ownership");
            AssertEqual((ushort)0, state.MovingFlag,
                "regeneration finish callback leaves the projectile stationary");
            AssertEqual(EvirAiFunction.ProjectileIdle, state.Function,
                "regeneration finish callback restores idle AI");
            AssertEqual((ushort)0x005e, enemies.LastEvirSoundEffect,
                "regeneration script publishes the native spit sound");
        }

        AssertEqual(EvirInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Evir spritemap operands remain cartridge reads");
        for (int index = 0;
             index < EvirInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = EvirInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Evir presentation word $A8:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Evir mechanics byte");

        AssertThrows<InvalidDataException>(
            () => EvirInstructionProgramDefinitions.ReadMechanicsWord(
                EvirInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Evir spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => EvirInstructionProgramDefinitions.ReadMechanicsWord(
                EvirInstructionProgramDefinitions.AdjacentCallbackCode),
            "adjacent Evir callback code is rejected as mechanics");

        _ = ProbeEvirInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeEvirInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Evir allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Evir mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Evir instruction mechanics: 67 compiled words, all six body/arms/projectile " +
            "programs, complete regeneration callbacks, and 49 live spritemap reads pass.");
    }

    private static RoomEnemySystem NewEvirInstructionSystem(
        ISnesAddressSpace bus,
        BindingFlags flags)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        return enemies;
    }

    private static void PrepareEvirInstructionSlot(
        RoomEnemySlot slot,
        ushort definition,
        ushort entry)
    {
        slot.EnemyDefinitionPointer = definition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        slot.CurrentInstruction = entry;
        slot.InstructionTimer = 1;
    }

    private static int ProbeEvirInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += EvirInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? EvirInstructionProgramDefinitions.BodyFacingLeft
                    : EvirInstructionProgramDefinitions.ProjectileRegenerating);
        }
        return checksum;
    }

    private static ushort ReadEvirInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa80000 | address) |
            source.ReadByte(0xa80000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class EvirInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (EvirInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Evir mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < EvirInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        EvirInstructionProgramDefinitions.PresentationWordAddress(index);
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
