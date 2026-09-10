using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyPauseMomentumReconciliation()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int counterAddress = SamusMovementRomData.HorizontalMotion.SpeedBoostCounterLowBytes;
        ushort initialCounter = (ushort)(bus.ReadByte(counterAddress) | bus.ReadByte(counterAddress + 1) << 8);
        foreach (bool equipped in new[] { false, true })
        foreach (bool momentum in new[] { false, true })
        foreach (ushort counter in new ushort[] { 0, 0x0402 })
        {
            var speed = new SamusHorizontalSpeedState
            {
                BaseSpeed = 1, BaseSubspeed = 0x4000,
                ExtraRunSpeed = 2, ExtraRunSubspeed = 0x8000,
                SpeedBoostCounter = 0x0402,
                SpecialPaletteFrame = 6, SpecialPaletteTimer = 3,
            };
            // Populate both echo slots through their production capture path, then
            // put them in departure mode. Unpause must erase rather than restart it.
            speed.CaptureSpeedEchoPosition(0, 80, 200);
            speed.CaptureSpeedEchoPosition(4, 90, 200);
            speed.CancelRunningMomentum(8);
            speed.HasRunningMomentum = momentum;
            speed.SpeedBoostCounter = counter;
            speed.ReconcilePauseSpeedBoosterState(bus, equipped);
            AssertEqual(0x00014000u, speed.BaseFixed, "unpause preserves numeric base speed");
            AssertEqual(2, speed.ExtraRunSpeed, "unpause leaves extra whole speed for the next movement update");
            AssertEqual(0x8000, speed.ExtraRunSubspeed, "unpause leaves extra fractional speed for the next movement update");
            AssertTrue(speed.HasRunningMomentum == (equipped && momentum), "unpause reconciles momentum to Speed Booster equipment");
            ushort expectedCounter = equipped ? momentum && counter == 0
                ? initialCounter : counter : (ushort)0;
            AssertEqual(expectedCounter, speed.SpeedBoostCounter, "unpause preserves, arms or clears boost countdown");
            if (!equipped)
            {
                AssertEqual(0, speed.SpecialPaletteFrame, "unpause clears boost palette frame");
                AssertEqual(0, speed.SpecialPaletteTimer, "unpause clears boost palette timer");
                AssertEqual(0, speed.SpeedEchoIndex, "unpause resets echo index without departure mode");
                AssertEqual(0, speed.FirstSpeedEchoXPosition | speed.SecondSpeedEchoXPosition |
                    speed.FirstSpeedEchoYPosition | speed.SecondSpeedEchoYPosition |
                    speed.FirstSpeedEchoXSpeed | speed.SecondSpeedEchoXSpeed, "unpause clears both echo positions and velocities");
            }
            else if (momentum && counter == 0)
            {
                AssertEqual(0, speed.SpecialPaletteFrame, "newly enabled boost resets palette frame");
                AssertEqual(0, speed.SpecialPaletteTimer, "unpause arms boost with native zero palette timer");
            }
        }
    }
}
