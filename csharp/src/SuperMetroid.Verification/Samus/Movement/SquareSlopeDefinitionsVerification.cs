using SuperMetroid.AssetExtraction;
using static SuperMetroid.Verification.Rooms.LevelData.SharedSlopeHeightsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>Checks compiled Samus and enemy square-slope quadrants against native byte tables.</summary>
internal static class SquareSlopeDefinitionsVerification
{
    /// <summary>Runs separate missile-point and enemy-quadrant comparisons.</summary>
    internal static void VerifyCompiledSquareSlopes(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifySamusSquareSlopeAlgorithm), () => VerifySamusSquareSlopeAlgorithm(rom));
        Suite(nameof(VerifyEnemySquareSlopeAlgorithm), () => VerifyEnemySquareSlopeAlgorithm(rom));
    }

    /// <summary>The square slope shapes in the order the native tables hold them, four quadrant bytes each.</summary>
    private static readonly SquareSlopeShape[] NativeShapeOrder =
    [
        SquareSlopeShape.BottomHalf, SquareSlopeShape.RightHalf, SquareSlopeShape.BottomRightQuarter,
        SquareSlopeShape.AllButTopLeft, SquareSlopeShape.Full,
    ];

    /// <summary>The native solid quadrant byte's solidity bit.</summary>
    private const byte NativeSolidBit = 0x80;

    /// <summary>The quadrant a native table's byte describes: bit zero the right half, bit one the bottom half.</summary>
    private static SquareSlopeQuadrant NativeQuadrant(int index) => new(SquareSlopeQuadrant.ColumnFor((index & 1) != 0), SquareSlopeQuadrant.RowFor((index & 2) != 0));

    /// <summary>Whether the shape and quadrant the native index names are solid.</summary>
    private static bool SolidAtNativeIndex(int index) => SquareSlopes.IsSolid(NativeShapeOrder[index / 4], NativeQuadrant(index));

    /// <summary>Checks all native Samus quadrant bytes and actual missile reactions for five shapes and BTS orientations.</summary>
    private static void VerifySamusSquareSlopeAlgorithm(CartridgeImportAddressSpace rom)
    {
        var native = new byte[20];
        for (int i = 0; i < native.Length; i++)
        {
            native[i] = rom.ReadCartridgeByte(SamusProjectileRomDataCollision.SquareSlopeDefinitions + i);
            AssertEqual(native[i], SolidAtNativeIndex(i) ? NativeSolidBit : (byte)0, "Native Samus square quadrant byte");
        }
        for (int shape = 0; shape < 32; shape++)
        {
            var behavior = RoomBlockBehaviorCodec.Decode(shape);
            if (shape < NativeShapeOrder.Length)
                AssertEqual(NativeShapeOrder[shape], SquareSlopes.ShapeOf(behavior), $"square slope shape {shape}");
            else
                AssertThrows<ArgumentOutOfRangeException>(() => SquareSlopes.ShapeOf(behavior),
                    $"non-square slope shape {shape} has no square quadrants");
        }

        var missile = new Func<RoomCollisionBlock, SamusProjectile, bool>(ProjectileMotion.MissileSlopePointReaction);
        var bus = NewSlopeHeightNoReadBus();
        var projectile = new SamusProjectile(0);
        int cases = 0;
        for (int raw = 0; raw < 256; raw++)
        {
            int shape = raw & 31;
            if (shape >= 5) continue;
            var level = new RoomLevelData(new Extent16<RoomBlocks>(1, 1), new RoomLevelPlanes([RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.Slope)], [RoomBlockBehaviorCodec.Decode(raw)], [RoomBlockAppearance.Of(0)]), RoomLevelData.NothingBeyondGrid());
            var block = level.GetCollisionBlock(new(0, 0));
            for (ushort x = 0; x < 16; x++)
            for (ushort y = 0; y < 16; y++)
            {
                int column = (raw & 64) != 0 ? 15 - x : x;
                int row = (raw & 128) != 0 ? 15 - y : y;
                bool expected = native[shape * 4 + row / 8 * 2 + column / 8] != 0;
                projectile.Position = projectile.Position.WithWhole(new(x, y));
                AssertEqual(expected, missile(block, projectile), "Actual missile square quadrant");
                cases++;
            }
        }
        AssertEqual(10240, cases, "Every square shape/BTS orientation/pixel");
        Console.WriteLine("Samus square-slope algorithm: 20 native bytes, shape bounds and 10240 real missile point cases pass without ROM reads.");
    }

    /// <summary>Compares both native enemy quadrant copies and rejects indices outside their twenty-byte range.</summary>
    private static void VerifyEnemySquareSlopeAlgorithm(CartridgeImportAddressSpace rom)
    {
        for (int index = 0; index < 20; index++)
        {
            // The enemy copies also keep the quadrant's own index in their two low bits.
            byte expected = (byte)((SolidAtNativeIndex(index) ? NativeSolidBit : 0) | (index & 3));
            AssertEqual(expected, rom.ReadCartridgeByte(SquareSlopes.EnemyReferenceAddress + index),
                $"enemy square quadrant byte {index}, including identity bits");
            AssertEqual(expected, rom.ReadCartridgeByte(SquareSlopes.ProjectileReferenceAddress + index),
                $"enemy-projectile square quadrant copy {index}");
        }
        Console.WriteLine("Enemy square-slope algorithm: both twenty-byte native copies pass.");
    }
}
