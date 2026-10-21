using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Dragon;
namespace SuperMetroid.Verification.Enemies.Dragon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Dragon state each record owns: the body/wing selection and the cleared record.</summary>
internal static class DragonOwnedStateVerification
{
    /// <summary>
    /// Initializes a body and its wing and confirms parameter one selects the wing, the body starts waiting to rise,
    /// and a kill empties the body's facing, countdowns and function while its animation selection outlives it.
    /// </summary>
    internal static void VerifyDragonOwnedState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.Dragon, body.Definition);
        DragonBehavior.InitializeDragon(body);
        RoomEnemySlot wing = enemies.SlotTable.Slots[1];
        wing.Bind(EnemyDefinitionId.Dragon, wing.Definition);
        wing.SetSpawnParameter1(1);
        DragonBehavior.InitializeDragon(wing);
        DragonEnemyState bodyState = enemies.DragonStates[0] ?? throw new InvalidOperationException("The Dragon body has no state.");
        DragonEnemyState wingState = enemies.DragonStates[1] ?? throw new InvalidOperationException("The Dragon wing has no state.");
        AssertTrue(!bodyState.IsWing && wingState.IsWing, "parameter one selects the wing");
        AssertEqual(DragonEnemyFunction.WaitToRise, bodyState.Function, "the body starts waiting to rise");
        AssertEqual(DragonEnemyFunction.WingNoOp, wingState.Function, "the wing runs no body AI");

        bodyState.FacingLeft = true;
        bodyState.AttackCounter = WrappingCountdown.Start(2);
        body.Clear();
        AssertTrue(!bodyState.FacingLeft, "a kill empties the facing");
        AssertEqual((ushort)0, bodyState.AttackCounter.RawValue, "a kill empties the shot counter");
        AssertEqual(null, bodyState.Function, "a kill empties the function");
        AssertEqual(DragonAnimationSelector.IdleFacingLeft, bodyState.InstalledInstructionListIndex, "the installed animation outlives the kill");
        Console.WriteLine("Dragon owned state: the body/wing selection and the cleared record match.");
    }

    /// <summary>Runs the focused Dragon owned-state check.</summary>
    [VerificationCommand("--dragon-owned-state")]
    internal static int DragonOwnedStateCommand()
    {
        VerifyDragonOwnedState();
        return 0;
    }
}
