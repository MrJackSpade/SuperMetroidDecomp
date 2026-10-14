using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static Program;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static RetailRuntimeFixtureVerification;
using static VerificationHelpersVerification;

/// <summary>Checks beam lifetime under controlled camera movement against native deletion bounds.</summary>
internal static class HeroShotsVerification
{
    /// <summary>Checks horizontal and vertical shots at, inside, and beyond camera edges, optionally against a native trace.</summary>
    /// <remarks>The controlled camera is an independent input to the real projectile owner; the check isolates shot lifetime and collision from Samus camera tracking.</remarks>
    internal static void VerifyHeroShotCameraLifetime(string? nativeTrace = null)
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
            var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var blocks = new ushort[64 * 16];
            for (int row = 0; row < 16; row++) blocks[vertical ? 16 * 16 + row : row * 64 + 32] = 0x8000;
            var room = CreateRoom(vertical ? new(16, 64) : new(64, 16), blocks, new byte[blocks.Length]);
            var samus = new SamusState { Pose = vertical ? SamusPoseId.StandingAimUpRightPose : SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, (ushort)(vertical ? 640 : 128))) };
            var projectiles = CreateProjectileFixture();
            var shared = CreateBombFixture();
            int deletedFrame = -1, impactFrame = -1;
            for (int frame = 0; frame < 120; frame++)
            {
                ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
                var shot = projectiles.Slots[0];
                ushort cameraX = !vertical && followShot && shot.IsActive
                    ? (ushort)Math.Max(0, shot.Position.X.Whole - 128) : (ushort)0;
                ushort cameraY = vertical ? (ushort)(followShot && shot.IsActive ? shot.Position.Y.Whole - 128 : 512) : (ushort)0;
                if (cameraMode >= 2 && shot.IsActive)
                {
                    // Camera-only fixture control: put the *next* position at
                    // the native edge or one pixel outside. Production code
                    // independently performs movement, collision and deletion.
                    if (vertical)
                    {
                        uint next = unchecked((new SnesFixedPosition(shot.Position.Y.Whole, shot.Position.Y.Fraction).RawValue)
                            + (uint)((short)(shot.Velocity.Y.SignedRawValue - 16) * 256));
                        cameraY = unchecked((ushort)(SnesSixteenSixteen.WholeOf(next) + (cameraMode == 2 ? 64u : 65u)));
                    }
                    else
                    {
                        uint next = unchecked((new SnesFixedPosition(shot.Position.X.Whole, shot.Position.X.Fraction).RawValue)
                            + (uint)((short)(shot.Velocity.X.SignedRawValue + 16) * 256));
                        cameraX = unchecked((ushort)(SnesSixteenSixteen.WholeOf(next) - (cameraMode == 2 ? 319u : 320u)));
                    }
                }
                shared.StepFrame(new SamusTerrain { Bus = bus, Level = room }, samus, input, input);
                SamusProjectileSlotObservation beforeStep = projectiles.ObserveSlots();
                projectiles.StepFrame(new SamusTerrain { Bus = bus, Level = room }, samus, new ProjectileButtonInput { ControllerInput = input, ControllerNewInput = input }, new(cameraX, cameraY), shared);
                if (native is not null)
                {
                    ushort[] actual = [cameraX, cameraY, shot.Position.X.Whole, shot.Position.X.Fraction,
                        shot.Position.Y.Whole, shot.Position.Y.Fraction, unchecked((ushort)shot.Velocity.X.SignedRawValue),
                        unchecked((ushort)shot.Velocity.Y.SignedRawValue), shot.Type, shot.InstructionPointer];
                    AssertTrue(actual.SequenceEqual(native[(vertical ? 1 : 0, cameraMode, frame)]),
                        $"Native Hero trace vertical={vertical}, camera={cameraMode}, frame={frame}: actual {string.Join(',', actual.Select(x => x.ToString("X4")))}");
                    comparedFrames++;
                }
                if (frame == 0) AssertEqual((int?)0, beforeStep.FiredSlot(projectiles), "Hero fixture fires through normal input dispatch");
                if (shot.PackedType.Family == SamusProjectileFamily.BeamExplosion)
                {
                    impactFrame = frame;
                    AssertTrue(vertical ? shot.Position.Y.Whole is >= 248 and <= 288 : shot.Position.X.Whole is >= 496 and <= 520,
                        "Surviving shot explodes at the remote solid column in world space");
                    break;
                }
                if (!shot.IsActive) { deletedFrame = frame; break; }
                if (cameraMode == 2 && frame > 0) AssertEqual((short)(vertical ? -64 : 319),
                    unchecked((short)(vertical ? shot.Position.Y.Whole - cameraY : shot.Position.X.Whole - cameraX)),
                    "Shot stays active at the exact native camera-relative edge");
                if (!followShot) AssertTrue(vertical ? shot.Position.Y.Whole >= 448 : shot.Position.X.Whole < 320,
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

    /// <summary>Runs projectile camera-lifetime checks without a native trace file.</summary>
    [VerificationCommand("--hero-shots")]
    internal static int HeroShotsCommand()
    {
        Suite(nameof(VerifyHeroShotCameraLifetime), () => VerifyHeroShotCameraLifetime());
        return 0;
    }

    /// <summary>Checks each projectile frame against the supplied native trace as well as lifetime bounds.</summary>
    [VerificationCommand("--hero-shots", "nativeHeroTrace")]
    internal static int HeroShotsCommand2(string nativeHeroTrace)
    {
        Suite(nameof(VerifyHeroShotCameraLifetime), () => VerifyHeroShotCameraLifetime(nativeHeroTrace));
        return 0;
    }
}
