using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Ripper;
namespace SuperMetroid.Verification.Enemies.Ripper;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the GRipper and Ripper II state the variants own: the 16.16 velocity from the speed record and the cleared record.</summary>
internal static class RipperVariantOwnedStateVerification
{
    /// <summary>
    /// A Ripper II initialized toward negative speed holds its record's negative half as one 16.16 velocity, a reversal
    /// loads the positive half, and a kill empties the velocity and record.
    /// </summary>
    internal static void VerifyRipperVariantOwnedState()
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Ripper2, slot.Definition);
        const int speedRecord = 3;
        slot.Parameter1 = speedRecord;
        slot.Parameter2 = 0;
        enemies.RipperBehavior.InitializeRipper2(slot);
        RipperVariantEnemyState state = slot.SpeciesState<RipperVariantEnemyState>()
            ?? throw new InvalidOperationException("The Ripper II has no state.");

        (short negativeWhole, ushort negativeFraction) = EnemyLinearSpeedDefinitions.Read(speedRecord * EnemyLinearSpeedDefinitions.RecordSize + 4);
        (short positiveWhole, ushort positiveFraction) = EnemyLinearSpeedDefinitions.Read(speedRecord * EnemyLinearSpeedDefinitions.RecordSize);
        AssertEqual(speedRecord * EnemyLinearSpeedDefinitions.RecordSize, state.SpeedTableByteOffset, "the speed record's byte offset");
        AssertEqual(SnesSignedSixteenSixteen.FromParts(negativeWhole, negativeFraction), state.XVelocity, "the negative half is loaded");
        enemies.RipperBehavior.ReverseRipperVariant(slot, RipperProgram.Ripper2MovingLeft, RipperProgram.Ripper2MovingRight);
        AssertEqual(SnesSignedSixteenSixteen.FromParts(positiveWhole, positiveFraction), state.XVelocity, "a reversal loads the positive half");

        slot.Clear();
        AssertEqual((SnesSignedSixteenSixteen.Zero, 0), (state.XVelocity, state.SpeedTableByteOffset), "a kill empties the velocity and record");
        Console.WriteLine("Ripper variant owned state: the record's velocity halves, the reversal and the cleared record match.");
    }

    /// <summary>Runs the focused Ripper variant owned-state check.</summary>
    [VerificationCommand("--ripper-variant-owned-state")]
    internal static int RipperVariantOwnedStateCommand()
    {
        VerifyRipperVariantOwnedState();
        return 0;
    }
}
