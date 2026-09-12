using SuperMetroid.Core.Game;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Charge acquisition through launch, without seeding stored shine or a launch pose.</summary>
internal static class DiagonalSparkInputAudit
{
    public static int CompareNative(string rom, string path, bool complete = false, int direction = -1)
    {
        if (direction is < -1 or > 1 || (complete && direction != -1))
            throw new ArgumentOutOfRangeException(nameof(direction));
        string expectedHash = direction == 0
            ? "891F74968F7697787B286CD07D0145E4AC03CDAC26EFAA449351945D865CBDA6"
            : direction == 1 ? "108E4C1DB58315D1EA70D969B81CAC8A552CA24BB845582371087315B8CF3A9C"
            : complete
            ? "CDA780C3FB58C08A0DF557AB3338608F19E0B5D8AD1A40B5C674409C6B370056"
            : "782C3EAD92A3FD7D20F526FF7BD55A5FE31A522B2466C05135212727711A34CF";
        if (Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path))) != expectedHash)
            throw new InvalidDataException("Use the accepted original-CPU diagonal input trace.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int compared = 0;
        foreach (var group in File.ReadLines(path).Skip(1).Select(line => line.Split(','))
            .GroupBy(row => string.Join(',', row[..2])))
        {
            bool left = group.First()[0] == "1";
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            if (complete)
                for (int x = 0; x < runtime.LevelData!.WidthInBlocks; x++)
                    runtime.LevelData.SetForegroundEntry(x, 0x8000);
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
            if (direction >= 0 && int.Parse(group.First()[1]) >= 3)
            {
                ShinesparkPhase expected = direction == 0 ? ShinesparkPhase.Vertical : ShinesparkPhase.Horizontal;
                if (samus.Shinespark.Phase != expected)
                    throw new InvalidDataException($"Direction control did not launch {expected}.");
            }
        }
        if (compared != (complete ? 6448 : 3578)) throw new InvalidDataException("Incomplete diagonal input matrix.");
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
