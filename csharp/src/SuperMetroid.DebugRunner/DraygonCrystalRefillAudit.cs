using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Placement, pickup collision and cleanup admission with ten-capacity ammo.</summary>
internal static class DraygonCrystalRefillAudit
{
    public static int Run(string rom, string trace)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        string text = File.ReadAllText(trace).Replace("\r\n", "\n", StringComparison.Ordinal);
        if (Convert.ToHexString(SHA256.HashData(bus.Rom)) != "12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72" ||
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))) != "4A6BCFCBB8FE018A61139A83A2E93BD78ACA02D3207ADA66B510FB8C54D3834C")
            throw new InvalidDataException("Use the pinned ROM and native refill trace.");
        var expected = text.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();
        int index = 0;
        for (int right = 0; right < 2; right++)
        for (int mode = 0; mode < 8; mode++)
        {
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            var samus = runtime.Samus!;
            var bombs = runtime.BombProjectiles;
            // Prepared ball pose and existing pickup. After setup, inventory changes
            // only through production placement and collection, never fixture writes.
            samus.Pose = right != 0 ? (byte)0x1d : (byte)0x41;
            samus.XPosition = 256; samus.YPosition = 400;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.Kinematics.YSpeed = samus.Kinematics.YSubspeed = 0;
            samus.Health = 49; samus.MaxHealth = 99; samus.ReserveEnergy = 0;
            samus.PowerBombs = samus.MaxPowerBombs = (ushort)(mode == 4 ? 9 : mode == 5 ? 11 : 10);
            samus.Missiles = samus.MaxMissiles = samus.SuperMissiles = samus.MaxSuperMissiles = 10;
            samus.EquippedItems = samus.CollectedItems = 4;
            samus.EquippedBeams = samus.CollectedBeams = 0;
            samus.SelectedHudItem = 3; samus.AutoCancelHudItemIndex = 0;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            void Check(int stage)
            {
                string actual = $"{right},{mode},{stage},{samus.PowerBombs:X4},{samus.Missiles:X4},{samus.SuperMissiles:X4},{samus.Pose:X2},{(samus.CrystalFlash.Phase == CrystalFlashPhase.Raising ? 1 : 0)}";
                if (index >= expected.Length || actual != expected[index++])
                    throw new InvalidDataException($"Refill stage mismatch: {actual}; native {expected[index - 1]}");
            }
            void Pickup(bool distant)
            {
                var pickup = runtime.Enemies.EnemyProjectiles[0];
                pickup.Variable0 = 6; pickup.Variable1 = 400;
                pickup.XRadius = pickup.YRadius = 5;
                pickup.XPosition = (ushort)(samus.XPosition + (distant ? 100 : 0));
                pickup.YPosition = samus.YPosition;
                typeof(RoomEnemySystem).GetMethod("RunEnemyPickupPreInstruction", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(runtime.Enemies, new object[] { pickup, samus });
            }
            Check(0);
            if (mode == 1) Pickup(false);
            Check(1);
            bombs.StepFrame(bus, runtime.LevelData!, samus, 0x40, 0x40, deferSamusOverlap: true);
            if (!bombs.PowerBombExplosion.IsArmed) throw new InvalidDataException("Power Bomb was not placed.");
            Check(2);
            if (mode is 2 or 3 or 4 or 6 or 7) Pickup(mode == 3);
            Check(3);
            samus.DraygonGrabbed.Begin(bus, samus, right != 0);
            if (mode == 7) samus.XPosition++;
            int frames = 0;
            while (bombs.PowerBombExplosion.IsArmed && samus.CrystalFlash.Phase == CrystalFlashPhase.Inactive)
            {
                if (++frames > 1000) throw new InvalidDataException("Power Bomb never reached cleanup.");
                bombs.StepFrame(bus, runtime.LevelData!, samus, (ushort)(mode == 6 ? 0x4f0 : 0x470), 0, deferSamusOverlap: true);
            }
            Check(4);
        }
        if (index != 80 || expected.Length != index) throw new InvalidDataException("Incomplete refill matrix.");
        Console.WriteLine($"Draygon/Flash refill: 16 cases, {index} native seam comparisons, all match.");
        return 0;
    }
}
