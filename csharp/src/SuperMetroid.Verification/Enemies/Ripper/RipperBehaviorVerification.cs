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
    /// Checks the retail populations' definitions, then runs each initializer and the grapple Ripper reversal through the
    /// production enemy system: the loop it selects shows its four frames on the native 8/7/8/7 cadence and returns to
    /// its first frame, and the six loops together show all twelve walking frames.
    /// </summary>
    internal static void VerifyRipperBehavior(CartridgeImportAddressSpace rom)
    {
        foreach ((int record, EnemyDefinitionId definition) in new[]
                 {
                     new VerifyRipperBehaviorCase(EnemyRomTablePointersRipper.GrappleRipperPopulationRecord, EnemyDefinitionId.GrappleRipper),
                     new VerifyRipperBehaviorCase(EnemyRomTablePointersRipper.Ripper2PopulationRecord, EnemyDefinitionId.JetRipper),
                     new VerifyRipperBehaviorCase(EnemyRomTablePointersRipper.RipperPopulationRecord, EnemyDefinitionId.Ripper),
                 })
        {
            AssertEqual(definition, EnemyDefinitionIdsConstants.FromHeaderPointer(rom.ReadCartridgeWord(record)),
                $"retail Ripper population ${record:X6} definition");
            ushort extra = rom.ReadCartridgeWord(record + 10);
            AssertTrue(!SuperMetroid.AssetExtraction.Enemies.EnemyPropertyWordCodec.Decode(0, extra).CollidesByFrameHitboxes,
                $"retail Ripper population ${record:X6} uses ordinary OAM");
        }

        HashSet<ushort> observedFrames = [];
        Suite(nameof(VerifyGrappleRipper), () => VerifyGrappleRipper(RipperSegment.GrappleRipperMovingRight, "GrappleRipper right"));
        Suite(nameof(VerifyGrappleRipperReversal), () => VerifyGrappleRipperReversal());
        Suite(nameof(VerifyRipper2), () => VerifyRipper2(0, RipperSegment.GrappleRipperMovingLeft, "Ripper II flying left"));
        Suite(nameof(VerifyRipper2), () => VerifyRipper2(1, RipperSegment.GrappleRipperMovingRight, "Ripper II flying right"));
        Suite(nameof(VerifyRipper), () => VerifyRipper(1, RipperSegment.RipperMovingRight, "Ripper right"));
        Suite(nameof(VerifyRipper), () => VerifyRipper(0, RipperSegment.RipperMovingLeft, "Ripper left"));

        AssertEqual(RipperFrameDefinitionsConstants.PoseCount, observedFrames.Count,
            "all Ripper-family walking frames execute across six loops");
        Console.WriteLine("  Ripper behavior: three initializers and the reversal select and run their loops.");

        void VerifyGrappleRipper(RipperSegment expectedProgram, string context)
        {
            RoomEnemySystem enemies = NewSystem();
            RoomEnemyPlace place = NewPlace(enemies, EnemyDefinitionId.GrappleRipper);
            place.SetSpawnSettings(new GrappleRipperSpawnSettings(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(1), HorizontalDirection.Right, MinimumX: 0, MaximumX: ushort.MaxValue));
            place.SetSpawnParameter1(EnemyDefinitionId.GrappleRipper, 0);
            place.SetSpawnParameter2(EnemyDefinitionId.GrappleRipper, ushort.MaxValue);
            RipperBehavior.InitializeGrappleRipper(place.HeldOccupant(), place);
            VerifyInstalledLoop(enemies, place, expectedProgram, context);
        }

        void VerifyGrappleRipperReversal()
        {
            RoomEnemySystem enemies = NewSystem();
            RoomEnemyPlace place = NewPlace(enemies, EnemyDefinitionId.GrappleRipper);
            place.SetSpawnSettings(new GrappleRipperSpawnSettings(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(1), HorizontalDirection.Right, MinimumX: 0, MaximumX: ushort.MaxValue));
            place.SetSpawnParameter1(EnemyDefinitionId.GrappleRipper, 0);
            place.SetSpawnParameter2(EnemyDefinitionId.GrappleRipper, ushort.MaxValue);
            RipperBehavior.InitializeGrappleRipper(place.HeldOccupant(), place);
            RipperBehavior.ReverseRipperVariant(place);
            VerifyInstalledLoop(enemies, place, RipperSegment.GrappleRipperMovingLeft, "GrappleRipper left reversal");
        }

        void VerifyRipper2(ushort initialDirection, RipperSegment expectedProgram, string context)
        {
            RoomEnemySystem enemies = NewSystem();
            RoomEnemyPlace place = NewPlace(enemies, EnemyDefinitionId.JetRipper);
            place.SetSpawnParameter1(EnemyDefinitionId.Ripper, 1);
            place.SetSpawnParameter2(EnemyDefinitionId.Ripper, initialDirection);
            RipperBehavior.InitializeJetRipper(place.HeldOccupant(), place);
            VerifyInstalledLoop(enemies, place, expectedProgram, context);
        }

        void VerifyRipper(ushort initialDirection, RipperSegment expectedProgram, string context)
        {
            RoomEnemySystem enemies = NewSystem();
            RoomEnemyPlace place = NewPlace(enemies, EnemyDefinitionId.Ripper);
            place.SetSpawnParameter1(EnemyDefinitionId.Ripper, 1);
            place.SetSpawnParameter2(EnemyDefinitionId.Ripper, initialDirection);
            RipperBehavior.InitializeRipper(place.HeldOccupant(), place);
            VerifyInstalledLoop(enemies, place, expectedProgram, context);
        }

        RoomEnemySystem NewSystem()
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            return enemies;
        }

        void VerifyInstalledLoop(RoomEnemySystem enemies, RoomEnemyPlace place, RipperSegment expectedProgram, string context)
        {
            // The loop shows four frames held 8, 7, 8 and 7 updates, then its goto shows the first frame again.
            ushort[] loop = RipperAnimationsConstants.Spritemaps(expectedProgram).ToArray();
            ushort[] expected = [.. loop, loop[0]];
            int[] expectedUpdates = [0, 8, 15, 23, 30];
            place.AnimationClock.Restart();
            object?[] arguments = [place, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
            List<VerifyInstalledLoopCase> shown = [];
            for (int update = 0; update < 31; update++)
            {
                EnemyInstructionRunner.Process(arguments);
                if (place.AnimationClock.ShowsNewFrame)
                {
                    shown.Add(new VerifyInstalledLoopCase(update, place.SpritemapPointer));
                    observedFrames.Add(place.SpritemapPointer);
                }
            }
            AssertEqual(string.Join(",", expected.Zip(expectedUpdates, (map, update) => $"{update}:{map:X4}")),
                string.Join(",", shown.Select(frame => $"{frame.Update}:{frame.Spritemap:X4}")),
                $"{context} runs its selected loop on the 8/7/8/7 cadence and returns through its goto");
        }
    }

    /// <summary>A slot of <paramref name="enemies"/> holding <paramref name="definition"/> in bank $A2.</summary>
    private static RoomEnemyPlace NewPlace(RoomEnemySystem enemies, EnemyDefinitionId definition)
    {
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(definition, place.KeptDefinition);
        place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        return place;
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
