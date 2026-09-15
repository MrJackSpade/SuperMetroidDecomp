using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Regression reproducer for the two native grab/Flash ownership orders (#431).</summary>
internal static class DraygonCrystalAdmissionAudit
{
    public static int Run(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        if (Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bus.Rom)) !=
            "12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72")
            throw new InvalidDataException("Draygon/Flash audit requires the pinned Japan/USA ROM.");
        int failures = 0;
        for (int order = 0; order < 2; order++)
        for (int right = 0; right < 2; right++)
        {
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, wideRunway: true);
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var projectile in runtime.Enemies.EnemyProjectiles) projectile.Clear();
            var samus = runtime.Samus ?? throw new InvalidDataException("Missing Samus.");
            samus.Pose = right != 0 ? SamusPoseIds.FacingRightNormalPose : SamusPoseIds.FacingLeftNormalPose;
            samus.XPosition = 256; samus.YPosition = 400;
            samus.Kinematics.YSpeed = samus.Kinematics.YSubspeed = 0;
            samus.Health = 49; samus.MaxHealth = 99; samus.ReserveEnergy = 0;
            samus.Missiles = samus.SuperMissiles = samus.PowerBombs = 10;
            samus.MaxMissiles = samus.MaxSuperMissiles = samus.MaxPowerBombs = 10;
            samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
            try
            {
                if (order == 0) samus.DraygonGrabbed.Begin(bus, samus, right != 0);
                if (!samus.CrystalFlash.TryBegin(bus, samus, 0x470, 0x40))
                    throw new InvalidDataException("Expected native-admitted Crystal Flash.");
                if (order == 1)
                {
                    for (int frame = 0; frame < 12; frame++) runtime.StepFrame(0);
                    samus.DraygonGrabbed.Begin(bus, samus, right != 0);
                }
                runtime.StepFrame(0);
                ushort expectedY = order == 0 ? (ushort)398 : (ushort)380;
                if (samus.YPosition != expectedY || !samus.DraygonGrabbed.IsActive ||
                    samus.DraygonGrabbed.EscapeButtonCounter != (order == 0 ? 10 : 0) ||
                    (order == 1 && samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive))
                    throw new InvalidDataException($"Wrong owner/counter: Y={samus.YPosition}, grab={samus.DraygonGrabbed.IsActive}, counter={samus.DraygonGrabbed.EscapeButtonCounter}, Flash={samus.CrystalFlash.Phase}.");
                Console.WriteLine($"Draygon/Flash order={order}, right={right}: admission matches native.");
            }
            catch (Exception error) when (error is InvalidOperationException or InvalidDataException)
            {
                failures++;
                Console.WriteLine($"Draygon/Flash order={order}, right={right}: {error.GetType().Name}: {error.Message}");
            }
        }
        Console.WriteLine($"Draygon/Flash admission: {failures} failing cases.");
        return failures == 0 ? 0 : 1;
    }
}
