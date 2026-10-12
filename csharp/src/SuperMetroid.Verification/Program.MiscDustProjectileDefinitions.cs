using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMiscDustProjectileDefinitions(SuperMetroidAddressSpace rom)
    {
        const int instructionTable = 0x86e42c;
        const int placementTable = 0x86e47e;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (ushort index = 0; index < 30; index++)
        {
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord((instructionTable + index * 2));
            AssertEqual(native, MiscDustProjectileDefinitions.InstructionList(index),
                $"misc-dust instruction selector {index}");

            var enemies = CreateMiscDustEnemySystem(rom);
            var spawn = typeof(RoomEnemySystem).GetMethod(
                    "SpawnRoomGraphicsDustExplosion", flags)!
                .CreateDelegate<Action<Position16<RoomPixels>, ushort>>(enemies);
            spawn(new(0x1234, 0x5678), index);
            RoomEnemyProjectileSlot roomSlot = enemies.EnemyProjectiles.Single(slot => slot.IsActive);
            AssertEqual(native, roomSlot.InstructionPointer,
                $"room-graphics misc-dust production selector {index}");
        }

        for (ushort index = 0; index < 5; index++)
        {
            int address = placementTable + index * 8;
            MiscDustPlacementDefinition definition =
                MiscDustProjectileDefinitions.SmokePlacement(index);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(address), definition.RandomMask.X,
                $"misc-dust placement X mask {index}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 2)), definition.RandomMask.Y,
                $"misc-dust placement Y mask {index}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 4))), definition.Base.X,
                $"misc-dust placement X base {index}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 6))), definition.Base.Y,
                $"misc-dust placement Y base {index}");

            const ushort random = 0x5aad;
            int advances = 0;
            var enemies = CreateMiscDustEnemySystem(rom);
            enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => random);
            enemies.VerificationServices().NextRandom = (Func<ushort>)(() => { advances++; return 0xbeef; });
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeEyeDoorSmoke", flags)!
                .CreateDelegate<Action<RoomEnemyProjectileSlot, ushort, Position16<RoomBlocks>>>(enemies);
            RoomEnemyProjectileSlot smoke = enemies.EnemyProjectiles[0];
            initialize(smoke, (ushort)((index << 8) | 10), new(7, 9));
            AssertEqual(MiscDustProjectileDefinitions.InstructionList(10), smoke.InstructionPointer,
                $"eye-door smoke production instruction {index}");
            AssertEqual(unchecked((ushort)(7 * 16 + 8 + definition.Base.X +
                (random & definition.RandomMask.X))), smoke.Position.X.Whole,
                $"eye-door smoke production X {index}");
            AssertEqual(unchecked((ushort)(9 * 16 + 8 + definition.Base.Y +
                ((random >> 8) & definition.RandomMask.Y))), smoke.Position.Y.Whole,
                $"eye-door smoke production Y {index}");
            AssertEqual(1, advances, $"eye-door smoke RNG advance {index}");
        }

        var ridley = CreateMiscDustEnemySystem(rom);
        var spawnRidley = typeof(RoomEnemySystem).GetMethod("SpawnRidleyDust", flags)!
            .CreateDelegate<Action<Position16<RoomPixels>, ushort>>(ridley);
        spawnRidley(new(0x1234, 0x5678), ushort.MaxValue);
        AssertEqual(MiscDustProjectileDefinitions.InstructionList(29),
            ridley.EnemyProjectiles.Single(slot => slot.IsActive).InstructionPointer,
            "Ridley production dust clamps oversized variants to native selector 29");

        var invalidRoom = CreateMiscDustEnemySystem(rom);
        var spawnInvalidRoom = typeof(RoomEnemySystem).GetMethod(
                "SpawnRoomGraphicsDustExplosion", flags)!
            .CreateDelegate<Action<Position16<RoomPixels>, ushort>>(invalidRoom);
        AssertThrows<ArgumentOutOfRangeException>(
            () => spawnInvalidRoom(Position16<RoomPixels>.Zero, 30),
            "room-graphics selector beyond authored table");
        AssertEqual(0, invalidRoom.ActiveEnemyProjectileCount,
            "invalid room-graphics misc dust does not partially allocate a slot");

        AssertThrows<InvalidDataException>(
            () => MiscDustProjectileDefinitions.SmokePlacement(5),
            "misc-dust placement beyond authored table");
        Console.WriteLine(
            "Misc dust definitions: thirty selectors, five placement records, and Mother Brain, room-graphics, Eye Door, and Ridley production consumers pass with both ROM tables forbidden.");
    }

    private static RoomEnemySystem CreateMiscDustEnemySystem(SuperMetroidAddressSpace source)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!
            .SetValue(enemies, NewMiscDustDefinitionReadGuard(source));
        return enemies;
    }

    private static CartridgeReadGuard NewMiscDustDefinitionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x86e42c and < 0x86e468 or >= 0x86e47e and < 0x86e4a6,
                address => $"Misc dust attempted migrated definition read ${address:X6}.");
}
