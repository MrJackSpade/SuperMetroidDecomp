using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks Wave beam door reactions at tile, screen, and signed-coordinate Y boundaries.</summary>
internal static class WaveDoorBandsVerification
{
    /// <summary>Checks the XBA/BMI guard at pixel-half, screen, and signed-coordinate boundaries.</summary>
    internal static void VerifyHorizontalWaveDoorBands()
    {
        foreach (ushort y in new ushort[] { 64, 127, 128, 192, 255, 256, 383, 384, 390, 511, 512, 65408 })
        foreach (bool left in new[] { false, true })
        {
            var words = new RoomTerrainBlock[16 * 32];
            var behaviors = new RoomBlockBehavior[words.Length];
            bool inside = y < 512;
            int target = inside ? (y >> 4) * 16 + 4 : 4;
            words[target] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.ShootableBlock);
            behaviors[target] = (left ? RoomBlockBehaviorValues.BlueDoorFacingRight : RoomBlockBehaviorValues.BlueDoorFacingLeft);
            var level = CreateRoom(new(16, 32), words, behaviors);
            var plms = PlmFixtures.Unbound();
            var shot = new SamusProjectileSlot(0)
            {
                Position = new(new(72, 0), new(y, 0)), Radius = new(1, 0),
                Velocity = new((short)(left ? -1 : 1), 0), Type = SamusProjectileType.Beam(SamusBeamCombinations.Of(SamusBeamFlags.Wave), charged: false),
            };
            // A one-pixel span avoids testing adjacent-row aggregation at the boundary.
            shot.Radius = shot.Radius.WithY(1);
            if (inside && (y & 15) == 0 && y > 0)
            {
                words[target - 16] = words[target];
                behaviors[target - 16] = behaviors[target];
                level = CreateRoom(new(16, 32), words, behaviors);
            }
            ProjectileTerrainCollision.ScanHorizontalWaveShotReactions(level, shot, plms);
            AssertEqual(inside ? ((y & 15) == 0 && y > 0 ? 2 : 1) : 0, plms.SlotTable.ActiveCount,
                $"Wave door reaction at Y={y}, left={left}: XBA tests the original high byte");
        }
        Console.WriteLine("PASS horizontal Wave door reactions across pixel/screen/sign boundaries (24 cases).");
    }
}
