using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.FaceBlocks;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;

/// <summary>Checks the side the face block's main AI finds Samus on once she is in vertical range.</summary>
internal static class BlueBrinstarFaceBlockHorizontalDistanceVerification
{
    /// <summary>Room X of the face block in the ordinary cases.</summary>
    private const int BlockX = 256;

    /// <summary>Room X of the face block in the wrapping case, near the left edge of the coordinate word.</summary>
    private const int WrappingBlockX = 8;

    /// <summary>Room Y shared by the face block and Samus, so the vertical range test always passes.</summary>
    private const int RowY = 256;

    /// <summary>
    /// Main AI finds Samus's side from her X minus the block's X as a signed distance: to the left, to the right, and to
    /// the left when Samus's room X wraps below zero behind a block at the left edge of the coordinate word. A block
    /// wakes only for Samus on the side it faces, so each side is seen by whether the block wakes.
    /// </summary>
    internal static void VerifyBlueBrinstarFaceBlockHorizontalDistance()
    {
        (bool leftWoke, bool facesRight) = Wakes(BlockX, BlockX - 32);
        AssertEqual(!facesRight, leftWoke, "face block finds Samus 32 pixels left on its left");
        (bool rightWoke, _) = Wakes(BlockX, BlockX + 32);
        AssertEqual(facesRight, rightWoke, "face block finds Samus 32 pixels right on its right");
        (bool wrappedWoke, _) = Wakes(WrappingBlockX, ushort.MaxValue + 1 + WrappingBlockX - 16);
        AssertEqual(!facesRight, wrappedWoke, "face block finds Samus 16 pixels left across the wrap on its left");
        Console.WriteLine("  Face block horizontal distance: left, right and wrapped room-coordinate sides pass.");
    }

    /// <summary>
    /// Runs one main-AI update with Morph Ball collected and Samus level with the block at <paramref name="samusX"/>; returns
    /// whether the block woke, and which way it faces.
    /// </summary>
    private static (bool Woke, bool FacesRight) Wakes(int blockX, int samusX)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.FaceBlock, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SetSpawnParameter1(EnemyDefinitionId.FaceBlock, 64);
        slot.SetSpawnParameter2(EnemyDefinitionId.FaceBlock, 0);
        slot.Position = slot.Position.WithWhole(new(checked((ushort)blockX), RowY));
        enemies.BlueBrinstarFaceBlockBehavior.InitializeBlueBrinstarFaceBlock(slot, null);
        BlueBrinstarFaceBlockEnemyState state = enemies.BlueBrinstarFaceBlockStates[0]
            ?? throw new InvalidOperationException("The face block has no state.");
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            CollectedItems = SamusEquipmentFlags.MorphBall,
            Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)samusX), RowY)),
        };
        enemies.BlueBrinstarFaceBlockBehavior.RunBlueBrinstarFaceBlockMain(slot, state, samus);
        return (state.Activated, state.FacesRight);
    }

    /// <summary>Runs the face-block horizontal distance check through its dedicated command-line switch.</summary>
    [VerificationCommand("--face-block-horizontal-distance")]
    internal static int FaceBlockHorizontalDistanceCommand()
    {
        Suite(nameof(VerifyBlueBrinstarFaceBlockHorizontalDistance), () => VerifyBlueBrinstarFaceBlockHorizontalDistance());
        return 0;
    }
}
