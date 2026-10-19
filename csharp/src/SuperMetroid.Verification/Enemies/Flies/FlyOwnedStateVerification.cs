using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Flies;
namespace SuperMetroid.Verification.Enemies.Flies;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the fly state the family owns: the circling phase's revolution and wrap, and the cleared record.</summary>
internal static class FlyOwnedStateVerification
{
    /// <summary>
    /// A clockwise fly's phase returns to zero after sixteen circling updates (the cartridge's 32-unit steps of its
    /// nine-bit angle), an anticlockwise step from zero wraps to the last step, and a kill empties the record.
    /// </summary>
    internal static void VerifyFlyOwnedState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Mellow, slot.Definition);
        FliesBehavior.InitializeFly(slot);
        FlyEnemyState state = slot.EntityState<FliesEntity, FlyEnemyState>() ?? throw new InvalidOperationException("The fly has no state.");

        for (int update = 1; update < 16; update++)
        {
            state.AdvanceCircle(clockwise: true);
            AssertTrue(state.Angle != SnesAngle.Zero, $"circling update {update} is mid-revolution");
        }
        state.AdvanceCircle(clockwise: true);
        AssertEqual(SnesAngle.Zero, state.Angle, "sixteen circling updates complete a revolution");
        state.AdvanceCircle(clockwise: false);
        AssertEqual((byte)240, state.Angle.TableIndex, "an anticlockwise step from zero wraps to the last step");

        state.TargetHeight = new Position16<RoomPixels>(0, 0x0123);
        slot.Clear();
        AssertEqual(SnesAngle.Zero, state.Angle, "a kill empties the phase");
        AssertEqual(Position16<RoomPixels>.Zero, state.TargetHeight, "a kill empties the target height");
        AssertEqual(null, state.Function, "a kill empties the function");
        Console.WriteLine("Fly owned state: the circling revolution, its wrap and the cleared record match.");
    }

    /// <summary>Runs the focused fly owned-state check.</summary>
    [VerificationCommand("--fly-owned-state")]
    internal static int FlyOwnedStateCommand()
    {
        VerifyFlyOwnedState();
        return 0;
    }
}
