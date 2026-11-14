using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Skultera;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Skultera state the species owns: its oscillation phase wrap and speeds.</summary>
internal static class SkulteraOwnedStateVerification
{
    /// <summary>
    /// Initializes a Skultera and confirms its speeds are the linear-speed record's halves, its radius and angle step
    /// come from parameter two, and the phase wraps through a full turn in both directions as the cartridge's
    /// low-byte sum does.
    /// </summary>
    internal static void VerifySkulteraOwnedState()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Skultera, slot.Definition);
        const int speedRecord = 3;
        slot.SetSpawnParameter1(EnemyDefinitionId.Skultera, speedRecord);
        slot.SetSpawnParameter2(EnemyDefinitionId.Skultera, LittleEndianBytes.Word(20, 10));
        SkulteraBehavior.InitializeSkultera(slot);
        SkulteraEnemyState state = enemies.SkulteraStates[0]
            ?? throw new InvalidOperationException("The initialized Skultera has no state.");

        SignedSixteenSixteen right = EnemyLinearSpeedTable.Speed(speedRecord);
        SignedSixteenSixteen left = -right;
        AssertEqual(right, state.RightVelocity, "rightward speed");
        AssertEqual(left, state.LeftVelocity, "leftward speed");
        AssertEqual(new VerifySkulteraOwnedStateCase(20, 10), new VerifySkulteraOwnedStateCase(state.Radius, state.AngleDelta), "radius and angle step come from parameter two");

        state.Angle = RotationAngle.FromSteps(250);
        state.AdvanceAngle();
        AssertEqual((byte)4, state.Angle.ToTrigonometryAngle().RawValue, "the phase wraps forward through a full turn");
        state.AngleDelta = -state.AngleDelta;
        state.AdvanceAngle();
        AssertEqual((byte)250, state.Angle.ToTrigonometryAngle().RawValue, "the phase wraps backward through a full turn");
        Console.WriteLine("Skultera owned state: speeds and phase wrap match.");
    }

    /// <summary>Runs the focused Skultera owned-state check.</summary>
    [VerificationCommand("--skultera-owned-state")]
    internal static int SkulteraOwnedStateCommand()
    {
        VerifySkulteraOwnedState();
        return 0;
    }
}
