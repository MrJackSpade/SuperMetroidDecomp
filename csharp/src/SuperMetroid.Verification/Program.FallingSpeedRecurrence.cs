using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

/// <summary>Represents falling speed recurrence verification.</summary>
internal static class FallingSpeedRecurrenceVerification
{
    /// <summary>Verifies retail falling speed recurrence.</summary>
    internal static void VerifyRetailFallingSpeedRecurrence()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var level = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(new ushort[256], new byte[256], new ushort[256]), new byte[8]);
        foreach (ushort extraWhole in new ushort[] { 0, 1 })
        foreach (ushort extraFraction in new ushort[] { 0, 1 })
        foreach (ushort previousMode in new ushort[] { 0, 1, 2 })
        {
            var samus = new SamusState { Pose = SamusPoseId.FacingLeftNormalPose };
            samus.PoseTransitions.ApplyStandingLeftToRunningLeft(bus);
            samus.HorizontalSpeed.BaseSpeed = 2;
            samus.HorizontalSpeed.BaseSubspeed = 0xc000;
            samus.HorizontalSpeed.AccelerationMode = previousMode;
            samus.HorizontalSpeed.ExtraRunSpeed = extraWhole;
            samus.HorizontalSpeed.ExtraRunSubspeed = extraFraction;
            samus.PoseTransitions.ApplyWalkedOffFloorTransition(bus, level, SamusPoseId.FallingLeftPose);
            // $91:F60D replaces the prior grounded mode on falling-pose entry.
            // Fractional extra speed matters even when its whole word is zero.
            AssertEqual(extraWhole != 0 || extraFraction != 0 ? 2 : 0,
                samus.HorizontalSpeed.AccelerationMode, "fall entry selects acceleration mode from extra speed");
            AssertEqual(0x2c000u, samus.HorizontalSpeed.BaseFixed, "fall entry does not clamp base speed");
            AssertEqual(extraWhole, samus.HorizontalSpeed.ExtraRunSpeed, "fall entry preserves whole dash component");
            AssertEqual(extraFraction, samus.HorizontalSpeed.ExtraRunSubspeed, "fall entry preserves fractional dash component");
        }
        foreach (SamusLiquidMedium medium in new[] { SamusLiquidMedium.Air, SamusLiquidMedium.Water })
        {
            var speed = new SamusHorizontalSpeedState
            {
                AccelerationMode = 2,
                BaseSpeed = 2,
                BaseSubspeed = 0x9800,
            };
            speed.SelectEnvironmentSpeedTable(medium);
            for (int frame = 0; frame < 20; frame++)
            {
                // Captured by executing original $90:9B1F instructions in the CPU probe,
                // not generated from the C# comparison helper. The fractional signed
                // comparison allows 1.C000 on alternate calls before clamping to 1.0000.
                uint expected = (frame & 1) == 0 ? 0x10000u : 0x1c000u;
                var result = speed.CalculateBaseSpeedDecelerationDisallowed(bus, SamusMovementType.Falling);
                AssertEqual(expected, result.Speed, $"retail falling speed medium={medium} frame={frame}");
                AssertEqual(expected, speed.BaseFixed, "falling speed persists the CPU-observed recurrence");
                AssertEqual(2, speed.AccelerationMode, "aerial mode two does not become a deceleration mode");
            }
        }
    }
}
