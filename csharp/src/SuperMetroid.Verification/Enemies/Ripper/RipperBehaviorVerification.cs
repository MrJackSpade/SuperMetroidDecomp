using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.Core.Enemies.Ripper.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Ripper;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that each Ripper variant starts, runs and reverses its typed flying loop as the cartridge does.</summary>
internal static class RipperBehaviorVerification
{
    /// <summary>
    /// Checks the retail populations' definitions, then runs each initializer and the GRipper reversal through the
    /// production enemy system: the loop it selects shows its four frames on the native 8/7/8/7 cadence and returns to
    /// its first frame, and the six loops together show all twelve walking frames.
    /// </summary>
    internal static void VerifyRipperBehavior(SuperMetroidAddressSpace rom)
    {
        foreach ((int record, EnemyDefinitionId definition) in new[]
                 {
                     (EnemyRomTablePointersRipper.GRipperPopulationRecord, EnemyDefinitionId.GRipper),
                     (EnemyRomTablePointersRipper.Ripper2PopulationRecord, EnemyDefinitionId.Ripper2),
                     (EnemyRomTablePointersRipper.RipperPopulationRecord, EnemyDefinitionId.Ripper),
                 })
        {
            AssertEqual(definition, EnemyDefinitionIds.FromHeaderPointer(CartridgeImportSource.Require(rom).ReadCartridgeWord(record)),
                $"retail Ripper population ${record:X6} definition");
            ushort extra = CartridgeImportSource.Require(rom).ReadCartridgeWord(record + 10);
            AssertTrue(!extra.HasAny(EnemyExtraProperties.UsesExtendedSpritemap),
                $"retail Ripper population ${record:X6} uses ordinary OAM");
        }

        HashSet<ushort> observedFrames = [];
        Suite(nameof(VerifyGRipper), () => VerifyGRipper(RipperProgram.GRipperMovingRight, "GRipper right"));
        Suite(nameof(VerifyGRipperReversal), () => VerifyGRipperReversal());
        Suite(nameof(VerifyRipper2), () => VerifyRipper2(0, RipperProgram.Ripper2MovingRight, "Ripper II native-right label"));
        Suite(nameof(VerifyRipper2), () => VerifyRipper2(1, RipperProgram.Ripper2MovingLeft, "Ripper II native-left label"));
        Suite(nameof(VerifyRipper), () => VerifyRipper(1, RipperProgram.RipperMovingRight, "Ripper right"));
        Suite(nameof(VerifyRipper), () => VerifyRipper(0, RipperProgram.RipperMovingLeft, "Ripper left"));

        AssertEqual(RipperFrameDefinitionsConstants.PoseCount, observedFrames.Count,
            "all Ripper-family walking frames execute across six loops");
        Console.WriteLine("  Ripper behavior: three initializers and the reversal select and run their loops.");

        void VerifyGRipper(RipperProgram expectedProgram, string context)
        {
            RoomEnemySystem enemies = NewSystem();
            RoomEnemySlot slot = NewSlot(enemies, EnemyDefinitionId.GRipper);
            slot.CurrentInstruction = 1;
            slot.Parameter1 = 0;
            slot.Parameter2 = ushort.MaxValue;
            enemies.RipperBehavior.InitializeGRipper(slot);
            VerifyInstalledLoop(enemies, slot, expectedProgram, context);
        }

        void VerifyGRipperReversal()
        {
            RoomEnemySystem enemies = NewSystem();
            RoomEnemySlot slot = NewSlot(enemies, EnemyDefinitionId.GRipper);
            slot.CurrentInstruction = 1;
            slot.Parameter1 = 0;
            slot.Parameter2 = ushort.MaxValue;
            enemies.RipperBehavior.InitializeGRipper(slot);
            enemies.RipperBehavior.ReverseRipperVariant(slot, RipperProgram.GRipperMovingRight, RipperProgram.GRipperMovingLeft);
            VerifyInstalledLoop(enemies, slot, RipperProgram.GRipperMovingLeft, "GRipper left reversal");
        }

        void VerifyRipper2(ushort initialDirection, RipperProgram expectedProgram, string context)
        {
            RoomEnemySystem enemies = NewSystem();
            RoomEnemySlot slot = NewSlot(enemies, EnemyDefinitionId.Ripper2);
            slot.Parameter1 = 1;
            slot.Parameter2 = initialDirection;
            enemies.RipperBehavior.InitializeRipper2(slot);
            VerifyInstalledLoop(enemies, slot, expectedProgram, context);
        }

        void VerifyRipper(ushort initialDirection, RipperProgram expectedProgram, string context)
        {
            RoomEnemySystem enemies = NewSystem();
            RoomEnemySlot slot = NewSlot(enemies, EnemyDefinitionId.Ripper);
            slot.Parameter1 = 1;
            slot.Parameter2 = initialDirection;
            enemies.RipperBehavior.InitializeRipper(slot);
            VerifyInstalledLoop(enemies, slot, expectedProgram, context);
        }

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(rom);
            return enemies;
        }

        void VerifyInstalledLoop(RoomEnemySystem enemies, RoomEnemySlot slot, RipperProgram expectedProgram, string context)
        {
            // The loop shows four frames held 8, 7, 8 and 7 updates, then its goto shows the first frame again.
            ushort[] loop = RipperAnimationsConstants.Spritemaps(expectedProgram).ToArray();
            ushort[] expected = [.. loop, loop[0]];
            int[] expectedUpdates = [0, 8, 15, 23, 30];
            slot.InstructionTimer = NativeCountdown.Start(1);
            slot.Timer = NativeCountdown.Start(0);
            object?[] arguments = [slot, null, null, Position16<RoomPixels>.Zero, SnesButton.None];
            List<(int Update, ushort Spritemap)> shown = [];
            for (int update = 0; update < 31; update++)
            {
                EnemyInstructionRunner.Process(enemies, arguments);
                if (slot.ExtraProperties.HasAny(EnemyExtraProperties.NewInstructionFrame))
                {
                    shown.Add((update, slot.SpritemapPointer));
                    observedFrames.Add(slot.SpritemapPointer);
                }
            }
            AssertEqual(string.Join(",", expected.Zip(expectedUpdates, (map, update) => $"{update}:{map:X4}")),
                string.Join(",", shown.Select(frame => $"{frame.Update}:{frame.Spritemap:X4}")),
                $"{context} runs its selected loop on the 8/7/8/7 cadence and returns through its goto");
        }
    }

    /// <summary>A slot of <paramref name="enemies"/> holding <paramref name="definition"/> in bank $A2.</summary>
    private static RoomEnemySlot NewSlot(RoomEnemySystem enemies, EnemyDefinitionId definition)
    {
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = definition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = RipperFrameToolingDefinitions.Bank };
        return slot;
    }

    /// <summary>Runs the Ripper-family behavior checks against the local retail cartridge.</summary>
    [VerificationCommand("--ripper-behavior")]
    internal static int RipperBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyRipperBehavior), () => VerifyRipperBehavior(rom));
        Suite(nameof(RipperAnimationsVerification.VerifyRipperAnimations), () => RipperAnimationsVerification.VerifyRipperAnimations(rom));
        return 0;
    }
}
