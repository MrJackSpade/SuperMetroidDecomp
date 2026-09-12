using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    // #411: a controlled camera is an independent input to the real projectile
    // owner. This isolates lifetime/collision, not Samus camera-tracking parity.
    private static void VerifyHeroShotCameraLifetime()
    {
        foreach (bool followShot in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var blocks = new ushort[64 * 16];
            for (int row = 0; row < 16; row++) blocks[row * 64 + 32] = 0x8000;
            var room = CreateRoom(64, 16, blocks, new byte[blocks.Length]);
            var samus = new SamusState { Pose = 1, XPosition = 128, YPosition = 128 };
            var projectiles = new SamusProjectileSystem();
            var shared = new SamusBombProjectileSystem();
            int deletedFrame = -1, impactFrame = -1;
            for (int frame = 0; frame < 120; frame++)
            {
                ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
                var shot = projectiles.Slots[0];
                ushort cameraX = followShot && shot.IsActive
                    ? (ushort)Math.Max(0, shot.XPosition - 128) : (ushort)0;
                shared.StepFrame(bus, room, samus, input, input);
                var result = projectiles.StepFrame(bus, room, samus, input, input, cameraX, 0, shared);
                if (frame == 0) AssertEqual((int?)0, result.FiredSlot, "Hero fixture fires through normal input dispatch");
                if (shot.PackedType.Family == SamusProjectileFamily.BeamExplosion)
                {
                    impactFrame = frame;
                    AssertTrue(shot.XPosition >= 496 && shot.XPosition <= 520,
                        "Surviving shot explodes at the remote solid column in world space");
                    break;
                }
                if (!shot.IsActive) { deletedFrame = frame; break; }
                if (!followShot) AssertTrue(shot.XPosition < 320,
                    "Stationary camera retains the beam only inside native right deletion bound");
            }
            if (followShot) AssertTrue(impactFrame >= 0 && deletedFrame < 0,
                "Camera following preserves the shot until distant terrain collision");
            else AssertTrue(deletedFrame >= 0 && impactFrame < 0,
                "Identical firing without camera movement expires before the target");
            Console.WriteLine($"Hero shot camera follow={followShot}: deletion frame={deletedFrame}, impact frame={impactFrame}.");
        }
    }
}
