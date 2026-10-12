using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

/// <summary>Represents enemy breakable terrain definitions verification.</summary>
internal static class EnemyBreakableTerrainDefinitionsVerification
{
    /// <summary>Verifies enemy breakable terrain definitions.</summary>
    internal static void VerifyEnemyBreakableTerrainDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 16; index++)
        {
            ushort header = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa0c2da + index * 2));
            AssertEqual(index == 15 ? (ushort)PlmHeaderId.EnemyBreakableBlock : (ushort)0,
                header,
                $"enemy spike reaction header {index}");
        }

        for (int highBit = 0; highBit <= 0x80; highBit += 0x80)
        for (int index = 0; index < 16; index++)
        {
            byte behavior = (byte)(highBit | index);
            AssertEqual(index == 15,
                EnemyBreakableTerrainDefinitions.IsEnemyBreakable(behavior),
                $"compiled enemy spike reaction ${behavior:X2}");

            var level = new RoomLevelData(new Extent16<RoomBlocks>(1, 1), new RoomLevelPlanes([RoomLevelWord.Create(0, LevelBlockFlipFlags.None, RoomCollisionType.SpikeBlock).Raw], [behavior], [0]), []);
            var plms = new RoomPlmSystem();
            // EnemyTerrain owns no bus, so the reaction cannot read cartridge memory.
            var terrain = new EnemyTerrain();
            RoomCollisionBlock block = level.GetCollisionBlockByIndex(0);
            bool collision;
            using (new EnemyTerrain.EnemyTerrainScope(terrain, plms))
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
                () => EnemyBreakableTerrainDefinitions.IsEnemyBreakable(invalid),
                $"out-of-domain enemy spike BTS ${invalid:X2}");
        }

        Console.WriteLine(
            "Enemy terrain reactions: sixteen native headers, all 32 authored/high-bit selectors and real collision/PLM handoffs pass through the bus-free terrain owner; 224 adjacent-code selectors fail explicitly.");
    }
}
