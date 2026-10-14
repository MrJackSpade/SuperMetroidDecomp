using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks a missile impact at two camera positions against a native frame trace.</summary>
internal static class MissileEdgeVerification
{
    /// <summary>Compares projectile position, velocity, type, instruction, and explosion lifetime on every recorded frame.</summary>
    internal static void VerifyMissileImpactCameraEdge(string nativeTrace)
    {
        var native = File.ReadLines(nativeTrace).Skip(1).Select(line => line.Split(','))
            .ToDictionary(row => (int.Parse(row[0]), int.Parse(row[1])), row => row.Skip(2)
                .Select(value => Convert.ToUInt16(value, 16)).ToArray());
        int compared = 0;
        for (int edge = 0; edge < 2; edge++)
        {
            var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var blocks = new ushort[64 * 16];
            for (int row = 0; row < 16; row++) blocks[row * 64 + 32] = 0x8000;
            var room = CreateRoom(new(64, 16), blocks, new byte[blocks.Length]);
            var samus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(497, 128)),
                SelectedHudItem = 1, Missiles = 5, MaxMissiles = 5 };
            var projectiles = CreateProjectileFixture();
            var shared = CreateBombFixture();
            for (int frame = 0; frame < 20; frame++)
            {
                ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
                shared.StepFrame(new SamusTerrain { Bus = bus, Level = room }, samus, input, input);
                projectiles.StepFrame(new SamusTerrain { Bus = bus, Level = room }, samus, new ProjectileButtonInput { ControllerInput = input, ControllerNewInput = input }, new((ushort)(edge == 1 ? 192 : 384), 0), shared);
                if (frame == 0) AssertTrue(projectiles.LastFiredProjectileSnapshot is not null,
                    "Missile edge fixture uses actual fire dispatch");
                var shot = projectiles.Slots[0];
                ushort[] actual = [shot.Position.X.Whole, shot.Position.X.Fraction, shot.Position.Y.Whole, shot.Position.Y.Fraction,
                    unchecked((ushort)shot.Velocity.X.SignedRawValue), unchecked((ushort)shot.Velocity.Y.SignedRawValue), shot.Type, shot.InstructionPointer];
                AssertTrue(actual.SequenceEqual(native[(edge, frame)]),
                    $"Missile edge={edge} frame={frame} native comparison: {string.Join(',', actual.Select(v => v.ToString("X4")))}");
                compared++;
                if (!shot.IsActive || shot.PackedType.Family == SamusProjectileFamily.MissileExplosion)
                {
                    AssertEqual(edge == 0, shot.IsActive, "Only the centered-camera impact retains its explosion");
                    break;
                }
            }
        }
        AssertEqual(native.Count, compared, "Every native missile impact and lifetime frame is checked");
        Console.WriteLine($"Missile impact camera edge: {compared} original-cartridge frames match, including retained and deleted explosions.");
    }

    /// <summary>Runs the focused camera-edge missile trace comparison.</summary>
    [VerificationCommand("--missile-edge")]
    internal static int MissileEdgeCommand()
    {
        Suite(nameof(VerifyMissileImpactCameraEdge), () => VerifyMissileImpactCameraEdge("csharp/test-fixtures/movement-release/missile-edge-602.csv"));
        return 0;
    }
}
