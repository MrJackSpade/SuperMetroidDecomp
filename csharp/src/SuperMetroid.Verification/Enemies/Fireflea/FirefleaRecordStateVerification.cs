using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Fireflea;
namespace SuperMetroid.Verification.Enemies.Fireflea;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties the Fireflea values the cartridge keeps in the enemy's common record.</summary>
internal static class FirefleaRecordStateVerification
{
    /// <summary>
    /// Installs a Fireflea state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyFirefleaRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new FirefleaEnemyState(slot, new FirefleaExtensionWords());
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.AngularSpeed = SignedSixteenSixteen.FromParts(1, 0);
        state.Radius = 1;
        state.Angle = RotationAngle.FromTableIndex(1);
        state.Center = new Position16<RoomPixels>(1, 0);

        slot.Clear();
        AssertEqual<SignedSixteenSixteen>(SignedSixteenSixteen.Zero, state.AngularSpeed, "a kill empties AngularSpeed");
        AssertEqual<int>(0, state.Radius, "a kill empties Radius");
        AssertEqual<RotationAngle>(RotationAngle.FromTableIndex(0), state.Angle, "a kill empties Angle");
        AssertEqual<Position16<RoomPixels>>(Position16<RoomPixels>.Zero, state.Center, "a kill empties Center");
        Console.WriteLine("Fireflea record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Fireflea record-state check.</summary>
    [VerificationCommand("--fireflea-record-state")]
    internal static int FirefleaRecordStateCommand()
    {
        VerifyFirefleaRecordState();
        return 0;
    }
}
