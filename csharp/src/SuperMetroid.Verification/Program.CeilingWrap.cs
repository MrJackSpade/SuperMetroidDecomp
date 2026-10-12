using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static VerificationHelpersVerification;

internal static class CeilingWrapVerification
{
    internal static void VerifyCeilingWrapPlmTrace(string tracePath)
    {
        var native = File.ReadLines(tracePath).Skip(1).ToArray();
        AssertEqual(135, native.Length, "Native ceiling PLM allocation matrix");
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        // Invoke the real producer collision path with the native probe's stationary
        // projectile registers. This isolates collision/PLM allocation from launch AI.
        var collide = typeof(ProjectileTerrainCollision).GetMethod("RunInitialBeamCollision",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var system = new SamusProjectileSystem();
        var powerBomb = new SamusPowerBombExplosionState();
        int cursor = 0, mismatches = 0;
        for (int tileX = 63; tileX <= 65; tileX++)
        {
            var words = Enumerable.Repeat((ushort)0x0040, 128 * 32).ToArray();
            var level = CreateRoom(new(128, 32), words, new byte[words.Length]);
            var plms = new RoomPlmSystem();
            var shot = new SamusProjectileSlot(0) { Position = new(new((ushort)(tileX * 16), 0), new(4, 0)), Radius = new(1, 8), Type = 1,
                Direction = (ushort)SamusProjectileDirection.Right };
            for (int call = 0; call < 45; call++)
            {
                collide.Invoke(system.TerrainCollision, [level, shot, plms, true, powerBomb]);
                int owner = plms.SlotTable.ActiveCount == 0 ? 0 : plms.PopulationSlots[0].BlockIndex * 2;
                string actual = $"{tileX},{call},{plms.SlotTable.ActiveCount},{owner:X4},{level.GetCollisionBlockByIndex(tileX).LevelWord:X4},{level.GetCollisionBlockByIndex(tileX + 1).LevelWord:X4}";
                string expected = native[cursor++];
                if (actual != expected && mismatches++ < 8)
                    Console.WriteLine($"Ceiling PLM mismatch:\n{actual}\n{expected}");
            }
        }
        AssertEqual(0, mismatches, "Native odd-byte reaction, aligned mutation and slot exhaustion");
        Console.WriteLine("PASS 135 original-CPU ceiling PLM allocation records.");
    }

    [VerificationCommand("--ceiling-wrap")]
    internal static int CeilingWrapCommand()
    {
        Suite(nameof(VerifyCeilingWrapPlmTrace), () => VerifyCeilingWrapPlmTrace("csharp/test-fixtures/movement-release/ceiling-plm-410.csv"));
        return 0;
    }
}
