using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.LevelData;

/// <summary>Checks compiled slope heights and their real Samus, projectile, bomb, and enemy consumers.</summary>
internal static class SharedSlopeHeightsVerification
{
    /// <summary>Compares all 512 native samples and mirrored collision outcomes while blocking slope ROM reads.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for slope heights")]
    internal static void VerifyCompiledSlopeHeights(SuperMetroidAddressSpace rom)
    {
        byte[] native = new byte[512];
        for (int i = 0; i < native.Length; i++)
        {
            native[i] = rom.ReadCartridgeByte(SlopeHeightDefinitions.ReferenceAddress + i);
            AssertEqual((byte)(native[i] & 31), SlopeHeightDefinitions.Read(i / 16, i % 16), "All native slope height samples");
        }
        foreach (int invalid in new[] { -1, int.MinValue, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => SlopeHeightDefinitions.Read(invalid, 0), "invalid slope shape");
            AssertThrows<ArgumentOutOfRangeException>(() => SlopeHeightDefinitions.Read(0, invalid), "invalid slope column");
        }
        for (int shape = 0; shape < 32; shape++)
            AssertThrows<ArgumentOutOfRangeException>(() => SlopeHeightDefinitions.Read(shape, 16), "column cannot cross profile seam");
        for (int column = 0; column < 16; column++)
            AssertThrows<ArgumentOutOfRangeException>(() => SlopeHeightDefinitions.Read(32, column), "shape cannot exceed catalog");
        var forbidden = NewSlopeHeightNoReadBus();
        var enemies = new RoomEnemySystem();
        var enemy = enemies.SlotTable.Slots[0];
        var align = new Func<RoomLevelData, RoomEnemySlot, Position16<RoomPixels>, bool, bool>(EnemyTerrain.AlignAgainstSlopeAtPixel);
        var missile = new Func<RoomCollisionBlock, SamusProjectileSlot, bool, bool>(ProjectileMotion.MissileSlopePointReaction);
        var bomb = new Func<RoomLevelData, SamusBombProjectileSlot, bool>(SamusBombProjectileSystem.BombSpreadCollides);
        var projectile = new SamusProjectileSlot(0);
        var spread = new SamusBombProjectileSlot(0);
        int collisionCases = 0;
        for (int raw = 0; raw < 256; raw++)
        {
            var bts = new RoomBlockBehavior((byte)raw);
            var level = new RoomLevelData(new Extent16<RoomBlocks>(1, 1), new RoomLevelPlanes([0x1000], [(byte)raw], [0]), new byte[8]);
            var block = level.GetCollisionBlock(new(0, 0));
            for (ushort x = 0; x < 16; x++)
            {
                int column = (raw & 64) != 0 ? 15 - x : x;
                byte height = (byte)(native[(raw & 31) * 16 + column] & 31);
                foreach (ushort origin in new ushort[] { 0, 0x1230, 0xfff0 })
                    AssertEqual(height, SamusSlopePhysics.ReadAlignmentHeight(forbidden, bts, (ushort)(origin + x)), "Samus real mirrored sample independent of world high bits");
                if ((raw & 31) < 5) continue; // Square slopes have a separate table.
                for (ushort y = 0; y < 16; y++)
                {
                    int edge = (raw & 128) != 0 ? 15 - y : y;
                    bool solid = height <= edge;
                    spread.Position = spread.Position.WithWhole(new(x, y));
                    projectile.Position = projectile.Position.WithWhole(new(x, y));
                    AssertEqual(solid, missile(block, projectile, true), "Missile horizontal slope point collision");
                    AssertEqual(solid, missile(block, projectile, false), "Missile vertical slope point collision");
                    AssertEqual(solid, bomb(level, spread), "Bomb spread real slope collision");
                    enemy.Position = enemy.Position.WithY(new(128, 0x1234));
                    int adjustment = height - edge - 1;
                    bool changed = align(level, enemy, new(x, y), (raw & 128) != 0);
                    AssertEqual(adjustment < 0, changed, "Enemy real slope alignment admission");
                    int expectedY = 128 + (adjustment < 0 ? (raw & 128) != 0 ? -adjustment : adjustment : 0);
                    AssertEqual((ushort)expectedY, enemy.Position.Y.Whole, "Enemy exact floor/ceiling alignment Y");
                    AssertEqual((ushort)0x1234, enemy.Position.Y.Fraction, "Enemy alignment preserves fractional Y");
                    collisionCases++;
                }
            }
        }
        AssertEqual(55296, collisionCases, "All non-square shapes/orientations/points covered");
        Console.WriteLine("Shared slopes: 512 native heights, 12288 Samus samples and 55296 cross-consumer collision/alignment cases pass with ROM reads forbidden.");
    }

    /// <summary>Creates an unbacked bus that rejects every cartridge read and write during slope use.</summary>
    internal static CartridgeReadGuard NewSlopeHeightNoReadBus() =>
        CartridgeReadGuard.Unbacked()
            .ForbidAll(address => $"Unexpected slope ROM read at {address:X6}.")
            .RejectWrites(_ => "Unexpected slope bus write.");
}
