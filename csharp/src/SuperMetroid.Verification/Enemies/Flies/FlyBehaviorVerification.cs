using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Flies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Flies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that every fly-family definition starts and runs the shared flight loop.</summary>
internal static class FlyBehaviorVerification
{
    /// <summary>
    /// Each of Mellow, Mella and Memu starts the flight loop, shows its four frames two updates apiece and returns to
    /// the first through the loop's goto.
    /// </summary>
    internal static void VerifyFlyBehavior()
    {
        ushort[] loop = FlyAnimationsConstants.Spritemaps(FlySegment.Flight).ToArray();
        string expected = EnemyInstructionRunner.ExpectedFrames([.. loop, loop[0]], [2, 2, 2, 2, 2]);
        foreach (EnemyDefinitionId definition in new[] { EnemyDefinitionId.Mellow, EnemyDefinitionId.Mella, EnemyDefinitionId.MaridiaFly })
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Bind(definition, place.Definition);
            place.Bind(place.DefinitionId, default(RoomEnemyDefinition));
            FliesBehavior.InitializeFly(place);
            AssertEqual(new AnimationPlayback<FlySegment>(FlyAnimations.StartingWith(FlySegment.Flight), AnimationPlace.StartOfAnimation),
                (enemies.Places.InUpdateOrder[0].EntityState<FliesEntity, FlyEnemyState>() ?? throw new InvalidOperationException("The fly has no state.")).Playback,
                $"fly {definition} initializer animation");
            AssertEqual(expected, EnemyInstructionRunner.ShownFrames(place, 9),
                $"fly {definition} runs the flight loop");
        }
        Console.WriteLine("  Fly behavior: all three initializers start and run the shared flight loop.");
    }

    /// <summary>Runs the fly-family behavior checks and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--fly-instruction-mechanics")]
    internal static int FlyInstructionMechanicsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyFlyBehavior), () => VerifyFlyBehavior());
        Suite(nameof(FlyAnimationsVerification.VerifyFlyAnimations), () => FlyAnimationsVerification.VerifyFlyAnimations(rom));
        return 0;
    }
}
