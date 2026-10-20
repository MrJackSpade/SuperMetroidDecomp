using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Cacatac;
namespace SuperMetroid.Verification.Enemies.Cacatac;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that a kill empties the Cacatac values the cartridge keeps in the enemy's common record.</summary>
internal static class CacatacRecordStateVerification
{
    /// <summary>
    /// Installs a Cacatac state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyCacatacRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new CacatacEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.RightVelocity = SnesSignedSixteenSixteen.FromParts(1, 0);
        state.LeftVelocity = SnesSignedSixteenSixteen.FromParts(1, 0);
        state.Direction = Enum.GetValues<CacatacDirection>()[^1];
        state.Function = Enum.GetValues<CacatacEnemyFunction>()[^1];

        slot.Clear();
        AssertEqual<SnesSignedSixteenSixteen>(SnesSignedSixteenSixteen.Zero, state.RightVelocity, "a kill empties RightVelocity");
        AssertEqual<SnesSignedSixteenSixteen>(SnesSignedSixteenSixteen.Zero, state.LeftVelocity, "a kill empties LeftVelocity");
        AssertEqual<CacatacDirection>(default(CacatacDirection), state.Direction, "a kill empties Direction");
        AssertEqual<CacatacEnemyFunction?>(null, state.Function, "a kill empties Function");
        Console.WriteLine("Cacatac record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Cacatac record-state check.</summary>
    [VerificationCommand("--cacatac-record-state")]
    internal static int CacatacRecordStateCommand()
    {
        VerifyCacatacRecordState();
        return 0;
    }
}
