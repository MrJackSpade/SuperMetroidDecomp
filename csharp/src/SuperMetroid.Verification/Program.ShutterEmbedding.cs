using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyShutterEmbedding()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int activatedCases = 0;
        int carriedCases = 0;
        int minimumGap = 0;
        foreach (int slot in new[] { 0, 1 })
        foreach (int bombAt in new[] { 0, 4, 8, 12, 16, 20, 30, 40, 60 })
        foreach (int stopAt in new[] { 8, 12, 16, 20, 40, 60, 90 })
        {
            int offset = slot == 0 ? 32 : -32;
            SnesButton direction = slot == 0 ? SnesButton.Left : SnesButton.Right;
            var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointersTooling.BrinstarShutterRoom, new Position16<RoomPixels>(0, 0));
            var samus = runtime.Samus!;
            var platform = runtime.Enemies.Slots[slot];
            samus.Pose = SamusPoseId.MorphBallGroundRightPose;
            samus.InputLocked = false;
            samus.EquippedItems |= (ushort)SamusEquipmentFlags.Bombs;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.Position = samus.Position.WithWhole(new((ushort)(platform.XPosition + offset), (ushort)(platform.YPosition - platform.YRadius - samus.Kinematics.Radius.Y)));
            runtime.StepFrame(0);
            bool activated = false;
            bool carried = false;
            for (int frame = 0; frame < 220; frame++)
            {
                ushort input = frame < stopAt ? (ushort)direction : (ushort)0;
                if (frame == bombAt || frame == bombAt + 20) input |= runtime.ControllerBindings.Shoot;
                runtime.StepFrame(input);
                activated |= runtime.Enemies.VerticalShutterStates[slot]!.Function == VerticalShutterFunction.MovingUp;
                carried |= samus.Kinematics.ExtraDisplacement.Y.RawValue < 0;
                int gap = platform.YPosition - platform.YRadius - samus.Position.Y.Whole - samus.Kinematics.Radius.Y;
                if (Math.Abs(samus.Position.X.Whole-platform.XPosition) < platform.XRadius+samus.Kinematics.Radius.X &&
                    samus.Position.Y.Whole < platform.YPosition && gap < minimumGap)
                {
                    minimumGap = gap;
                    Console.WriteLine($"Overlap candidate: slot={slot} offset={offset} bomb={bombAt} stop={stopAt}/{direction} frame={frame} gap={gap} Samus={samus.Position.X.Whole},{samus.Position.Y.Whole}/{(int)samus.Pose:X2} platformY={platform.YPosition}");
                }
            }
            if (activated) activatedCases++;
            if (carried) carriedCases++;
        }
        AssertTrue(activatedCases > 0 && carriedCases > 0, "approach sweep reaches bomb activation and upward rider carry");
        // This is an investigation tool, not a passing regression for #347. Small
        // negative gaps must be compared with native subpixel/carry rounding before
        // deciding whether they are faulty; none proves the reported trapping alone.
        Console.WriteLine($"126 shutter approach sequences: {activatedCases} activate the shutter, {carriedCases} reach upward carry; minimum support gap={minimumGap}.");
    }
}
