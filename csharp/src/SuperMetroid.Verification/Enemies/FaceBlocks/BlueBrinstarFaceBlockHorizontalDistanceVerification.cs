using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Enemies.FaceBlocks.Artwork;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.FaceBlocks;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the signed horizontal distance the face block's main AI records once Samus is in vertical range.</summary>
internal static class BlueBrinstarFaceBlockHorizontalDistanceVerification
{
    /// <summary>Room X of the face block in the ordinary cases.</summary>
    private const int BlockX = 256;

    /// <summary>Room X of the face block in the wrapping case, near the left edge of the coordinate word.</summary>
    private const int WrappingBlockX = 8;

    /// <summary>Room Y shared by the face block and Samus, so the vertical range test always passes.</summary>
    private const int RowY = 256;

    /// <summary>
    /// Main AI records Samus's X minus the block's X as a signed distance: negative to the left, positive to the right,
    /// and negative when Samus's room X wraps below zero behind a block at the left edge of the coordinate word.
    /// </summary>
    internal static void VerifyBlueBrinstarFaceBlockHorizontalDistance()
    {
        AssertEqual(-32, RecordedDistance(BlockX, BlockX - 32), "face block records Samus 32 pixels left as -32");
        AssertEqual(32, RecordedDistance(BlockX, BlockX + 32), "face block records Samus 32 pixels right as 32");
        AssertEqual(-16, RecordedDistance(WrappingBlockX, ushort.MaxValue + 1 + WrappingBlockX - 16),
            "face block records the wrapped room-coordinate difference as -16");
        Console.WriteLine("  Face block horizontal distance: left, right and wrapped room-coordinate distances pass.");
    }

    /// <summary>Runs one main-AI update with Morph Ball collected and Samus level with the block at <paramref name="samusX"/>.</summary>
    private static int RecordedDistance(int blockX, int samusX)
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.FaceBlock, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = BlueBrinstarFaceBlockFrameToolingDefinitions.Bank });
        slot.SetSpawnParameter1(64);
        slot.SetSpawnParameter2(0);
        slot.Position = slot.Position.WithWhole(new(checked((ushort)blockX), RowY));
        enemies.BlueBrinstarFaceBlockBehavior.InitializeBlueBrinstarFaceBlock(slot, null);
        BlueBrinstarFaceBlockEnemyState state = enemies.BlueBrinstarFaceBlockStates[0]
            ?? throw new InvalidOperationException("The face block has no state.");
        var samus = new SamusState
        {
            CollectedItems = SamusEquipmentFlags.MorphBall,
            Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)samusX), RowY)),
        };
        enemies.BlueBrinstarFaceBlockBehavior.RunBlueBrinstarFaceBlockMain(slot, state, samus);
        return state.LastHorizontalDelta;
    }

    /// <summary>Runs the face-block horizontal distance check through its dedicated command-line switch.</summary>
    [VerificationCommand("--face-block-horizontal-distance")]
    internal static int FaceBlockHorizontalDistanceCommand()
    {
        Suite(nameof(VerifyBlueBrinstarFaceBlockHorizontalDistance), () => VerifyBlueBrinstarFaceBlockHorizontalDistance());
        return 0;
    }
}
