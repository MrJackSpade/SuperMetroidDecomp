using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Checks enemy spike-block reactions against native PLM headers and bus-free production collision behavior.</summary>
internal static class EnemyBreakableTerrainDefinitionsVerification
{
    /// <summary>
    /// The original's enemy-breakable block object, the one entry of its enemy spike-reaction table that spawns anything.
    /// Core spawns the break by what it does and carries no identity for it (#1622).
    /// </summary>
    private const ushort NativeEnemyBreakableBlockHeader = 0xd094;

    /// <summary>Confirms BTS low nibble fifteen spawns a breakable PLM and clears terrain, while other authored selectors retain collision.</summary>
    internal static void VerifyEnemyBreakableTerrainDefinitions(CartridgeImportAddressSpace rom)
    {
        for (int index = 0; index < 16; index++)
        {
            ushort header = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa0c2da + index * 2));
            AssertEqual(index == 15 ? NativeEnemyBreakableBlockHeader : (ushort)0,
                header,
                $"enemy spike reaction header {index}");
        }

        for (int highBit = 0; highBit <= 0x80; highBit += 0x80)
        for (int index = 0; index < 16; index++)
        {
            byte behavior = (byte)(highBit | index);
            AssertEqual(index == 15,
                EnemyBreakableTerrain.IsEnemyBreakable(RoomBlockBehaviorCodec.Decode(behavior)),
                $"compiled enemy spike reaction ${behavior:X2}");

            var level = new RoomLevelData(new Extent16<RoomBlocks>(1, 1), new RoomLevelPlanes(
                [RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SpikeBlock)],
                [RoomBlockBehaviorCodec.Decode(behavior)],
                [RoomBlockAppearance.Of(0, BlockMirroring.None)]), RoomLevelData.NothingBeyondGrid());
            var roomObjects = RoomObjectFixtures.Unbound();
            var terrain = new EnemyTerrain();
            RoomCollisionBlock block = level.GetCollisionBlock(Position16<RoomBlocks>.Zero);
            bool collision;
            using (new EnemyTerrain.EnemyTerrainScope(terrain, roomObjects.BreakableTerrain))
                collision = terrain.ReactToEnemySpikeBlock(level, Position16<RoomBlocks>.Zero, block);

            AssertEqual(index != 15, collision,
                $"production enemy spike collision ${behavior:X2}");
            AssertEqual(index == 15 ? 1 : 0, roomObjects.PopulationSlots.Count,
                $"production enemy spike PLM count ${behavior:X2}");
            // The break is the room object that makes the block's break, known by what it does rather than an identity.
            if (index == 15)
                AssertTrue(roomObjects.PopulationSlots.Single() is
                    {
                        Block: { X: 0, Y: 0 },
                        Behaviour: TerrainChangeObject { Change: var change },
                    } && change == ScriptedTerrainChangeDefinitions.EnemyBreakableBlockBreak,
                    $"production enemy spike ${behavior:X2} spawns the block's break at the block");
            AssertEqual(index == 15 ? RoomCollisionType.Air : RoomCollisionType.SpikeBlock,
                level.GetCollisionBlock(Position16<RoomBlocks>.Zero).CollisionType,
                $"production enemy spike resulting collision type ${behavior:X2}");
        }

        for (int behavior = 0; behavior <= byte.MaxValue; behavior++)
        {
            if ((behavior & 0x7f) <= 15) continue;
            byte invalid = (byte)behavior;
            AssertThrows<InvalidDataException>(
                () => EnemyBreakableTerrain.IsEnemyBreakable(RoomBlockBehaviorCodec.Decode(invalid)),
                $"out-of-domain enemy spike BTS ${invalid:X2}");
        }

        Console.WriteLine(
            "Enemy terrain reactions: sixteen native headers, all 32 authored/high-bit selectors and real collision/PLM handoffs pass through the bus-free terrain owner; 224 adjacent-code selectors fail explicitly.");
    }
}
