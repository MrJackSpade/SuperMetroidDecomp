using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Choot;
using SuperMetroid.Core.Enemies.Choot.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Choot;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Choot's idle, jumping and falling animations and their off-screen processing switches.</summary>
internal static class ChootBehaviorVerification
{
    /// <summary>
    /// The initializer starts the idle pose with off-screen processing off; the real jump preparation and apex handoffs
    /// start the jumping and falling animations, which switch it on. Each shows its frames and holds the last; the update
    /// that reaches the native sleep still reports the held frame as new.
    /// </summary>
    internal static void VerifyChootBehavior(EnemyTileArtworkCatalog? artwork = null)
    {
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Choot, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = ChootFrameToolingDefinitions.Bank });
        slot.SetSpawnParameter1(1);
        slot.SetSpawnParameter2(0);
        slot.Position = slot.Position.WithWhole(new(256, 256));
        enemies.ChootBehavior.InitializeChoot(slot);
        ChootEnemyState state = enemies.ChootStates[0] ?? throw new InvalidOperationException("Verification requires enemies.ChootStates[0].");

        AssertEqual(AnimationProgramCursor<ChootProgram>.Start(ChootProgram.Idle), state.Program, "Choot initializer animation");
        ushort[] idle = ChootAnimationsConstants.Spritemaps(ChootProgram.Idle).ToArray();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([idle[0], idle[0]], [1, 1]),
            EnemyInstructionRunner.ShownFrames(slot, 3), "Choot idle pose");
        AssertTrue(!slot.Properties.HasAny(EnemyProperties.ProcessOffScreen), "Choot idle program disables off-screen processing");

        state.JumpDelayTimer = NativeCountdown.Start(0);
        ChootBehavior.PrepareChootJump(slot, state);
        AssertEqual(AnimationProgramCursor<ChootProgram>.Start(ChootProgram.Jumping), state.Program,
            "Choot real preparation handoff starts the jumping animation");
        ushort[] jumping = ChootAnimationsConstants.Spritemaps(ChootProgram.Jumping).ToArray();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([jumping[0], jumping[1], jumping[1]], [8, 1, 1]),
            EnemyInstructionRunner.ShownFrames(slot, 10), "Choot jumping animation");
        AssertTrue(slot.Properties.HasAny(EnemyProperties.ProcessOffScreen), "Choot jumping program enables off-screen processing");

        state.YSpeedTableIndex = 1;
        ChootBehavior.RunChootJump(slot, state);
        AssertEqual(AnimationProgramCursor<ChootProgram>.Start(ChootProgram.Falling), state.Program,
            "Choot real apex handoff starts the falling animation");
        ushort[] falling = ChootAnimationsConstants.Spritemaps(ChootProgram.Falling).ToArray();
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
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyChootBehavior), () => VerifyChootBehavior());
        Suite(nameof(ChootAnimationsVerification.VerifyChootAnimations), () => ChootAnimationsVerification.VerifyChootAnimations(rom));
        return 0;
    }
}
