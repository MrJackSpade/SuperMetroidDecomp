using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Compares ceiling projectile reactions and PLM allocation with a native CPU trace.</summary>
internal static class CeilingWrapVerification
{
    /// <summary>
    /// Replays stationary beam collisions at three ceiling tile positions and compares 135
    /// PLM owner, tile mutation and exhaustion records with the native trace at <paramref name="tracePath"/>.
    /// </summary>
    internal static void VerifyCeilingWrapPlmTrace(string tracePath)
    {
        var native = File.ReadLines(tracePath).Skip(1).ToArray();
        AssertEqual(135, native.Length, "Native ceiling PLM allocation matrix");
        // Invoke the real producer collision path with the native probe's stationary
        // projectile registers. This isolates collision/PLM allocation from launch AI.
        var system = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads);
        var powerBomb = new SamusPowerBombExplosionState();
        int cursor = 0, mismatches = 0;
        for (int tileX = 63; tileX <= 65; tileX++)
        {
            var words = Enumerable.Repeat(RoomTerrainBlock.Of(0x040, BlockMirroring.None, RoomCollisionType.Air), 128 * 32).ToArray();
            var level = CreateRoom(new(128, 32), words, new RoomBlockBehavior[words.Length]);
            var plms = PlmFixtures.Unbound();
            var shot = new SamusProjectileSlot(0) { Position = new(new((ushort)(tileX * 16), 0), new(4, 0)), Radius = new(1, 8), Type = SamusProjectileType.Beam(SamusBeamCombination.Wave, charged: false),
                Direction = SamusProjectileDirection.Right };
            for (int call = 0; call < 45; call++)
            {
                system.TerrainCollision.RunInitialBeamCollision(level, shot, plms, true, powerBomb);
                int owner = plms.SlotTable.ActiveCount == 0 ? 0 : plms.PopulationSlots[0].BlockIndex * 2;
                string actual = $"{tileX},{call},{plms.SlotTable.ActiveCount},{owner:X4},{RoomLevelWordEncoder.Encode(level.GetCollisionBlockByIndex(tileX).Terrain):X4},{RoomLevelWordEncoder.Encode(level.GetCollisionBlockByIndex(tileX + 1).Terrain):X4}";
                string expected = native[cursor++];
                if (actual != expected && mismatches++ < 8)
                    Console.WriteLine($"Ceiling PLM mismatch:\n{actual}\n{expected}");
            }
        }
        AssertEqual(0, mismatches, "Native odd-byte reaction, aligned mutation and slot exhaustion");
        Console.WriteLine("PASS 135 original-CPU ceiling PLM allocation records.");
    }

    /// <summary>Runs the ceiling collision trace comparison using the recorded fixture.</summary>
    [VerificationCommand("--ceiling-wrap")]
    internal static int CeilingWrapCommand()
    {
        Suite(nameof(VerifyCeilingWrapPlmTrace), () => VerifyCeilingWrapPlmTrace("csharp/test-fixtures/movement-release/ceiling-plm-410.csv"));
        return 0;
    }
}
