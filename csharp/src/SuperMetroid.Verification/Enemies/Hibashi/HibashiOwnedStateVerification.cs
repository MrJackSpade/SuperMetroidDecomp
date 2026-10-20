using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Hibashi;
namespace SuperMetroid.Verification.Enemies.Hibashi;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Hibashi state each part owns: its captured population parameters, spawn height and the cleared record.</summary>
internal static class HibashiOwnedStateVerification
{
    /// <summary>
    /// Initializes a graphics part and its collision part and confirms each keeps its own population parameters (the
    /// full reset word and the part selector), the graphics part keeps its spawn height, and a kill empties the record.
    /// </summary>
    internal static void VerifyHibashiOwnedState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot graphics = enemies.SlotTable.Slots[0];
        graphics.Bind(EnemyDefinitionId.Hibashi, graphics.Definition);
        graphics.SetSpawnParameter1(ushort.MaxValue);
        graphics.SetSpawnParameter2(0);
        graphics.Position = graphics.Position.WithWhole(new(0x0040, 0x0120));
        HibashiBehavior.InitializeHibashi(graphics);
        RoomEnemySlot hitbox = enemies.SlotTable.Slots[1];
        hitbox.Bind(EnemyDefinitionId.Hibashi, hitbox.Definition);
        hitbox.SetSpawnParameter2(1);
        HibashiBehavior.InitializeHibashi(hitbox);

        HibashiEnemyState graphicsState = enemies.HibashiStates[0] ?? throw new InvalidOperationException("The graphics part has no state.");
        HibashiEnemyState hitboxState = enemies.HibashiStates[1] ?? throw new InvalidOperationException("The collision part has no state.");
        AssertTrue(!graphicsState.IsCollisionPart && hitboxState.IsCollisionPart, "parameter two selects the collision part");
        AssertEqual(ushort.MaxValue, graphicsState.InactiveTimerReset.RawValue, "the reset keeps parameter one's full word");
        AssertEqual((ushort)0x0120, graphicsState.SpawnHeight.Y, "the graphics part keeps its spawn height");
        AssertEqual(HibashiEnemyFunction.Inactive, graphicsState.Function, "the graphics part starts inactive");
        AssertEqual(null, hitboxState.Function, "the collision part runs no state machine");

        graphics.Clear();
        AssertEqual(null, graphicsState.Function, "a kill empties the function");
        AssertEqual((ushort)0, graphicsState.InactiveTimerReset.RawValue, "a kill empties the reset parameter");
        AssertEqual(Position16<RoomPixels>.Zero, graphicsState.SpawnHeight, "a kill empties the spawn height");
        Console.WriteLine("Hibashi owned state: both parts' parameters, the spawn height and the cleared record match.");
    }

    /// <summary>Runs the focused Hibashi owned-state check.</summary>
    [VerificationCommand("--hibashi-owned-state")]
    internal static int HibashiOwnedStateCommand()
    {
        VerifyHibashiOwnedState();
        return 0;
    }
}
