using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpacePirates;
namespace SuperMetroid.Verification.Enemies.SpacePirates;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the walking Space Pirate state the species owns: its patrol posts and the cleared record.</summary>
internal static class WalkingSpacePirateOwnedStateVerification
{
    /// <summary>
    /// Initializes a walking Pirate at X 256 with a patrol radius of 48 and confirms its posts lie that far either side,
    /// then confirms a kill empties the behaviour and both posts kept in the common record.
    /// </summary>
    internal static void VerifyWalkingSpacePirateOwnedState()
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.PirateGreyWalking, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = 0xb2 });
        slot.Position = slot.Position.WithWhole(new(0x0100, 0x0100));
        slot.Parameter2 = 48;
        WalkingSpacePirateBehavior.InitializeWalkingSpacePirate(slot);
        WalkingSpacePirateEnemyState state = enemies.WalkingSpacePirateStates[0]
            ?? throw new InvalidOperationException("The walking Pirate has no state.");
        AssertEqual(new Position16<RoomPixels>(0x0100 - 48, 0), state.LeftPost, "the left post lies the patrol radius left");
        AssertEqual(new Position16<RoomPixels>(0x0100 + 48, 0), state.RightPost, "the right post lies the patrol radius right");

        slot.Clear();
        AssertEqual<WalkingSpacePirateFunction?>(null, state.Function, "a kill empties the behaviour");
        AssertEqual(Position16<RoomPixels>.Zero, state.LeftPost, "a kill empties the left post");
        AssertEqual(Position16<RoomPixels>.Zero, state.RightPost, "a kill empties the right post");
        Console.WriteLine("Walking Space Pirate owned state: the patrol posts and the cleared record match.");
    }

    /// <summary>Runs the focused walking Space Pirate owned-state check.</summary>
    [VerificationCommand("--walking-space-pirate-owned-state")]
    internal static int WalkingSpacePirateOwnedStateCommand()
    {
        VerifyWalkingSpacePirateOwnedState();
        return 0;
    }
}
