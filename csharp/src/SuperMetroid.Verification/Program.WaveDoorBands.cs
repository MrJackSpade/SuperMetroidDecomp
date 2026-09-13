using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks the XBA/BMI guard at pixel-half, screen, and signed-coordinate boundaries.</summary>
    private static void VerifyHorizontalWaveDoorBands()
    {
        foreach (ushort y in new ushort[] { 64, 127, 128, 192, 255, 256, 383, 384, 390, 511, 512, 65408 })
        foreach (bool left in new[] { false, true })
        {
            var words = new ushort[16 * 32];
            var behaviors = new byte[words.Length];
            bool inside = y < 512;
            int target = inside ? (y >> 4) * 16 + 4 : 4;
            words[target] = (ushort)((int)RoomCollisionType.ShootableBlock << 12);
            behaviors[target] = (left ? RoomBlockBehaviorValues.BlueDoorFacingRight : RoomBlockBehaviorValues.BlueDoorFacingLeft).Value;
            var level = CreateRoom(16, 32, words, behaviors);
            var plms = new RoomPlmSystem();
            var shot = new SamusProjectileSlot(0)
            {
                XPosition = 72, YPosition = y, XRadius = 1, YRadius = 0,
                XVelocity = (short)(left ? -1 : 1), Type = (ushort)SamusBeamFlags.Wave,
            };
            // A one-pixel span avoids testing adjacent-row aggregation at the boundary.
            shot.YRadius = 1;
            if (inside && (y & 15) == 0 && y > 0)
            {
                words[target - 16] = words[target];
                behaviors[target - 16] = behaviors[target];
                level = CreateRoom(16, 32, words, behaviors);
            }
            typeof(SamusProjectileSystem).GetMethod("ScanHorizontalWaveShotReactions", BindingFlags.Static | BindingFlags.NonPublic)!
                .Invoke(null, [level, shot, plms]);
            AssertEqual(inside ? ((y & 15) == 0 && y > 0 ? 2 : 1) : 0, plms.ActiveCount,
                $"Wave door reaction at Y={y}, left={left}: XBA tests the original high byte");
        }
        Console.WriteLine("PASS horizontal Wave door reactions across pixel/screen/sign boundaries (24 cases).");
    }
}
