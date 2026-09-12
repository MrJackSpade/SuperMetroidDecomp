using SuperMetroid.Core.Hardware;

/// <summary>Full-runtime reproduction of the draw-time Shoot edge retained by missile admission.</summary>
internal static class MissileInputCarryAudit
{
    public static int Run(string rom)
    {
        foreach (int secondPress in new[] { 8, 9, 10, 11, -1 })
        {
            var runtime = FlatFloorMovementFixture.Create(SuperMetroidAddressSpace.LoadRetailRom(rom), false);
            var samus = runtime.Samus!;
            samus.Missiles = samus.MaxMissiles = 10;
            runtime.StepFrame(runtime.ControllerBindings.ItemSelect);
            runtime.StepFrame(0);
            var fired = new List<int>();
            for (int frame = 0; frame < 24; frame++)
            {
                bool shoot = frame == 0 || frame == secondPress || secondPress < 0;
                runtime.StepFrame(shoot ? runtime.ControllerBindings.Shoot : (ushort)0);
                if (runtime.Projectiles.LastFiredProjectileSnapshot is not null) fired.Add(frame);
            }
            int[] expected = secondPress is 8 or -1 ? [0] : [0, Math.Max(10, secondPress)];
            if (!fired.SequenceEqual(expected) || samus.Missiles != 10 - expected.Length)
                throw new InvalidDataException($"Missile input carry press={secondPress}: fires={string.Join(',', fired)}, ammo={samus.Missiles}; expected {string.Join(',', expected)}.");
            Console.WriteLine($"Missile input carry press={secondPress}: {string.Join(',', fired)}.");
        }
        return 0;
    }
}
