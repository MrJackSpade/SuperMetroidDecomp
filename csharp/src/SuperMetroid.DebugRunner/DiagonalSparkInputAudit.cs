using SuperMetroid.Core.Game;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Charge acquisition through launch, without seeding stored shine or a launch pose.</summary>
internal static class DiagonalSparkInputAudit
{
    public static int CompareNative(string rom, string path)
    {
        if (Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path))) !=
            "782C3EAD92A3FD7D20F526FF7BD55A5FE31A522B2466C05135212727711A34CF")
            throw new InvalidDataException("Use the accepted original-CPU diagonal input trace.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int compared = 0;
        foreach (var group in File.ReadLines(path).Skip(1).Select(line => line.Split(','))
            .GroupBy(row => string.Join(',', row[..2])))
        {
            bool left = group.First()[0] == "1";
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var samus = runtime.Samus!;
            samus.XPosition = (ushort)(left ? 1200 : 128);
            samus.YPosition = 235;
            samus.Health = samus.MaxHealth = 999;
            samus.EquippedItems = (ushort)SamusEquipmentFlags.SpeedBooster;
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            var audio = new CartridgeAudioState();
            foreach (var row in group)
            {
                runtime.StepFrame(Convert.ToUInt16(row[3], 16), queueEchoSound: () =>
                    audio.QueueSoundAndGetAccumulator(SoundEffectLibrary3Sounds.SpeedBoosterEcho, 6));
                string actual = $"{samus.Pose:X4},{samus.HorizontalSpeed.SpeedBoostCounter:X4},{samus.Shinespark.ShineTimer:X4},{samus.Shinespark.StartStopTimer:X4},{samus.XPosition:X4}{samus.Kinematics.XSubposition:X4},{samus.YPosition:X4}{samus.Kinematics.YSubposition:X4}";
                if (actual != string.Join(',', row[4..]))
                    throw new InvalidDataException($"Native diagonal {group.Key}, frame {row[2]}: expected {string.Join(',', row[4..])}; actual {actual}.");
                compared++;
            }
        }
        if (compared != 3578) throw new InvalidDataException("Incomplete diagonal input matrix.");
        Console.WriteLine($"Native charge-to-diagonal: {compared} frame records match.");
        return 0;
    }

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
