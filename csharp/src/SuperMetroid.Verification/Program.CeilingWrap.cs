using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCeilingWrapPlmTrace(string tracePath)
    {
        var native = File.ReadLines(tracePath).Skip(1).ToArray();
        AssertEqual(135, native.Length, "Native ceiling PLM allocation matrix");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        // Invoke the real producer collision path with the native probe's stationary
        // projectile registers. This isolates collision/PLM allocation from launch AI.
        var collide = typeof(SamusProjectileSystem).GetMethod("RunInitialBeamCollision",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        int cursor = 0, mismatches = 0;
        for (int tileX = 63; tileX <= 65; tileX++)
        {
            var words = Enumerable.Repeat((ushort)0x0040, 128 * 32).ToArray();
            var level = CreateRoom(128, 32, words, new byte[words.Length]);
            var plms = new RoomPlmSystem();
            var shot = new SamusProjectileSlot(0) { XPosition = (ushort)(tileX * 16),
                YPosition = 4, XRadius = 1, YRadius = 8, Type = 1,
                Direction = (ushort)SamusProjectileDirection.Right };
            for (int call = 0; call < 45; call++)
            {
                collide.Invoke(null, [bus, level, shot, plms, true]);
                int owner = plms.ActiveCount == 0 ? 0 : plms.PopulationSlots[0].BlockIndex * 2;
                string actual = $"{tileX},{call},{plms.ActiveCount},{owner:X4},{level.GetCollisionBlockByIndex(tileX).LevelWord:X4},{level.GetCollisionBlockByIndex(tileX + 1).LevelWord:X4}";
                string expected = native[cursor++];
                if (actual != expected && mismatches++ < 8)
                    Console.WriteLine($"Ceiling PLM mismatch:\n{actual}\n{expected}");
            }
        }
        AssertEqual(0, mismatches, "Native odd-byte reaction, aligned mutation and slot exhaustion");
        Console.WriteLine("PASS 135 original-CPU ceiling PLM allocation records.");
    }
}
