using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Stoke;
namespace SuperMetroid.Verification.Enemies.Stoke;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Stoke state the species owns: its 16.16 walking speeds and the cleared record.</summary>
internal static class StokeOwnedStateVerification
{
    /// <summary>
    /// Initializes a Stoke for every linear-speed record and confirms each walking speed is that record's half as
    /// one signed 16.16 value, then confirms a kill empties the record's speeds, facing and behaviour.
    /// </summary>
    internal static void VerifyStokeOwnedState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        StokeEnemyState? state = null;
        for (int record = 0; record < EnemyLinearSpeedDefinitions.RecordCount; record++)
        {
            slot.Bind(EnemyDefinitionId.Stoke, slot.Definition);
            slot.SetSpawnParameter1((ushort)StokeDirection.Right);
            slot.SetSpawnParameter2((ushort)record);
            StokeBehavior.InitializeStoke(slot);
            state = enemies.StokeStates[0] ?? throw new InvalidOperationException("The initialized Stoke has no state.");
            SnesSignedSixteenSixteen right = EnemyLinearSpeedDefinitions.Read(record * EnemyLinearSpeedDefinitions.RecordSize);
            SnesSignedSixteenSixteen left = EnemyLinearSpeedDefinitions.Read(record * EnemyLinearSpeedDefinitions.RecordSize + 4);
            AssertEqual(right, state.RightVelocity, $"record {record} rightward speed");
            AssertEqual(left, state.LeftVelocity, $"record {record} leftward speed");
            AssertEqual(StokeAiFunction.MovingRight, state.Function, $"record {record} walks right");
        }

        slot.Clear();
        StokeEnemyState cleared = state ?? throw new InvalidOperationException("No Stoke was initialized.");
        AssertEqual(SnesSignedSixteenSixteen.Zero, cleared.RightVelocity, "a kill empties the rightward speed");
        AssertEqual(SnesSignedSixteenSixteen.Zero, cleared.LeftVelocity, "a kill empties the leftward speed");
        AssertEqual(StokeDirection.Left, cleared.Direction, "a kill empties the facing");
        AssertEqual(null, cleared.Function, "a kill empties the behaviour");
        Console.WriteLine("Stoke owned state: all linear-speed records and the cleared record match.");
    }

    /// <summary>Runs the focused Stoke owned-state check.</summary>
    [VerificationCommand("--stoke-owned-state")]
    internal static int StokeOwnedStateCommand()
    {
        VerifyStokeOwnedState();
        return 0;
    }
}
