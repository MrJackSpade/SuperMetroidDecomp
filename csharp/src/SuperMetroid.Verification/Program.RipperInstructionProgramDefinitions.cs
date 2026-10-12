using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyRipperInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyRipperInstructionProgramDefinitions), () => VerifyRipperInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    private static void VerifyRipperInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < RipperInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                RipperInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa20000 | definition.Address)),
                $"Ripper-family instruction mechanics word $A2:{definition.Address:X4}");
        }

        for (int index = 0;
             index < RipperInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address = RipperInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            EnemyDefinitionId definition = address >= RipperInstructionProgramDefinitions.RipperMovingRight
                ? EnemyDefinitionId.Ripper
                : address >= RipperInstructionProgramDefinitions.Ripper2MovingRight
                    ? EnemyDefinitionId.Ripper2
                    : EnemyDefinitionId.GRipper;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa20000 | address)),
                RipperVisualDefinitions.FrameAt(definition, address),
                $"compiled Ripper-family frame selector $A2:{address:X4}");
        }
        foreach ((int record, EnemyDefinitionId definition) in new[]
                 {
                     (EnemyRomTablePointersRipper.GRipperPopulationRecord,
                         EnemyDefinitionId.GRipper),
                     (EnemyRomTablePointersRipper.Ripper2PopulationRecord,
                         EnemyDefinitionId.Ripper2),
                     (EnemyRomTablePointersRipper.RipperPopulationRecord,
                         EnemyDefinitionId.Ripper),
                 })
        {
            AssertEqual(definition, EnemyDefinitionIds.FromHeaderPointer(CartridgeImportSource.Require(rom).ReadCartridgeWord(record)),
                $"retail Ripper population ${record:X6} definition");
            ushort extra = CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 10));
            AssertTrue(!extra.HasAny(EnemyExtraProperties.UsesExtendedSpritemap),
                $"retail Ripper population ${record:X6} uses ordinary OAM");
        }
        AssertThrows<InvalidDataException>(
            () => RipperVisualDefinitions.FrameAt(EnemyDefinitionId.Ripper, 0xe477),
            "Ripper timing word is not a visual selector");

        var guard = NewRipperInstructionProgramReadGuard(rom);
        HashSet<ushort> observedFrames = [];
        Suite(nameof(VerifyGRipper), () => VerifyGRipper(1,
            RipperInstructionProgramDefinitions.GRipperMovingRight,
            "GRipper right"));
        Suite(nameof(VerifyGRipperReversal), () => VerifyGRipperReversal());
        Suite(nameof(VerifyRipper2), () => VerifyRipper2(0,
            RipperInstructionProgramDefinitions.Ripper2MovingRight,
            "Ripper II native-right label"));
        Suite(nameof(VerifyRipper2), () => VerifyRipper2(1,
            RipperInstructionProgramDefinitions.Ripper2MovingLeft,
            "Ripper II native-left label"));
        Suite(nameof(VerifyRipper), () => VerifyRipper(1,
            RipperInstructionProgramDefinitions.RipperMovingRight,
            "Ripper right"));
        Suite(nameof(VerifyRipper), () => VerifyRipper(0,
            RipperInstructionProgramDefinitions.RipperMovingLeft,
            "Ripper left"));

        AssertEqual(12, observedFrames.Count,
            "all Ripper-family walking frames execute across six native loops");
        foreach (ushort frame in new ushort[]
                 { 0xe3c5, 0xe3db, 0xe3ec, 0xe402, 0xe418, 0xe429,
                   0xe527, 0xe533, 0xe53f, 0xe54b, 0xe557, 0xe563 })
            AssertTrue(observedFrames.Contains(frame),
                $"Ripper-family live frame ${frame:X4}");

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Ripper mechanics and visual bytes");
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
            "production-installed animation loops, and twenty-four compiled visual selectors " +
            "pass with source bytes forbidden.");

        void VerifyGRipper(ushort packedSelector, ushort expectedProgram, string context)
        {
            RoomEnemySystem enemies = CreateSystem();
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeGRipper", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.GRipper;
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
            slot.EnemyDefinitionPointer = EnemyDefinitionId.GRipper;
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
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Ripper2;
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
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Ripper;
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

        void VerifyInstalledLoop(
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
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int frame = 0; frame < 31; frame++)
            {
                process.Invoke(enemies, arguments);
                if (slot.ExtraProperties.HasAny(EnemyExtraProperties.NewInstructionFrame))
                    observedFrames.Add(slot.SpritemapPointer);
            }
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

    private static CartridgeReadGuard NewRipperInstructionProgramReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(RipperInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Ripper-family instruction byte ${address:X6}.")
            .ForbidWords(0xa20000, RipperInstructionProgramDefinitionsTooling.PresentationWordCount,
                RipperInstructionProgramDefinitionsTooling.PresentationWordAddress,
                address => $"Production read compiled Ripper-family instruction byte ${address:X6}.");
}
