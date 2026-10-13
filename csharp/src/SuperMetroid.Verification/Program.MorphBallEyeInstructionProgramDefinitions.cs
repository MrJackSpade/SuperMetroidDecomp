using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

internal static class MorphBallEyeInstructionProgramDefinitionsVerification
{
    internal static void VerifyMorphBallEyeInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyMorphBallEyeInstructionProgramDefinitions), () => VerifyMorphBallEyeInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    internal static void VerifyMorphBallEyeInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
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
            "Morph Ball eye fixed presentation word count");
        for (int index = 0;
             index < MorphBallEyeInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                MorphBallEyeInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadMorphBallEyeInstructionWord(rom, definition.Address),
                $"Morph Ball eye mechanics word $A8:{definition.Address:X4}");
        }

        var guard = NewMorphBallEyeInstructionReadGuard(rom);
        {
            var enemies = NewMorphBallEyeInstructionSystem(guard);
            RoomEnemySlot slot = PrepareMorphBallEyeInstructionSlot(
                enemies,
                MorphBallEyeInstructionProgramDefinitions.Active);
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int call = 0;
                 call < MorphBallEyeInstructionProgramDefinitions.ActiveFrameCount + 1;
                 call++)
            {
                slot.InstructionTimer = NativeCountdown.Start(1);
                EnemyInstructionRunner.Process(enemies, arguments);
            }
            AssertEqual(unchecked((ushort)(
                    MorphBallEyeInstructionProgramDefinitions.Active + 4)),
                slot.CurrentInstruction,
                "Morph Ball eye active program completes its loop");
        }

        foreach ((ushort entry, int frames, ushort sleep, string name) in sleepingPrograms)
        {
            var enemies = NewMorphBallEyeInstructionSystem(guard);
            RoomEnemySlot slot = PrepareMorphBallEyeInstructionSlot(enemies, entry);
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int call = 0; call < frames + 1; call++)
            {
                slot.InstructionTimer = NativeCountdown.Start(1);
                EnemyInstructionRunner.Process(enemies, arguments);
            }
            AssertEqual(sleep, slot.CurrentInstruction,
                $"Morph Ball eye {name} reaches terminal sleep");
        }

        Suite(nameof(VerifyMorphBallEyeInitializerSelections), () => VerifyMorphBallEyeInitializerSelections(guard));

        AssertEqual(0, guard.ObservedWords.Count,
            "Morph Ball eye programs use compiled spritemap selectors");
        for (int index = 0;
             index < MorphBallEyeInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                MorphBallEyeInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(!guard.ObservedWords.Contains(address),
                $"production execution avoids eye presentation word $A8:{address:X4}");
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
            "mount programs, six initializer roles, and 36 compiled spritemap selectors pass " +
            "without presentation ROM reads.");
    }

    private static void VerifyMorphBallEyeInitializerSelections(ISnesAddressSpace bus)
    {
        foreach ((ushort parameter1, ushort expected) in new[]
        {
            ((ushort)0, MorphBallEyeInstructionProgramDefinitions.FacingRightClosed),
            ((ushort)1, MorphBallEyeInstructionProgramDefinitions.FacingLeftClosed),
        })
        {
            var enemies = NewMorphBallEyeInstructionSystem(bus);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Eye;
            slot.Parameter1 = parameter1;
            slot.Parameter2 = 0;
            enemies.MorphBallEyeBehavior.InitializeMorphBallEye(slot);
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
            var enemies = NewMorphBallEyeInstructionSystem(bus);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Eye;
            slot.Parameter2 = unchecked((ushort)(0x8000 | direction));
            enemies.MorphBallEyeBehavior.InitializeMorphBallEye(slot);
            AssertEqual(mountPrograms[direction], slot.CurrentInstruction,
                $"Morph Ball eye mount initializer direction {direction}");
        }
    }

    private static RoomEnemySystem NewMorphBallEyeInstructionSystem(
        ISnesAddressSpace bus)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(bus);
        return enemies;
    }

    private static RoomEnemySlot PrepareMorphBallEyeInstructionSlot(
        RoomEnemySystem enemies,
        ushort entry)
    {
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Eye;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        slot.CurrentInstruction = entry;
        slot.InstructionTimer = NativeCountdown.Start(1);
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

    [CpuBusBoundary("exact bus test: retail ROM oracle for Morph Ball Eye instruction words")]
    internal static ushort ReadMorphBallEyeInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(0xa80000 | address);

    private static CartridgeReadGuard NewMorphBallEyeInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => MorphBallEyeInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address),
                address => $"Production read compiled Morph Ball eye mechanics byte ${address:X6}.")
            .ObserveWords(0xa80000, MorphBallEyeInstructionProgramDefinitions.PresentationWordCount, MorphBallEyeInstructionProgramDefinitions.PresentationWordAddress);

    [VerificationCommand("--morph-ball-eye-instruction-program-definitions")]
    internal static int MorphBallEyeInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyMorphBallEyeInstructionProgramDefinitions), () => VerifyMorphBallEyeInstructionProgramDefinitions());
        return 0;
    }
}
