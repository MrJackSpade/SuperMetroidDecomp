using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Zoa;
namespace SuperMetroid.Verification.Enemies.Zoa;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that a kill empties the Zoa values the cartridge keeps in the enemy's common record.</summary>
internal static class ZoaRecordStateVerification
{
    /// <summary>
    /// Installs a Zoa state on a slot with its record-clearing operation, gives every value the record holds a
    /// nonzero value, and confirms a kill empties each one.
    /// </summary>
    internal static void VerifyZoaRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()));
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        var state = new ZoaEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.SpawnPosition = new Position16<RoomPixels>(1, 0);
        state.InstructionListTableIndex = Enum.GetValues<ZoaAnimationSelector>()[^1];
        state.PreviousInstructionListTableIndex = Enum.GetValues<ZoaAnimationSelector>()[^1];
        state.Function = Enum.GetValues<ZoaEnemyFunction>()[^1];

        slot.Clear();
        AssertEqual<Position16<RoomPixels>>(Position16<RoomPixels>.Zero, state.SpawnPosition, "a kill empties SpawnPosition");
        AssertEqual<ZoaAnimationSelector>(default(ZoaAnimationSelector), state.InstructionListTableIndex, "a kill empties InstructionListTableIndex");
        AssertEqual<ZoaAnimationSelector>(default(ZoaAnimationSelector), state.PreviousInstructionListTableIndex, "a kill empties PreviousInstructionListTableIndex");
        AssertEqual<ZoaEnemyFunction?>(null, state.Function, "a kill empties Function");
        Console.WriteLine("Zoa record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused Zoa record-state check.</summary>
    [VerificationCommand("--zoa-record-state")]
    internal static int ZoaRecordStateCommand()
    {
        VerifyZoaRecordState();
        return 0;
    }
}
