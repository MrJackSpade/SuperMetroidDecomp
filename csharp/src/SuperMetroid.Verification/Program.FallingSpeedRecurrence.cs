using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyRetailFallingSpeedRecurrence()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (ushort medium in new[] { SamusLiquidPhysicsState.Air, SamusLiquidPhysicsState.Water })
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
