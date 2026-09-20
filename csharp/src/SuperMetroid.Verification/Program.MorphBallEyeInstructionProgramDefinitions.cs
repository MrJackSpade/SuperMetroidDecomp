using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMorphBallEyeInstructionProgramDefinitions()
    {
        VerifyMorphBallEyeInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyMorphBallEyeInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        (ushort Entry, int Frames, ushort Sleep, string Name)[] sleepingPrograms =
        [
            (MorphBallEyeInstructionProgramDefinitions.FacingRightDeactivating,
                4,
                unchecked((ushort)(MorphBallEyeInstructionProgramDefinitions
                    .FacingRightClosed + 4)),
                "right deactivation and closed hold"),
            (MorphBallEyeInstructionProgramDefinitions.FacingRightClosed,
                1,
                unchecked((ushort)(MorphBallEyeInstructionProgramDefinitions
                    .FacingRightClosed + 4)),
                "right closed hold"),
            (MorphBallEyeInstructionProgramDefinitions.FacingLeftDeactivating,
                4,
                unchecked((ushort)(MorphBallEyeInstructionProgramDefinitions
                    .FacingLeftClosed + 4)),
                "left deactivation and closed hold"),
            (MorphBallEyeInstructionProgramDefinitions.FacingLeftClosed,
                1,
                unchecked((ushort)(MorphBallEyeInstructionProgramDefinitions
                    .FacingLeftClosed + 4)),
                "left closed hold"),
            (MorphBallEyeInstructionProgramDefinitions.FacingRightActivating,
                4,
                unchecked((ushort)(MorphBallEyeInstructionProgramDefinitions
                    .FacingRightActivating + 16)),
                "right activation"),
            (MorphBallEyeInstructionProgramDefinitions.FacingLeftActivating,
                4,
                unchecked((ushort)(MorphBallEyeInstructionProgramDefinitions
                    .FacingLeftActivating + 16)),
                "left activation"),
            (MorphBallEyeInstructionProgramDefinitions.MountFacingRight,
                1,
                unchecked((ushort)(MorphBallEyeInstructionProgramDefinitions
                    .MountFacingRight + 4)),
                "right mount"),
            (MorphBallEyeInstructionProgramDefinitions.MountFacingDown,
                1,
                unchecked((ushort)(MorphBallEyeInstructionProgramDefinitions
                    .MountFacingDown + 4)),
                "down mount"),
            (MorphBallEyeInstructionProgramDefinitions.MountFacingLeft,
                1,
                unchecked((ushort)(MorphBallEyeInstructionProgramDefinitions
                    .MountFacingLeft + 4)),
                "left mount"),
            (MorphBallEyeInstructionProgramDefinitions.MountFacingUp,
                1,
                unchecked((ushort)(MorphBallEyeInstructionProgramDefinitions
                    .MountFacingUp + 4)),
                "up mount"),
        ];

        AssertEqual(46, MorphBallEyeInstructionProgramDefinitions.MechanicsWordCount,
            "Morph Ball eye compiled mechanics word count");
        AssertEqual(36, MorphBallEyeInstructionProgramDefinitions.PresentationWordCount,
            "Morph Ball eye live presentation word count");
        for (int index = 0;
             index < MorphBallEyeInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            MorphBallEyeInstructionMechanicsWord definition =
                MorphBallEyeInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadMorphBallEyeInstructionWord(rom, definition.Address),
                $"Morph Ball eye mechanics word $A8:{definition.Address:X4}");
        }

        var guard = new MorphBallEyeInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        {
            var enemies = NewMorphBallEyeInstructionSystem(guard, flags);
            RoomEnemySlot slot = PrepareMorphBallEyeInstructionSlot(
                enemies,
                MorphBallEyeInstructionProgramDefinitions.Active);
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int call = 0;
                 call < MorphBallEyeInstructionProgramDefinitions.ActiveFrameCount + 1;
                 call++)
            {
                slot.InstructionTimer = 1;
                process.Invoke(enemies, arguments);
            }
            AssertEqual(unchecked((ushort)(
                    MorphBallEyeInstructionProgramDefinitions.Active + 4)),
                slot.CurrentInstruction,
                "Morph Ball eye active program completes its loop");
        }

        foreach ((ushort entry, int frames, ushort sleep, string name) in sleepingPrograms)
        {
            var enemies = NewMorphBallEyeInstructionSystem(guard, flags);
            RoomEnemySlot slot = PrepareMorphBallEyeInstructionSlot(enemies, entry);
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int call = 0; call < frames + 1; call++)
            {
                slot.InstructionTimer = 1;
                process.Invoke(enemies, arguments);
            }
            AssertEqual(sleep, slot.CurrentInstruction,
                $"Morph Ball eye {name} reaches terminal sleep");
        }

        VerifyMorphBallEyeInitializerSelections(guard, flags);

        AssertEqual(MorphBallEyeInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Morph Ball eye spritemap operands remain cartridge reads");
        for (int index = 0;
             index < MorphBallEyeInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                MorphBallEyeInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads eye presentation word $A8:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Morph Ball eye mechanics byte");

        AssertThrows<InvalidDataException>(
            () => MorphBallEyeInstructionProgramDefinitions.ReadMechanicsWord(
                MorphBallEyeInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Morph Ball eye spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MorphBallEyeInstructionProgramDefinitions.ReadMechanicsWord(
                MorphBallEyeInstructionProgramDefinitions.AdjacentProximityDefinitions),
            "adjacent Morph Ball eye proximity definitions are rejected as mechanics");

        _ = ProbeMorphBallEyeInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMorphBallEyeInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Morph Ball eye allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Morph Ball eye mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Morph Ball eye instruction mechanics: 46 compiled words, all eleven body/" +
            "mount programs, six initializer roles, and 36 live spritemap reads pass.");
    }

    private static void VerifyMorphBallEyeInitializerSelections(
        ISnesAddressSpace bus,
        BindingFlags flags)
    {
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeMorphBallEye",
            flags)!;
        foreach ((ushort parameter1, ushort expected) in new[]
        {
            ((ushort)0, MorphBallEyeInstructionProgramDefinitions.FacingRightClosed),
            ((ushort)1, MorphBallEyeInstructionProgramDefinitions.FacingLeftClosed),
        })
        {
            var enemies = NewMorphBallEyeInstructionSystem(bus, flags);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.MorphBallEyeDefinition;
            slot.Parameter1 = parameter1;
            slot.Parameter2 = 0;
            initialize.Invoke(enemies, [slot]);
            AssertEqual(expected, slot.CurrentInstruction,
                $"Morph Ball eye body initializer parameter {parameter1}");
        }

        ushort[] mountPrograms =
        [
            MorphBallEyeInstructionProgramDefinitions.MountFacingLeft,
            MorphBallEyeInstructionProgramDefinitions.MountFacingRight,
            MorphBallEyeInstructionProgramDefinitions.MountFacingUp,
            MorphBallEyeInstructionProgramDefinitions.MountFacingDown,
        ];
        for (ushort direction = 0; direction < mountPrograms.Length; direction++)
        {
            var enemies = NewMorphBallEyeInstructionSystem(bus, flags);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.MorphBallEyeDefinition;
            slot.Parameter2 = unchecked((ushort)(0x8000 | direction));
            initialize.Invoke(enemies, [slot]);
            AssertEqual(mountPrograms[direction], slot.CurrentInstruction,
                $"Morph Ball eye mount initializer direction {direction}");
        }
    }

    private static RoomEnemySystem NewMorphBallEyeInstructionSystem(
        ISnesAddressSpace bus,
        BindingFlags flags)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        return enemies;
    }

    private static RoomEnemySlot PrepareMorphBallEyeInstructionSlot(
        RoomEnemySystem enemies,
        ushort entry)
    {
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.MorphBallEyeDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        slot.CurrentInstruction = entry;
        slot.InstructionTimer = 1;
        return slot;
    }

    private static int ProbeMorphBallEyeInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MorphBallEyeInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? MorphBallEyeInstructionProgramDefinitions.Active
                    : MorphBallEyeInstructionProgramDefinitions.MountFacingUp);
        }
        return checksum;
    }

    private static ushort ReadMorphBallEyeInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa80000 | address) |
            source.ReadByte(0xa80000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class MorphBallEyeInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (MorphBallEyeInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Morph Ball eye mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < MorphBallEyeInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = MorphBallEyeInstructionProgramDefinitions
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
