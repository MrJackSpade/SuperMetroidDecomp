using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Polyp;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that Polyp starts its stationary pose and holds it.</summary>
internal static class PolypBehaviorVerification
{
    /// <summary>The real initializer starts the stationary program, which shows its one frame and then holds it.</summary>
    internal static void VerifyPolypBehavior()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().SeedRandom = _ => { };
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Polyp, place.Definition);
        place.Bind(place.DefinitionId, default(RoomEnemyDefinition));
        enemies.PolypBehavior.InitializePolyp(place);
        AssertEqual(new AnimationPlayback<PolypSegment>(PolypAnimations.StartingWith(PolypSegment.Stationary), AnimationPlace.StartOfAnimation),
            (enemies.Places.InUpdateOrder[0].EntityState<PolypEntity, PolypEnemyState>() ?? throw new InvalidOperationException("The Polyp has no state.")).Playback,
            "real Polyp initializer starts the stationary program");
        // A still picture is held (#2276): it is new on the update it shows and stays without showing anew.
        ushort frame = PolypAnimationsConstants.Spritemaps(PolypSegment.Stationary).Single();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([frame], [1]),
            EnemyInstructionRunner.ShownFrames(place, 4), "Polyp shows its stationary frame once and holds it");
        Console.WriteLine("  Polyp behavior: the stationary pose shows and holds.");
    }

    /// <summary>Runs the focused Polyp behavior check and program oracle.</summary>
    [VerificationCommand("--polyp-instruction-mechanics")]
    internal static int PolypInstructionMechanicsCommand()
    {
        CartridgeImportAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyPolypBehavior), () => VerifyPolypBehavior());
        Suite(nameof(PolypAnimationsVerification.VerifyPolypAnimations), () => PolypAnimationsVerification.VerifyPolypAnimations(rom));
        return 0;
    }
}
