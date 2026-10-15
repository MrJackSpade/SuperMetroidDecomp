using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Enemies.WreckedShip.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.WreckedShip;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that the Wrecked Ship ghost starts and runs its floating loop.</summary>
internal static class WreckedShipGhostBehaviorVerification
{
    /// <summary>The real initializer starts the floating loop, which shows its three poses sixteen updates apiece.</summary>
    internal static void VerifyWreckedShipGhostBehavior(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(rom);
        enemies.Presentation._cgram = new SnesCgram();
        RoomEnemySlot ghost = enemies.SlotTable.Slots[0];
        ghost.EnemyDefinitionPointer = EnemyDefinitionId.Coven;
        ghost.Definition = default(RoomEnemyDefinition) with { Bank = WreckedShipGhostFrameDefinitions.Bank };
        enemies.WreckedShipGhostBehavior.InitializeWreckedShipGhost(ghost);
        AssertEqual(AnimationProgramCursor<WreckedShipGhostProgram>.Start(WreckedShipGhostProgram.Floating),
            (enemies.WreckedShipGhostBehavior._wreckedShipGhostStates[0] ?? throw new InvalidOperationException("The ghost has no state.")).Program,
            "real Wrecked Ship ghost initializer starts the floating loop");
        ushort[] loop = WreckedShipGhostAnimations.Spritemaps(WreckedShipGhostProgram.Floating).ToArray();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. loop, loop[0]], [16, 16, 16, 16]),
            EnemyInstructionRunner.ShownFrames(enemies, ghost, 49), "Wrecked Ship ghost floating loop and its goto");
        Console.WriteLine("  Wrecked Ship ghost behavior: the initializer starts and runs the floating loop.");
    }

    /// <summary>Dispatches the cartridge-backed ghost behavior check and program oracle.</summary>
    [VerificationCommand("--wrecked-ship-ghost-instruction-mechanics")]
    internal static int WreckedShipGhostInstructionMechanicsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyWreckedShipGhostBehavior), () => VerifyWreckedShipGhostBehavior(rom));
        Suite(nameof(WreckedShipGhostAnimationsVerification.VerifyWreckedShipGhostAnimations),
            () => WreckedShipGhostAnimationsVerification.VerifyWreckedShipGhostAnimations(rom));
        return 0;
    }
}
