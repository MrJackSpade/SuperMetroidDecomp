using SuperMetroid.Core.Rooms.LevelData;
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
using SuperMetroid.Core.Progression;

/// <summary>Checks a missile impact at two camera positions against a native frame trace.</summary>
internal static class MissileEdgeVerification
{
    /// <summary>The recorded trace's column holding the projectile type word, after the edge and frame columns.</summary>
    private const int NativeTypeColumn = 6;

    /// <summary>Compares projectile position, velocity, type, instruction, and explosion lifetime on every recorded frame.</summary>
    internal static void VerifyMissileImpactCameraEdge(string nativeTrace)
    {
        var native = File.ReadLines(nativeTrace).Skip(1).Select(line => line.Split(','))
            .ToDictionary(row => new MissileEdgeNative(int.Parse(row[0]), int.Parse(row[1])), row => row.Skip(2)
                .Select(value => Convert.ToUInt16(value, 16)).ToArray());
        int compared = 0;
        for (int edge = 0; edge < 2; edge++)
        {
            var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var blocks = new RoomTerrainBlock[64 * 16];
            for (int row = 0; row < 16; row++) blocks[row * 64 + 32] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
            var room = CreateRoom(new(64, 16), blocks, new RoomBlockBehavior[blocks.Length]);
            var samus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(497, 128)),
                SelectedHudItem = HudItem.Missile, Missiles = 5, MaxMissiles = 5 };
            var projectiles = CreateProjectileFixture();
            var shared = CreateBombFixture();
            for (int frame = 0; frame < 20; frame++)
            {
                ControllerButton input = frame == 0 ? ControllerButton.X : 0;
                shared.StepFrame(new SamusTerrain { Level = room }, samus, input, input);
                projectiles.StepFrame(new SamusTerrain { Level = room }, samus, new ProjectileButtonInput { ControllerInput = input, ControllerNewInput = input }, new((ushort)(edge == 1 ? 192 : 384), 0), shared);
                if (frame == 0) AssertEqual(4, samus.Missiles,
                    "Missile edge fixture uses actual fire dispatch");
                var shot = projectiles.Slots[0];
                ushort[] expected = native[new MissileEdgeNative(edge, frame)];
                ushort[] actual = [shot.Position.X.Whole, shot.Position.X.Fraction, shot.Position.Y.Whole, shot.Position.Y.Fraction,
                    unchecked((ushort)shot.Velocity.X.SignedRawValue), unchecked((ushort)shot.Velocity.Y.SignedRawValue), checked((ushort)shot.InstructionPointer)];
                // The recorded type word is compared as the projectile it decodes to.
                AssertTrue(actual.SequenceEqual(expected.Where((_, column) => column != NativeTypeColumn)),
                    $"Missile edge={edge} frame={frame} native comparison: {string.Join(',', actual.Select(v => v.ToString("X4")))}");
                AssertEqual(SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(expected[NativeTypeColumn]), shot.Type,
                    $"Missile edge={edge} frame={frame} projectile matches the recorded type");
                compared++;
                if (!shot.IsActive || shot.Type.Family == SamusProjectileFamily.MissileExplosion)
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
