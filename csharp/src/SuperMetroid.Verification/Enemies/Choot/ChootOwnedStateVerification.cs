using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Choot;
namespace SuperMetroid.Verification.Enemies.Choot;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Choot state the species owns: the ascent index's apex underflow and the cleared record.</summary>
internal static class ChootOwnedStateVerification
{
    /// <summary>
    /// Initializes a two-loop Choot and confirms its ascent starts at an even record index, the jump reaches its apex
    /// on the update that takes the index below zero (the cartridge's signed underflow of its 8.8 cursor), and a
    /// kill empties the values kept in the common record.
    /// </summary>
    internal static void VerifyChootOwnedState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Choot, slot.Definition);
        slot.Position = slot.Position.WithWhole(new(0x0100, 0x0200));
        slot.SetSpawnParameter1(2);
        enemies.ChootBehavior.InitializeChoot(slot);
        ChootEnemyState state = enemies.ChootStates[0]
            ?? throw new InvalidOperationException("The initialized Choot has no state.");

        int initial = state.InitialYSpeedTableIndex;
        AssertTrue(initial > 0 && initial % 2 == 0, "the ascent starts at a positive even record index");
        AssertEqual(initial, state.YSpeedTableIndex, "the ascent starts at the calculated index");
        int jumpingUpdates = 0;
        while (state.Function != ChootEnemyFunction.Falling)
        {
            ChootBehavior.RunChootJump(slot, state);
            jumpingUpdates++;
            AssertTrue(jumpingUpdates <= initial, "the jump reaches its apex");
        }
        AssertEqual(initial / 2 + 1, jumpingUpdates, "the apex is the update that takes the index below zero");
        AssertEqual(-2, state.YSpeedTableIndex, "the index stays below zero through the fall");
        AssertEqual(0, state.FallingPatternFrame, "the fall starts at its first frame");

        slot.Clear();
        AssertEqual(null, state.Function, "a kill empties the function");
        AssertEqual(0, state.YSpeedTableIndex, "a kill empties the ascent index");
        AssertEqual<ChootFallingPath?>(null, state.FallingPath, "a kill empties the falling pattern");
        AssertEqual(0, state.FallingPatternYDistance, "a kill empties the loop advance");
        AssertEqual(initial, state.InitialYSpeedTableIndex, "the slot's saved ascent outlives the kill");
        Console.WriteLine("Choot owned state: the apex underflow and the cleared record match.");
    }

    /// <summary>Runs the focused Choot owned-state check.</summary>
    [VerificationCommand("--choot-owned-state")]
    internal static int ChootOwnedStateCommand()
    {
        VerifyChootOwnedState();
        return 0;
    }
}
