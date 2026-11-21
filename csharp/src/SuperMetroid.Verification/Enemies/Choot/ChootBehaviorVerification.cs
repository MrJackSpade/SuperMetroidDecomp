using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Choot;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Choot;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Choot's idle, jumping and falling animations and their off-screen processing switches.</summary>
internal static class ChootBehaviorVerification
{
    /// <summary>
    /// The initializer starts the idle pose with off-screen processing off; the real jump preparation and apex handoffs
    /// start the jumping and falling animations, which switch it on. Each shows its frames and holds the last; the update
    /// that reaches the native sleep still reports the held frame as new.
    /// </summary>
    internal static void VerifyChootBehavior(ImportEnemyArtwork? artwork = null)
    {
        var enemies = EnemySystemFixtures.New((artwork is null ? EnemySessionFixtures.WithInstalledServices() : EnemySessionFixtures.WithInstalledServices(artwork.Enemies)), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: artwork?.Bosses);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Choot, place.KeptDefinition);
        place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        place.SetSpawnParameter1(EnemyDefinitionId.Choot, 1);
        place.SetSpawnParameter2(EnemyDefinitionId.Choot, 0);
        place.Position = place.Position.WithWhole(new(256, 256));
        // The Choot's setup builds the state its steps and its animation, which the check plays, run on.
        ChootEnemyState state = ChootBehavior.InitializeChoot(place.HeldOccupant().SpawnSettings<ChootSpawnSettings>(), place);
        void Animate() => ChootBehavior.AdvanceChootTypedAnimation(place, state);

        ushort[] idle = ChootAnimationsConstants.Spritemaps(ChootSegment.Idle).ToArray();
        // A still picture shows once and holds (#2275).
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([idle[0]], [1]),
            EnemyInstructionRunner.ShownFrames(place, 3, Animate), "Choot idle pose");
        AssertTrue(!place.Processing.HasAny(EnemyProcessing.OffScreen), "Choot idle program disables off-screen processing");

        state.JumpDelayTimer = UpdateCountdown.Ended();
        ChootBehavior.PrepareChootJump(place, state);
        ushort[] jumping = ChootAnimationsConstants.Spritemaps(ChootSegment.Jumping).ToArray();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([jumping[0], jumping[1]], [8, 1]),
            EnemyInstructionRunner.ShownFrames(place, 10, Animate), "Choot real preparation handoff starts the jumping animation");
        AssertTrue(place.Processing.HasAny(EnemyProcessing.OffScreen), "Choot jumping program enables off-screen processing");

        state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(1);
        ChootBehavior.RunChootJump(place.HeldOccupant(), place, state);
        ushort[] falling = ChootAnimationsConstants.Spritemaps(ChootSegment.Falling).ToArray();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([falling[0], falling[1]], [8, 1]),
            EnemyInstructionRunner.ShownFrames(place, 10, Animate), "Choot real apex handoff starts the falling animation");
        Console.WriteLine(artwork is null
            ? "  Choot behavior: idle, jumping and falling animations and their off-screen switches run as the cartridge does."
            : "  Installed Choot: idle, jumping and falling animations run with stock artwork.");
    }

    /// <summary>Runs the Choot behavior check and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--choot-instruction-mechanics")]
    internal static int ChootInstructionMechanicsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyChootBehavior), () => VerifyChootBehavior());
        Suite(nameof(ChootAnimationsVerification.VerifyChootAnimations), () => ChootAnimationsVerification.VerifyChootAnimations(rom));
        return 0;
    }
}
