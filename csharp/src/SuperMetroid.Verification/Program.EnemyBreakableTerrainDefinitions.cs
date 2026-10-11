using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

internal static partial class Program
{
    private static void VerifyEnemyBreakableTerrainDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 16; index++)
        {
            ushort header = ReadEnemyTerrainWord(rom, 0xa0c2da + index * 2);
            AssertEqual(index == 15 ? (ushort)PlmHeaderId.EnemyBreakableBlock : (ushort)0,
                header,
                $"enemy spike reaction header {index}");
        }

        MethodInfo react = typeof(RoomEnemySystem).GetMethod(
            "ReactToEnemySpikeBlock",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo busField = typeof(RoomEnemySystem).GetField(
            "_bus", BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo plmField = typeof(RoomEnemySystem).GetField(
            "_collisionPlms", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var guarded = NewEnemyTerrainReactionReadGuard(rom);
        for (int highBit = 0; highBit <= 0x80; highBit += 0x80)
        for (int index = 0; index < 16; index++)
        {
            byte behavior = (byte)(highBit | index);
            AssertEqual(index == 15,
                EnemyBreakableTerrainDefinitions.IsEnemyBreakable(behavior),
                $"compiled enemy spike reaction ${behavior:X2}");

            var level = new RoomLevelData(
                new Extent16<RoomBlocks>(1, 1),
                [RoomLevelWord.Create(0, LevelBlockFlipFlags.None, RoomCollisionType.SpikeBlock).Raw],
                [behavior],
                [0],
                []);
            var plms = new RoomPlmSystem();
            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            plmField.SetValue(enemies, plms);
            RoomCollisionBlock block = level.GetCollisionBlockByIndex(0);
            bool collision = (bool)react.Invoke(enemies, [level, 0, block])!;

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
            "Enemy terrain reactions: sixteen native headers, all 32 authored/high-bit selectors and real collision/PLM handoffs pass with the reaction table forbidden; 224 adjacent-code selectors fail explicitly.");
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for enemy terrain words")]
    private static ushort ReadEnemyTerrainWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadCartridgeByte(address) | bus.ReadCartridgeByte(address + 1) << 8);

    private static CartridgeReadGuard NewEnemyTerrainReactionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa0c2da and < 0xa0c2fa,
                address => $"Enemy terrain reaction attempted migrated table read ${address:X6}.");
}
