using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Runtime suit release with suspended Flash and timer-eight bomb overlap controls.</summary>
internal static class FlashSuitFlowAudit
{
    public static int Run(string rom, string trace)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        string text = File.ReadAllText(trace).Replace("\r\n", "\n", StringComparison.Ordinal);
        if (Convert.ToHexString(SHA256.HashData(bus.Rom)) != "12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72" ||
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))) != "BB58506A1B95B58BF991D87C8653AC77F3C8F2FA34E81E7DD74151FBF4F3E217")
            throw new InvalidDataException("Use the pinned ROM and native suit-flow trace.");
        var rows = text.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(line => line.Split(',')).ToArray();
        if (rows.Length != 5280 || rows.Any(row => row.Length != 20)) throw new InvalidDataException("Incomplete suit flow.");
        int mismatches = 0;
        foreach (var group in rows.GroupBy(row => $"{row[0]},{row[1]},{row[2]}"))
        {
            var first = group.First();
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
            samus.Pose = first[0] == "1" ? SamusPoseIds.FacingRightNormalPose : SamusPoseIds.FacingLeftNormalPose;
            samus.XPosition = 256; samus.YPosition = 491;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Kinematics.YSpeed = samus.Kinematics.YSubspeed = 0;
            samus.EquippedItems = samus.CollectedItems = samus.EquippedBeams = samus.CollectedBeams = 0;
            samus.Health = 49; samus.MaxHealth = 99; samus.ReserveEnergy = 0;
            samus.Missiles = samus.SuperMissiles = samus.PowerBombs = 10;
            samus.MaxMissiles = samus.MaxSuperMissiles = samus.MaxPowerBombs = 10;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            samus.PoseHistory.PreviousPose = samus.Pose;
            samus.PoseHistory.PreviousDirectionAndMovement = (ushort)(first[0] == "1" ? 8 : 4);
            if (!samus.CrystalFlash.TryBegin(bus, samus, 0x470, 0x40)) throw new InvalidDataException("Flash rejected.");
            bool gravity = first[1] == "1";
            samus.EquippedItems = samus.CollectedItems = (ushort)(gravity ? SamusEquipmentFlags.GravitySuit : SamusEquipmentFlags.VariaSuit);
            runtime.SuitPickup.Begin(bus, samus, 0, 355, gravity ? SamusSuitPickupKind.Gravity : SamusSuitPickupKind.Varia);
            int frame = 0;
            bool reported = false;
            foreach (var row in group)
            {
                if (int.Parse(row[3]) != frame++) throw new InvalidDataException("Reordered suit flow.");
                var bomb = runtime.BombProjectiles.Slots[0];
                bomb.ClearFields();
                if (runtime.SuitPickup.IsActive && runtime.SuitPickup.Substate == 6 && first[2] != "0")
                {
                    bomb.Type = (ushort)SamusProjectileFamily.Bomb; bomb.Damage = 30; bomb.BombTimer = 8;
                    bomb.XPosition = (ushort)(samus.XPosition + (first[2] == "2" ? 13 : 0));
                    bomb.YPosition = samus.YPosition; bomb.XRadius = bomb.YRadius = 8;
                }
                int currentFrame = frame - 1;
                runtime.StepFrame((ushort)(currentFrame < 400 ? 0 : currentFrame == 400 ? 0x80 : 0x880));
                if (currentFrame == 399 && (samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive ||
                    samus.BombJumpActive || samus.BombJumpStarting ||
                    (first[2] == "1" ? samus.SharedShineTimer == 0 || samus.CrystalFlash.SpecialPaletteKind != SamusSpecialPaletteType.CrystalFlash
                                     : samus.SharedShineTimer != 0 || samus.CrystalFlash.SpecialPaletteKind != SamusSpecialPaletteType.None)))
                    throw new InvalidDataException("Bomb hit/control did not restore the native movement and retention state.");
                if (currentFrame == 403 && first[2] == "1" && (samus.Shinespark.Phase != ShinesparkPhase.Vertical ||
                    samus.HorizontalSpeed.ContactDamageIndex != 2 || samus.Health != 48))
                    throw new InvalidDataException("Suit/Flash bomb retention cannot launch the native damaging spark.");
                bool flashPalette = samus.CrystalFlash.SpecialPaletteKind == SamusSpecialPaletteType.CrystalFlash;
                string actual = $"{samus.Pose:X2},{samus.XPosition:X4},{samus.YPosition:X4},{samus.Kinematics.YSubposition:X4}," +
                    $"{samus.Kinematics.YSpeed:X4},{samus.Kinematics.YSubspeed:X4},{samus.Kinematics.YDirection:X4}," +
                    $"{samus.SharedShineTimer:X4},{(flashPalette ? samus.CrystalFlash.SpecialPaletteType : samus.Shinespark.PaletteType):X4},{(samus.Shinespark.Phase != ShinesparkPhase.Inactive ? samus.Shinespark.VerticalAccelerationSpeed : samus.CrystalFlash.AmmoDecrementTimer):X4}," +
                    $"{samus.Health:X4},{samus.Missiles:X4},{samus.SuperMissiles:X4},{samus.PowerBombs:X4},{(runtime.SuitPickup.IsActive ? 1 : 0)}";
                string expected = string.Join(',', row[4..11].Concat(row[12..20]));
                if (actual == expected) continue;
                mismatches++;
                if (!reported) Console.WriteLine($"Suit flow {group.Key} frame {frame - 1}: {actual} != {expected}");
                reported = true;
            }
        }
        Console.WriteLine($"Suit/Flash flow: {rows.Length} frames, {mismatches} mismatches.");
        return mismatches == 0 ? 0 : 1;
    }
}
