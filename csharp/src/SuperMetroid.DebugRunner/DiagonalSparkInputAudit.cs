using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Charge acquisition through launch, without seeding stored shine or a launch pose.</summary>
internal static class DiagonalSparkInputAudit
{
    public static int Run(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        foreach (bool left in new[] { false, true })
        foreach (bool upPriorityControl in new[] { false, true })
        {
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var samus = runtime.Samus!;
            samus.XPosition = (ushort)(left ? 1200 : 128);
            samus.YPosition = 235;
            samus.Health = samus.MaxHealth = 999;
            samus.EquippedItems = (ushort)SamusEquipmentFlags.SpeedBooster;
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            int frame = 0;
            void Step(SnesButton input) { runtime.StepFrame((ushort)input); frame++; }
            SnesButton forward = left ? SnesButton.Left : SnesButton.Right;
            while (samus.HorizontalSpeed.SpeedBoostCounter < SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter)
            {
                Step(forward | SnesButton.B);
                if (frame > 200) throw new InvalidDataException("Ordinary run input did not acquire Speed Booster charge.");
            }
            int charge = frame;
            Step(SnesButton.Down);
            if (samus.Shinespark.Phase != ShinesparkPhase.Stored)
                throw new InvalidDataException("Ordinary crouch input did not store charge.");
            for (int settle = 0; settle < 15; settle++) Step(SnesButton.None);
            int jump = frame;
            ShinesparkPhase expected = upPriorityControl ? ShinesparkPhase.Vertical : ShinesparkPhase.Diagonal;
            do
            {
                Step(SnesButton.A | SnesButton.R |
                    (upPriorityControl && samus.Shinespark.Phase == ShinesparkPhase.Windup
                        ? SnesButton.Up | forward : SnesButton.None));
                if (frame - jump > 40) throw new InvalidDataException($"Launch input did not enter {expected}.");
            } while (samus.Shinespark.Phase != expected);
            int launch = frame;
            int x = samus.XPosition, y = samus.YPosition;
            Step(SnesButton.None);
            if (!(samus.YPosition < y && (upPriorityControl ? samus.XPosition == x :
                left ? samus.XPosition < x : samus.XPosition > x)))
                throw new InvalidDataException("Launch did not move along its selected axes.");
            while (samus.Shinespark.Phase != ShinesparkPhase.Inactive)
            {
                Step(SnesButton.None);
                if (frame - launch > 200) throw new InvalidDataException("Diagonal spark did not terminate.");
            }
            Console.WriteLine($"{(left ? "Left" : "Right")} {expected}: charge {charge}, jump {jump}, launch {launch}, completion {frame}.");
        }
        return 0;
    }
}
