using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Checks enemy spike-block reactions against native PLM headers and bus-free production collision behavior.</summary>
internal static class EnemyBreakableTerrainDefinitionsVerification
{
    /// <summary>Confirms BTS low nibble fifteen spawns a breakable PLM and clears terrain, while other authored selectors retain collision.</summary>
    internal static void VerifyEnemyBreakableTerrainDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 16; index++)
        {
            ushort header = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa0c2da + index * 2));
            AssertEqual(index == 15 ? PlmNativeWords.Word(PlmHeaderId.EnemyBreakableBlock) : (ushort)0,
                header,
                $"enemy spike reaction header {index}");
        }

        for (int highBit = 0; highBit <= 0x80; highBit += 0x80)
        for (int index = 0; index < 16; index++)
        {
            byte behavior = (byte)(highBit | index);
            AssertEqual(index == 15,
                EnemyBreakableTerrain.IsEnemyBreakable(new RoomBlockBehavior(behavior)),
                $"compiled enemy spike reaction ${behavior:X2}");

            var level = new RoomLevelData(new Extent16<RoomBlocks>(1, 1), new RoomLevelPlanes([RoomLevelWord.Create(0, LevelBlockFlipFlags.None, RoomCollisionType.SpikeBlock).Raw], [behavior], [0]));
            var plms = PlmFixtures.Unbound();
            var terrain = new EnemyTerrain();
            RoomCollisionBlock block = level.GetCollisionBlockByIndex(0);
            bool collision;
            using (new EnemyTerrain.EnemyTerrainScope(terrain, plms.BreakableTerrain))
                collision = terrain.ReactToEnemySpikeBlock(level, 0, block);

            AssertEqual(index != 15, collision,
                $"production enemy spike collision ${behavior:X2}");
            AssertEqual(index == 15 ? 1 : 0, plms.PopulationSlots.Count,
                $"production enemy spike PLM count ${behavior:X2}");
            AssertEqual(index == 15 ? RoomCollisionType.Air : RoomCollisionType.SpikeBlock,
                level.GetCollisionBlockByIndex(0).CollisionType,
                $"production enemy spike resulting collision type ${behavior:X2}");
        }

        for (int behavior = 0; behavior <= byte.MaxValue; behavior++)
        {
            if ((behavior & 0x7f) <= 15) continue;
            byte invalid = (byte)behavior;
            AssertThrows<InvalidDataException>(
                () => EnemyBreakableTerrain.IsEnemyBreakable(new RoomBlockBehavior(invalid)),
                $"out-of-domain enemy spike BTS ${invalid:X2}");
        }

        Console.WriteLine(
            "Enemy terrain reactions: sixteen native headers, all 32 authored/high-bit selectors and real collision/PLM handoffs pass through the bus-free terrain owner; 224 adjacent-code selectors fail explicitly.");
    }
}
