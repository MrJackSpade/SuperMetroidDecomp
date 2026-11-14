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
        var enemies = new RoomEnemySystem((artwork is null ? EnemySessionFixtures.WithInstalledServices() : EnemySessionFixtures.WithInstalledServices(artwork.Enemies)), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = artwork?.Bosses };
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Choot, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SetSpawnParameter1(EnemyDefinitionId.Choot, 1);
        slot.SetSpawnParameter2(EnemyDefinitionId.Choot, 0);
        slot.Position = slot.Position.WithWhole(new(256, 256));
        ChootBehavior.InitializeChoot(slot);
        ChootEnemyState state = enemies.ChootStates[0] ?? throw new InvalidOperationException("Verification requires enemies.ChootStates[0].");

        AssertEqual(new AnimationPlayback<ChootSegment>(ChootAnimations.StartingWith(ChootSegment.Idle), AnimationPlace.StartOfAnimation), state.Playback, "Choot initializer animation");
        ushort[] idle = ChootAnimationsConstants.Spritemaps(ChootSegment.Idle).ToArray();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([idle[0], idle[0]], [1, 1]),
            EnemyInstructionRunner.ShownFrames(slot, 3), "Choot idle pose");
        AssertTrue(!slot.Processing.HasAny(EnemyProcessing.OffScreen), "Choot idle program disables off-screen processing");

        state.JumpDelayTimer = 0;
        ChootBehavior.PrepareChootJump(slot, state);
        AssertEqual(new AnimationPlayback<ChootSegment>(ChootAnimations.StartingWith(ChootSegment.Jumping), AnimationPlace.StartOfAnimation), state.Playback,
            "Choot real preparation handoff starts the jumping animation");
        ushort[] jumping = ChootAnimationsConstants.Spritemaps(ChootSegment.Jumping).ToArray();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([jumping[0], jumping[1], jumping[1]], [8, 1, 1]),
            EnemyInstructionRunner.ShownFrames(slot, 10), "Choot jumping animation");
        AssertTrue(slot.Processing.HasAny(EnemyProcessing.OffScreen), "Choot jumping program enables off-screen processing");

        state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(1);
        ChootBehavior.RunChootJump(slot, state);
        AssertEqual(new AnimationPlayback<ChootSegment>(ChootAnimations.StartingWith(ChootSegment.Falling), AnimationPlace.StartOfAnimation), state.Playback,
            "Choot real apex handoff starts the falling animation");
        ushort[] falling = ChootAnimationsConstants.Spritemaps(ChootSegment.Falling).ToArray();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([falling[0], falling[1], falling[1]], [8, 1, 1]),
            EnemyInstructionRunner.ShownFrames(slot, 10), "Choot falling animation");
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
