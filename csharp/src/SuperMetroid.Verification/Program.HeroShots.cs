using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    // #411: a controlled camera is an independent input to the real projectile
    // owner. This isolates lifetime/collision, not Samus camera-tracking parity.
    private static void VerifyHeroShotCameraLifetime(string? nativeTrace = null)
    {
        var native = nativeTrace is null ? null : File.ReadLines(nativeTrace).Skip(1)
            .Select(line => line.Split(','))
            .ToDictionary(row => (int.Parse(row[0]), int.Parse(row[1]), int.Parse(row[2])), row => row.Skip(3)
                .Select(value => Convert.ToUInt16(value, 16)).ToArray());
        int comparedFrames = 0;
        foreach (bool vertical in new[] { false, true })
        for (int cameraMode = 0; cameraMode < 4; cameraMode++)
        {
            bool followShot = cameraMode != 0;
            var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var blocks = new ushort[64 * 16];
            for (int row = 0; row < 16; row++) blocks[vertical ? 16 * 16 + row : row * 64 + 32] = 0x8000;
            var room = CreateRoom(vertical ? 16 : 64, vertical ? 64 : 16, blocks, new byte[blocks.Length]);
            var samus = new SamusState { Pose = (byte)(vertical ? 3 : 1), XPosition = 128,
                YPosition = (ushort)(vertical ? 640 : 128) };
            var projectiles = new SamusProjectileSystem();
            var shared = new SamusBombProjectileSystem();
            int deletedFrame = -1, impactFrame = -1;
            for (int frame = 0; frame < 120; frame++)
            {
                ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
                var shot = projectiles.Slots[0];
                ushort cameraX = !vertical && followShot && shot.IsActive
                    ? (ushort)Math.Max(0, shot.XPosition - 128) : (ushort)0;
                ushort cameraY = vertical ? (ushort)(followShot && shot.IsActive ? shot.YPosition - 128 : 512) : (ushort)0;
                if (cameraMode >= 2 && shot.IsActive)
                {
                    // Camera-only fixture control: put the *next* position at
                    // the native edge or one pixel outside. Production code
                    // independently performs movement, collision and deletion.
                    if (vertical)
                    {
                        uint next = unchecked(((uint)shot.YPosition << 16 | shot.YSubposition)
                            + (uint)((short)(shot.YVelocity - 16) * 256));
                        cameraY = unchecked((ushort)((next >> 16) + (cameraMode == 2 ? 64u : 65u)));
                    }
                    else
                    {
                        uint next = unchecked(((uint)shot.XPosition << 16 | shot.XSubposition)
                            + (uint)((short)(shot.XVelocity + 16) * 256));
                        cameraX = unchecked((ushort)((next >> 16) - (cameraMode == 2 ? 319u : 320u)));
                    }
                }
                shared.StepFrame(bus, room, samus, input, input);
                var result = projectiles.StepFrame(bus, room, samus, input, input, cameraX, cameraY, shared);
                if (native is not null)
                {
                    ushort[] actual = [cameraX, cameraY, shot.XPosition, shot.XSubposition,
                        shot.YPosition, shot.YSubposition, unchecked((ushort)shot.XVelocity),
                        unchecked((ushort)shot.YVelocity), shot.Type, shot.InstructionPointer];
                    AssertTrue(actual.SequenceEqual(native[(vertical ? 1 : 0, cameraMode, frame)]),
                        $"Native Hero trace vertical={vertical}, camera={cameraMode}, frame={frame}: actual {string.Join(',', actual.Select(x => x.ToString("X4")))}");
                    comparedFrames++;
                }
                if (frame == 0) AssertEqual((int?)0, result.FiredSlot, "Hero fixture fires through normal input dispatch");
                if (shot.PackedType.Family == SamusProjectileFamily.BeamExplosion)
                {
                    impactFrame = frame;
                    AssertTrue(vertical ? shot.YPosition >= 248 && shot.YPosition <= 288 : shot.XPosition >= 496 && shot.XPosition <= 520,
                        "Surviving shot explodes at the remote solid column in world space");
                    break;
                }
                if (!shot.IsActive) { deletedFrame = frame; break; }
                if (cameraMode == 2 && frame > 0) AssertEqual((short)(vertical ? -64 : 319),
                    unchecked((short)(vertical ? shot.YPosition - cameraY : shot.XPosition - cameraX)),
                    "Shot stays active at the exact native camera-relative edge");
                if (!followShot) AssertTrue(vertical ? shot.YPosition >= 448 : shot.XPosition < 320,
                    "Stationary camera retains the beam only inside native right deletion bound");
            }
            if (cameraMode == 3) AssertEqual(1, deletedFrame,
                "Moving the camera one pixel outside retention deletes on the first boundary frame");
            else if (cameraMode == 2) AssertEqual(vertical ? 59 : 61, deletedFrame,
                "Collision relocates the edge-retained beam beyond the window and native immediately deletes its explosion");
            else if (followShot) AssertTrue(impactFrame >= 0 && deletedFrame < 0,
                "Camera following preserves the shot until distant terrain collision");
            else AssertTrue(deletedFrame >= 0 && impactFrame < 0,
                "Identical firing without camera movement expires before the target");
            Console.WriteLine($"Hero shot vertical={vertical}, camera mode={cameraMode}: deletion frame={deletedFrame}, impact frame={impactFrame}.");
        }
        if (native is not null) AssertEqual(native.Count, comparedFrames,
            "Every native lifetime frame, including deletion and impact, was compared");
    }
}
