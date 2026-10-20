using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpacePirates;
namespace SuperMetroid.Verification.Enemies.SpacePirates;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties the NinjaSpacePirate values the cartridge keeps in the enemy's common record.</summary>
internal static class NinjaSpacePirateRecordStateVerification
{
    /// <summary>
    /// Installs a NinjaSpacePirate state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyNinjaSpacePirateRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new NinjaSpacePirateEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = Enum.GetValues<NinjaSpacePirateFunction>()[^1];
        state.CalculatedSpinJumpSpeed = 1;
        state.PostsMidpoint = new Position16<RoomPixels>(1, 0);
        state.LeftPost = new Position16<RoomPixels>(1, 0);
        state.RightPost = new Position16<RoomPixels>(1, 0);

        slot.Clear();
        AssertEqual<NinjaSpacePirateFunction?>(null, state.Function, "a kill empties Function");
        AssertEqual<int>(0, state.CalculatedSpinJumpSpeed, "a kill empties CalculatedSpinJumpSpeed");
        AssertEqual<Position16<RoomPixels>>(Position16<RoomPixels>.Zero, state.PostsMidpoint, "a kill empties PostsMidpoint");
        AssertEqual<Position16<RoomPixels>>(Position16<RoomPixels>.Zero, state.LeftPost, "a kill empties LeftPost");
        AssertEqual<Position16<RoomPixels>>(Position16<RoomPixels>.Zero, state.RightPost, "a kill empties RightPost");
        Console.WriteLine("NinjaSpacePirate record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused NinjaSpacePirate record-state check.</summary>
    [VerificationCommand("--ninja-space-pirate-record-state")]
    internal static int NinjaSpacePirateRecordStateCommand()
    {
        VerifyNinjaSpacePirateRecordState();
        return 0;
    }
}
