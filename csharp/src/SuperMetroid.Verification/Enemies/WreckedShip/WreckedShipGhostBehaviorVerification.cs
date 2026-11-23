using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.WreckedShip;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that the Wrecked Ship ghost starts and runs its floating loop.</summary>
internal static class WreckedShipGhostBehaviorVerification
{
    /// <summary>The real initializer starts the floating loop, which shows its three poses sixteen updates apiece.</summary>
    internal static void VerifyWreckedShipGhostBehavior()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace ghost = enemies.Places.InUpdateOrder[0];
        ghost.Bind(EnemyDefinitionId.Coven, ghost.KeptDefinition);
        ghost.Bind(ghost.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        // The ghost's setup builds the state its animation, which the check plays, runs on.
        WreckedShipGhostEnemyState state = WreckedShipGhostBehavior.InitializeWreckedShipGhost(ghost);
        AssertEqual(new AnimationPlayback<WreckedShipGhostSegment>(WreckedShipGhostAnimations.StartingWith(WreckedShipGhostSegment.Floating), AnimationPlace.StartOfAnimation),
            state.Playback,
            "real Wrecked Ship ghost initializer starts the floating loop");
        ushort[] loop = WreckedShipGhostAnimationsTooling.Spritemaps(WreckedShipGhostSegment.Floating).ToArray();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. loop, loop[0]], [16, 16, 16, 16]),
            EnemyInstructionRunner.ShownFrames(ghost, 49, () => WreckedShipGhostBehavior.AdvanceWreckedShipGhostTypedAnimation(ghost, state)),
            "Wrecked Ship ghost floating loop and its goto");
        Console.WriteLine("  Wrecked Ship ghost behavior: the initializer starts and runs the floating loop.");
    }

    /// <summary>Dispatches the cartridge-backed ghost behavior check and program oracle.</summary>
    [VerificationCommand("--wrecked-ship-ghost-instruction-mechanics")]
    internal static int WreckedShipGhostInstructionMechanicsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyWreckedShipGhostBehavior), () => VerifyWreckedShipGhostBehavior());
        Suite(nameof(WreckedShipGhostAnimationsVerification.VerifyWreckedShipGhostAnimations),
            () => WreckedShipGhostAnimationsVerification.VerifyWreckedShipGhostAnimations(rom));
        return 0;
    }
}
