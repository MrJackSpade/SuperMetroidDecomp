using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Kago;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that Kago starts its slow loop and switches to the post-hit loop when first shot.</summary>
internal static class KagoBehaviorVerification
{
    /// <summary>
    /// The initializer starts the slow loop, which shows its four frames ten updates apiece; the first accepted shot
    /// starts the fast loop on the next update, three updates apiece, while keeping its bug-spawning side effect.
    /// </summary>
    internal static void VerifyKagoBehavior()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().ReadRandom = () => 0;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Kago, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SetSpawnParameter1(EnemyDefinitionId.Kago, 10);
        KagoBehavior.InitializeKago(slot);
        KagoEnemyState state = enemies.KagoStates[0] ?? throw new InvalidDataException("Kago initializer did not publish typed state.");
        AssertEqual(AnimationProgramCursor<KagoProgram>.Start(KagoProgram.Slow), state.Program, "Kago initializer animation");
        ushort[] slow = KagoAnimationsConstants.Spritemaps(KagoProgram.Slow).ToArray();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. slow, slow[0]], [10, 10, 10, 10, 10]),
            EnemyInstructionRunner.ShownFrames(slot, 41), "Kago slow loop");

        enemies.KagoBehavior.ResolveKagoShotAfterCommon(slot, state);
        AssertTrue(state.UsesFastAnimation, "Kago real shot handoff selects fast animation");
        AssertEqual(AnimationProgramCursor<KagoProgram>.Start(KagoProgram.Fast), state.Program, "Kago shot animation");
        AssertEqual(1, state.SpawnedBugCount, "Kago real shot handoff retains its projectile side effect");
        ushort[] fast = KagoAnimationsConstants.Spritemaps(KagoProgram.Fast).ToArray();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. fast, fast[0]], [3, 3, 3, 3, 3]),
            EnemyInstructionRunner.ShownFrames(slot, 13), "Kago post-hit loop");
        Console.WriteLine("  Kago behavior: the slow loop and the shot handoff to the post-hit loop run as the cartridge does.");
    }

    /// <summary>Runs the Kago behavior checks and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--kago-instruction-mechanics")]
    internal static int KagoInstructionMechanicsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyKagoBehavior), () => VerifyKagoBehavior());
        Suite(nameof(KagoAnimationsVerification.VerifyKagoAnimations), () => KagoAnimationsVerification.VerifyKagoAnimations(rom));
        return 0;
    }
}
