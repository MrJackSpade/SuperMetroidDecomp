using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Ripper;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the GRipper and Ripper II state the variants own: the 16.16 velocity from the speed record.</summary>
internal static class RipperVariantOwnedStateVerification
{
    /// <summary>
    /// A Ripper II initialized toward negative speed holds its record's negative half as one 16.16 velocity, a reversal
    /// loads the positive half.
    /// </summary>
    internal static void VerifyRipperVariantOwnedState()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Ripper2, slot.Definition);
        const int speedRecord = 3;
        slot.SetSpawnParameter1(EnemyDefinitionId.Ripper2, speedRecord);
        slot.SetSpawnParameter2(EnemyDefinitionId.Ripper2, 0);
        RipperBehavior.InitializeRipper2(slot);
        RipperEnemyState state = slot.SpeciesState<RipperEnemyState>()
            ?? throw new InvalidOperationException("The Ripper II has no state.");

        EnemyLinearSpeedRow speeds = EnemyLinearSpeeds.Row(speedRecord);
        SignedSixteenSixteen negative = speeds.Negative;
        SignedSixteenSixteen positive = speeds.Positive;
        AssertEqual(speeds, state.Speeds, "the speed row");
        AssertEqual(negative, state.XVelocity, "the negative half is loaded");
        RipperBehavior.ReverseRipperVariant(slot, RipperSegment.Ripper2MovingLeft, RipperSegment.Ripper2MovingRight);
        AssertEqual(positive, state.XVelocity, "a reversal loads the positive half");
        Console.WriteLine("Ripper variant owned state: the record's velocity halves and the reversal match.");
    }

    /// <summary>Runs the focused Ripper variant owned-state check.</summary>
    [VerificationCommand("--ripper-variant-owned-state")]
    internal static int RipperVariantOwnedStateCommand()
    {
        VerifyRipperVariantOwnedState();
        return 0;
    }
}
