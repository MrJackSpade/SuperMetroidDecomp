using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.Artwork.CompiledEnemyVisualSelectorsVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.FaceBlocks;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Enemies;

/// <summary>Checks which face-block animation each activation side starts and how long each pose holds.</summary>
internal static class BlueBrinstarFaceBlockBehaviorVerification
{
    /// <summary>
    /// The initializer starts the neutral pose, which shows once and holds; Morph Ball-equipped Samus approaching from
    /// the left or right activates the block, which then holds the neutral pose for 48 updates and its two turning poses
    /// for 16 each before holding the last. The exported frames are the poses those programs show.
    /// </summary>
    internal static void VerifyBlueBrinstarFaceBlockBehavior()
    {
        (RoomEnemySystem enemies, RoomEnemyPlace place) = Create(parameter2: 0);
        ushort neutral = BlueBrinstarFaceBlockAnimationsConstants.Spritemaps(BlueBrinstarFaceBlockSegment.LookingAhead).Single();
        // A still picture shows once and holds (#2275).
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([neutral], [1]), EnemyInstructionRunner.ShownFrames(place, 3), "face block neutral pose");

        foreach ((ushort parameter2, int samusX, BlueBrinstarFaceBlockSegment program) in new[]
                 {
                     new VerifyBlueBrinstarFaceBlockBehaviorCase(0, -32, BlueBrinstarFaceBlockSegment.SamusLeft),
                     new VerifyBlueBrinstarFaceBlockBehaviorCase(1, 32, BlueBrinstarFaceBlockSegment.SamusRight),
                 })
        {
            (enemies, place) = Create(parameter2);
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
            {
                CollectedItems = SamusEquipmentWordCodec.FromEquipmentWord(BlueBrinstarFaceBlockNativeValues.MorphBallItem),
                Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(place.Position.X.Whole + samusX)), place.Position.Y.Whole)),
            };
            // The block activates for Samus on the side it faces and turns toward her: its turn's poses show it chose them.
            place.RunMainAi(samus);
            int[] holds = [48, 16, 16];
            AssertEqual(EnemyInstructionRunner.ExpectedFrames(BlueBrinstarFaceBlockAnimationsConstants.Spritemaps(program), holds),
                EnemyInstructionRunner.ShownFrames(place, holds.Sum() + 1), $"face block {program} poses");
        }

        AssertEqual(5, BlueBrinstarFaceBlockVisualDefinitions.Frames().Select(frame => frame.Pointer).Distinct().Count(), "five distinct face-block poses");
        AssertTrue(BlueBrinstarFaceBlockVisualDefinitions.Frames().Select(frame => frame.Pointer).ToHashSet().SetEquals(
                BlueBrinstarFaceBlockAnimationsConstants.Spritemaps(Enum.GetValues<BlueBrinstarFaceBlockSegment>())),
            "the exported face-block frames are exactly the poses the programs show");
        Console.WriteLine("  Face block behavior: neutral start, both activation sides and their 48/16/16 turning poses pass.");
    }

    /// <summary>A face block initialized at (256, 256) with a 64-pixel trigger range.</summary>
    private static BlueBrinstarFaceBlockBehaviorCreateResult Create(ushort parameter2)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        var settings = (FaceBlockSpawnSettings)EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.FaceBlock, PopulationWords.Parameters(64, parameter2));
        _ = enemies.PlaceAs<BlueBrinstarFaceBlockEntity>(EnemyDefinitionId.FaceBlock, settings, RoomEnemyFrameFixtures.StandaloneSamus(), new(256, 256));
        return new BlueBrinstarFaceBlockBehaviorCreateResult(enemies, place);
    }

    /// <summary>Runs the face-block behavior check, its program oracle and the compiled selector check.</summary>
    [VerificationCommand("--lookup-face-block-programs")]
    internal static int LookupFaceBlockProgramsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyBlueBrinstarFaceBlockBehavior), () => VerifyBlueBrinstarFaceBlockBehavior());
        Suite(nameof(BlueBrinstarFaceBlockAnimationsVerification.VerifyBlueBrinstarFaceBlockAnimations), () => BlueBrinstarFaceBlockAnimationsVerification.VerifyBlueBrinstarFaceBlockAnimations(rom));
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        return 0;
    }
}
