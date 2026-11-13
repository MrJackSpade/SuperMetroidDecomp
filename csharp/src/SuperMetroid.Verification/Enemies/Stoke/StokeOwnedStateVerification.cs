using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Stoke;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Stoke state the species owns: its 16.16 walking speeds.</summary>
internal static class StokeOwnedStateVerification
{
    /// <summary>
    /// Initializes a Stoke for every linear-speed record and confirms each walking speed is that record's half as
    /// one signed 16.16 value.
    /// </summary>
    internal static void VerifyStokeOwnedState()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        for (int record = 0; record < EnemyLinearSpeedDefinitions.RowCount; record++)
        {
            slot.Bind(EnemyDefinitionId.Stoke, slot.Definition);
            slot.SetSpawnParameter1(EnemyDefinitionId.Stoke, (ushort)StokeDirection.Right);
            slot.SetSpawnParameter2(EnemyDefinitionId.Stoke, (ushort)record);
            StokeBehavior.InitializeStoke(slot);
            StokeEnemyState state = enemies.StokeStates[0] ?? throw new InvalidOperationException("The initialized Stoke has no state.");
            SignedSixteenSixteen right = EnemyLinearSpeeds.Row(record).Positive;
            SignedSixteenSixteen left = EnemyLinearSpeeds.Row(record).Negative;
            AssertEqual(right, state.RightVelocity, $"record {record} rightward speed");
            AssertEqual(left, state.LeftVelocity, $"record {record} leftward speed");
            AssertEqual(StokeAction.Moving, state.Action, $"record {record} walks");
            AssertEqual(StokeDirection.Right, state.Direction, $"record {record} walks right");
        }
        Console.WriteLine("Stoke owned state: all linear-speed records match.");
    }

    /// <summary>Runs the focused Stoke owned-state check.</summary>
    [VerificationCommand("--stoke-owned-state")]
    internal static int StokeOwnedStateCommand()
    {
        VerifyStokeOwnedState();
        return 0;
    }
}
