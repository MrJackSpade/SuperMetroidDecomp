using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

/// <summary>Full runtime movement after both Flash/grab orders, through a retained spark.</summary>
internal static class DraygonCrystalRuntimeAudit
{
    public static int Run(string rom, string trace, bool xrayCancellation = false, bool recharge = false, bool lifetime = false, bool repeat = false, bool sand = false)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        string text = File.ReadAllText(trace).Replace("\r\n", "\n", StringComparison.Ordinal);
        if (Convert.ToHexString(SHA256.HashData(bus.Rom)) != "12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72" ||
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))) != (sand
                ? "8BEC4E7921B8B1B76DFFBC93CA970F8ECEFB7D3596DA69CE21D72C4C37B3C43C" : repeat
                ? "1A8779EB4D6E56D2C7EF17E5ACE3DFA12161FB55BBD8EBBADBEA920BE233275D" : lifetime
                ? "9208A09FB48468802B707F291507E3C04C9BBB2C0F154FB597F8B609DF1AEAD3" : recharge
                ? "65BB9B69CBCDC1A28E9F0243D5BAE9D43053D6F95AE676F27040F7C535A5D368" : xrayCancellation
                ? "84F6CA152C32A090EA394E31B77568839A2F1C9CFD6F1869471BDAB4FA523AD2"
                : "E2F54300208FAA5BEF8F4D978FC2064900D2B04EE913432020D15785239487F7"))
            throw new InvalidDataException("Use the pinned ROM and native Draygon/Flash runtime trace.");
        var rows = text.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != (sand ? 11232 : repeat ? 3200 : lifetime ? 4000 : recharge ? 5600 : xrayCancellation ? 1404 : 6880) || rows.Any(row => row.Length != (sand ? 30 : lifetime ? 29 : 27)))
            throw new InvalidDataException("Incomplete runtime trace.");
        int mismatches = 0;
        foreach (var group in rows.GroupBy(row => $"{row[0]},{row[1]},{row[2]}"))
        {
            var first = group.First();
            int order = int.Parse(first[0]), right = int.Parse(first[1]), mode = int.Parse(first[2]);
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var level = runtime.LevelData ?? throw new InvalidDataException("Missing room.");
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                int block = y * level.WidthInBlocks + x;
                level.SetForegroundEntry(block, y is 16 or 32 ? (ushort)0x8000 : (ushort)0);
                level.SetBehavior(block, 0);
            }
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var projectile in runtime.Enemies.EnemyProjectiles) projectile.Clear();
            var samus = runtime.Samus ?? throw new InvalidDataException("Missing Samus.");
            samus.Pose = right != 0 ? SamusPoseIds.FacingRightNormalPose : SamusPoseIds.FacingLeftNormalPose;
            samus.XPosition = (ushort)(recharge ? 1152 : 256); samus.YPosition = 400;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Kinematics.YSpeed = samus.Kinematics.YSubspeed = 0;
            samus.EquippedItems = samus.CollectedItems = samus.EquippedBeams = samus.CollectedBeams = 0;
            if (recharge) samus.EquippedItems = samus.CollectedItems = 0x2000;
            samus.Health = 49; samus.MaxHealth = 99; samus.ReserveEnergy = 0;
            samus.Missiles = samus.SuperMissiles = samus.PowerBombs = 10;
            samus.MaxMissiles = samus.MaxSuperMissiles = samus.MaxPowerBombs = 10;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(right != 0 ? 8 : 4);
            if (order == 0) samus.DraygonGrabbed.Begin(bus, samus, right != 0);
            if (!samus.CrystalFlash.TryBegin(bus, samus, 0x470, 0x40)) throw new InvalidDataException("Flash admission failed.");
            runtime.Controller1.Latch(0x470);
            var audio = new CartridgeAudioState();
            int frame = 0;
            bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[3]) != frame) throw new InvalidDataException("Reordered native frames.");
                if (order == 1 && frame == 12) samus.DraygonGrabbed.Begin(bus, samus, right != 0);
                ushort input = ushort.Parse(row[4], NumberStyles.HexNumber);
                var publication = new GameplayAudioFramePublication(audio);
                runtime.StepFrame(input, queueEchoSound: recharge ? () => publication.QueueEcho(runtime) : null);
                if (recharge) publication.PublishPrefix(runtime);
                if (sand && frame == 350) TemporaryBlueSandProbe.Apply(bus, level, samus, mode);
                if (repeat && frame == 350 && samus.CrystalFlash.TryBegin(bus, samus, 0x470, 0x40) != (order == 1))
                    throw new InvalidDataException("Repeat Flash admission differs from native.");
                if (repeat && frame == 799 && (order == 1
                    ? samus.SharedShineTimer != 0 || samus.Missiles != 0 || samus.SuperMissiles != 0 || samus.PowerBombs != 0
                    : samus.SharedShineTimer == 0 || samus.CrystalFlash.SpecialPaletteKind != SamusSpecialPaletteType.CrystalFlash))
                    throw new InvalidDataException("Repeat Flash completion/failed admission retention differs from native.");
                if (recharge && frame == 699 && samus.SharedShineTimer != (mode == 2 ? 0 : 1))
                    throw new InvalidDataException("Recharge expiration/retained Flash control differs from native.");
                if (xrayCancellation && frame == 350 && !samus.Xray.TryBegin(bus, samus, samus.ReadMovementType(bus)))
                    throw new InvalidDataException("X-Ray rejected the native-admitted retained Flash setup.");
                if (xrayCancellation && frame == 350 && (!samus.Xray.TimeIsFrozen ||
                    samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive ||
                    samus.CrystalFlash.SpecialPaletteKind != SamusSpecialPaletteType.None))
                    throw new InvalidDataException("X-Ray did not replace the interrupted Flash owners.");
                if (!recharge && !lifetime && !repeat && mode == 2 && frame == 363 && (samus.Shinespark.Phase != ShinesparkPhase.Vertical ||
                    samus.HorizontalSpeed.ContactDamageIndex != 2 || samus.Health != (order == 0 ? 98 : 48)))
                    throw new InvalidDataException("Retained Flash timer did not launch a damaging, energy-consuming spark.");
                bool flashPalette = samus.CrystalFlash.SpecialPaletteKind == SamusSpecialPaletteType.CrystalFlash;
                string actual = $"{samus.Pose:X2},{samus.YPosition:X4},{samus.Health:X4},{samus.Missiles:X4},{samus.SuperMissiles:X4},{samus.PowerBombs:X4}," +
                    $"{samus.SharedShineTimer:X4},{(samus.Xray.IsActive ? samus.Xray.SpecialPaletteType : flashPalette ? samus.CrystalFlash.SpecialPaletteType : samus.Shinespark.PaletteType):X4}," +
                    $"{samus.XPosition:X4},{samus.Kinematics.XSubposition:X4},{samus.Kinematics.YSubposition:X4},{samus.Kinematics.YSpeed:X4},{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4},{samus.AnimationFrame:X4},{samus.AnimationFrameTimer:X4}";
                string expected = string.Join(',', row[5..7].Concat(row[12..26]));
                if (sand)
                {
                    actual += $",{samus.HorizontalSpeed.SpeedBoostCounter:X4},{samus.Kinematics.ExtraYDisplacement:X4},{samus.Kinematics.ExtraYSubdisplacement:X4}";
                    expected += "," + string.Join(',', row[27..30]);
                }
                if (lifetime && frame >= 300)
                {
                    ushort timer = samus.SharedShineTimer, inv = samus.InvincibilityTimer;
                    if (timer is < 1 or > 5 || samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive)
                        throw new InvalidDataException("Retained palette timer stopped cycling independently of movement.");
                    var oam = new OamBuffer();
                    samus.InvincibilityTimer = 100;
                    oam.BeginFrame(); samus.Draw(bus, oam, 128, 400, (ushort)(frame + 2));
                    int body = oam.NextByteOffset;
                    // Counterfactual draw only; restore the earned timer before advancing.
                    var timerProperty = typeof(SamusCrystalFlashState).GetProperty(nameof(SamusCrystalFlashState.SpecialPaletteTimer))!;
                    timerProperty.SetValue(samus.CrystalFlash, (ushort)0);
                    oam.BeginFrame(); samus.Draw(bus, oam, 128, 400, (ushort)(frame + 2));
                    int control = oam.NextByteOffset;
                    timerProperty.SetValue(samus.CrystalFlash, timer);
                    samus.InvincibilityTimer = inv;
                    if (body != int.Parse(row[27]) || control != int.Parse(row[28]))
                        throw new InvalidDataException($"Flash body draw at {group.Key}/{frame}: {body},{control} != {row[27]},{row[28]}");
                }
                if (actual != expected)
                {
                    mismatches++;
                    if (!reported || recharge && frame == 490) Console.WriteLine($"Draygon/Flash runtime {group.Key} frame {frame}: {actual} != {expected}");
                    reported = true;
                }
                frame++;
            }
        }
        Console.WriteLine($"Draygon/Flash runtime: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
