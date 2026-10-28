using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.FaceBlocks;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;

/// <summary>Checks the face block state the species owns: its activation latch, its distance and the cleared record.</summary>
internal static class BlueBrinstarFaceBlockOwnedStateVerification
{
    /// <summary>
    /// Runs one main-AI update with Samus level with the block and 32 pixels to its left, confirms the recorded distance,
    /// then confirms a kill empties the latch and the distance kept in the common record.
    /// </summary>
    internal static void VerifyBlueBrinstarFaceBlockOwnedState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.FaceBlock, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SetSpawnParameter1(64);
        slot.SetSpawnParameter2(0);
        slot.Position = slot.Position.WithWhole(new(256, 256));
        enemies.BlueBrinstarFaceBlockBehavior.InitializeBlueBrinstarFaceBlock(slot, null);
        BlueBrinstarFaceBlockEnemyState state = enemies.BlueBrinstarFaceBlockStates[0]
            ?? throw new InvalidOperationException("The face block has no state.");
        var samus = new SamusState
        {
            CollectedItems = SamusEquipmentFlags.MorphBall,
            Position = FixedPosition<RoomPixels>.FromWhole(new(224, 256)),
        };
        enemies.BlueBrinstarFaceBlockBehavior.RunBlueBrinstarFaceBlockMain(slot, state, samus);
        AssertEqual(-32, state.LastHorizontalDelta, "the face block records Samus 32 pixels left");
        state.Activated = true;

        slot.Clear();
        AssertEqual(false, state.Activated, "a kill empties the activation latch");
        AssertEqual(0, state.LastHorizontalDelta, "a kill empties the recorded distance");
        Console.WriteLine("Face block owned state: the recorded distance and the cleared record match.");
    }

    /// <summary>Runs the focused face block owned-state check.</summary>
    [VerificationCommand("--face-block-owned-state")]
    internal static int FaceBlockOwnedStateCommand()
    {
        VerifyBlueBrinstarFaceBlockOwnedState();
        return 0;
    }
}
