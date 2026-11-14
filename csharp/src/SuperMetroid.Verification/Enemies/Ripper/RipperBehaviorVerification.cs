using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.Core.Enemies.Ripper.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Ripper;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that each Ripper variant starts, runs and reverses its typed flying loop as the cartridge does.</summary>
internal static class RipperBehaviorVerification
{
    /// <summary>
    /// Checks the retail populations' definitions, then runs each initializer and the GRipper reversal through the
    /// production enemy system: the loop it selects shows its four frames on the native 8/7/8/7 cadence and returns to
    /// its first frame, and the six loops together show all twelve walking frames.
    /// </summary>
    internal static void VerifyRipperBehavior(CartridgeImportAddressSpace rom)
    {
        foreach ((int record, EnemyDefinitionId definition) in new[]
                 {
                     new VerifyRipperBehaviorCase(EnemyRomTablePointersRipper.GRipperPopulationRecord, EnemyDefinitionId.GRipper),
                     new VerifyRipperBehaviorCase(EnemyRomTablePointersRipper.Ripper2PopulationRecord, EnemyDefinitionId.Ripper2),
                     new VerifyRipperBehaviorCase(EnemyRomTablePointersRipper.RipperPopulationRecord, EnemyDefinitionId.Ripper),
                 })
        {
            AssertEqual(definition, EnemyDefinitionIds.FromHeaderPointer(CartridgeImportSource.Require(rom).ReadCartridgeWord(record)),
                $"retail Ripper population ${record:X6} definition");
            ushort extra = CartridgeImportSource.Require(rom).ReadCartridgeWord(record + 10);
            AssertTrue(!SuperMetroid.AssetExtraction.Enemies.EnemyPropertyWordCodec.Decode(0, extra).CollidesByFrameHitboxes,
                $"retail Ripper population ${record:X6} uses ordinary OAM");
        }

        HashSet<ushort> observedFrames = [];
        Suite(nameof(VerifyGRipper), () => VerifyGRipper(RipperSegment.GRipperMovingRight, "GRipper right"));
        Suite(nameof(VerifyGRipperReversal), () => VerifyGRipperReversal());
        Suite(nameof(VerifyRipper2), () => VerifyRipper2(0, RipperSegment.GRipperMovingLeft, "Ripper II flying left"));
        Suite(nameof(VerifyRipper2), () => VerifyRipper2(1, RipperSegment.GRipperMovingRight, "Ripper II flying right"));
        Suite(nameof(VerifyRipper), () => VerifyRipper(1, RipperSegment.RipperMovingRight, "Ripper right"));
        Suite(nameof(VerifyRipper), () => VerifyRipper(0, RipperSegment.RipperMovingLeft, "Ripper left"));

        AssertEqual(RipperFrameDefinitionsConstants.PoseCount, observedFrames.Count,
            "all Ripper-family walking frames execute across six loops");
        Console.WriteLine("  Ripper behavior: three initializers and the reversal select and run their loops.");

        void VerifyGRipper(RipperSegment expectedProgram, string context)
        {
            RoomEnemySystem enemies = NewSystem();
            RoomEnemySlot slot = NewSlot(enemies, EnemyDefinitionId.GRipper);
            slot.SetSpawnSettings(new GRipperSpawnSettings(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(1), GRipperStartDirection.Right, MinimumX: 0, MaximumX: ushort.MaxValue));
            slot.SetSpawnParameter1(EnemyDefinitionId.GRipper, 0);
            slot.SetSpawnParameter2(EnemyDefinitionId.GRipper, ushort.MaxValue);
            RipperBehavior.InitializeGRipper(slot);
            VerifyInstalledLoop(enemies, slot, expectedProgram, context);
        }

        void VerifyGRipperReversal()
        {
            RoomEnemySystem enemies = NewSystem();
            RoomEnemySlot slot = NewSlot(enemies, EnemyDefinitionId.GRipper);
            slot.SetSpawnSettings(new GRipperSpawnSettings(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(1), GRipperStartDirection.Right, MinimumX: 0, MaximumX: ushort.MaxValue));
            slot.SetSpawnParameter1(EnemyDefinitionId.GRipper, 0);
            slot.SetSpawnParameter2(EnemyDefinitionId.GRipper, ushort.MaxValue);
            RipperBehavior.InitializeGRipper(slot);
            RipperBehavior.ReverseRipperVariant(slot, RipperSegment.GRipperMovingRight, RipperSegment.GRipperMovingLeft);
            VerifyInstalledLoop(enemies, slot, RipperSegment.GRipperMovingLeft, "GRipper left reversal");
        }

        void VerifyRipper2(ushort initialDirection, RipperSegment expectedProgram, string context)
        {
            RoomEnemySystem enemies = NewSystem();
            RoomEnemySlot slot = NewSlot(enemies, EnemyDefinitionId.Ripper2);
            slot.SetSpawnParameter1(EnemyDefinitionId.Ripper, 1);
            slot.SetSpawnParameter2(EnemyDefinitionId.Ripper, initialDirection);
            RipperBehavior.InitializeRipper2(slot);
            VerifyInstalledLoop(enemies, slot, expectedProgram, context);
        }

        void VerifyRipper(ushort initialDirection, RipperSegment expectedProgram, string context)
        {
            RoomEnemySystem enemies = NewSystem();
            RoomEnemySlot slot = NewSlot(enemies, EnemyDefinitionId.Ripper);
            slot.SetSpawnParameter1(EnemyDefinitionId.Ripper, 1);
            slot.SetSpawnParameter2(EnemyDefinitionId.Ripper, initialDirection);
            RipperBehavior.InitializeRipper(slot);
            VerifyInstalledLoop(enemies, slot, expectedProgram, context);
        }

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            return enemies;
        }

        void VerifyInstalledLoop(RoomEnemySystem enemies, RoomEnemySlot slot, RipperSegment expectedProgram, string context)
        {
            // The loop shows four frames held 8, 7, 8 and 7 updates, then its goto shows the first frame again.
            ushort[] loop = RipperAnimationsConstants.Spritemaps(expectedProgram).ToArray();
            ushort[] expected = [.. loop, loop[0]];
            int[] expectedUpdates = [0, 8, 15, 23, 30];
            slot.AnimationClock.Restart();
            object?[] arguments = [slot, null, null, Position16<RoomPixels>.Zero, ControllerButton.None];
            List<VerifyInstalledLoopCase> shown = [];
            for (int update = 0; update < 31; update++)
            {
                EnemyInstructionRunner.Process(arguments);
                if (slot.AnimationClock.ShowsNewFrame)
                {
                    shown.Add(new VerifyInstalledLoopCase(update, slot.SpritemapPointer));
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
        slot.Bind(definition, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        return slot;
    }

    /// <summary>Runs the Ripper-family behavior checks against the local retail cartridge.</summary>
    [VerificationCommand("--ripper-behavior")]
    internal static int RipperBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyRipperBehavior), () => VerifyRipperBehavior(rom));
        Suite(nameof(RipperAnimationsVerification.VerifyRipperAnimations), () => RipperAnimationsVerification.VerifyRipperAnimations(rom));
        return 0;
    }
}
