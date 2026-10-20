using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>Checks falling-pose dash carryover and base-speed recurrence against the retail movement code.</summary>
internal static class FallingSpeedRecurrenceVerification
{
    /// <summary>Confirms extra fractional run speed selects mode two on fall entry and CPU-observed air/water deceleration alternates before clamping.</summary>
    internal static void VerifyRetailFallingSpeedRecurrence()
    {
        var level = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(new ushort[256], new byte[256], new ushort[256]), new byte[8]);
        foreach (ushort extraWhole in new ushort[] { 0, 1 })
        foreach (ushort extraFraction in new ushort[] { 0, 1 })
        foreach (SamusAccelerationMode previousMode in Enum.GetValues<SamusAccelerationMode>())
        {
            var samus = new SamusState { Pose = SamusPoseId.FacingLeftNormalPose };
            samus.PoseTransitions.ApplyStandingLeftToRunningLeft();
            samus.HorizontalSpeed.BaseSpeed = new SnesUnsignedSixteenSixteen(2, 0xc000);
            samus.HorizontalSpeed.AccelerationMode = previousMode;
            samus.HorizontalSpeed.ExtraRunSpeed = new SnesUnsignedSixteenSixteen(extraWhole, extraFraction);
            samus.PoseTransitions.ApplyWalkedOffFloorTransition(level, SamusPoseId.FallingLeftPose);
            AssertEqual(extraWhole != 0 || extraFraction != 0 ? SamusAccelerationMode.Decelerating : SamusAccelerationMode.Accelerating,
                samus.HorizontalSpeed.AccelerationMode, "fall entry selects acceleration mode from extra speed");
            AssertEqual(0x2c000u, samus.HorizontalSpeed.BaseFixed, "fall entry does not clamp base speed");
            AssertEqual(extraWhole, samus.HorizontalSpeed.ExtraRunSpeed.Whole, "fall entry preserves whole dash component");
            AssertEqual(extraFraction, samus.HorizontalSpeed.ExtraRunSpeed.Fraction, "fall entry preserves fractional dash component");
        }
        foreach (SamusLiquidMedium medium in new[] { SamusLiquidMedium.Air, SamusLiquidMedium.Water })
        {
            var speed = new SamusHorizontalSpeedState
            {
                AccelerationMode = SamusAccelerationMode.Decelerating,
                BaseSpeed = new SnesUnsignedSixteenSixteen(2, 0x9800),
            };
            speed.SelectEnvironmentSpeedTable(medium);
            for (int frame = 0; frame < 20; frame++)
            {
                uint expected = (frame & 1) == 0 ? 0x10000u : 0x1c000u;
                var result = speed.CalculateBaseSpeedDecelerationDisallowed(SamusMovementType.Falling);
                AssertEqual(expected, result.Speed, $"retail falling speed medium={medium} frame={frame}");
                AssertEqual(expected, speed.BaseFixed, "falling speed persists the CPU-observed recurrence");
                AssertEqual(SamusAccelerationMode.Decelerating, speed.AccelerationMode, "aerial decelerating mode is kept by the falling recurrence");
            }
        }
    }
}
