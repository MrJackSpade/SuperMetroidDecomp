using SuperMetroid.Verification.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Projectiles;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks misc-dust instruction and smoke-placement tables through their production projectile consumers.</summary>
internal static class MiscDustProjectileDefinitionsVerification
{
    /// <summary>Compares thirty selectors and five placement records, including RNG advancement and invalid allocation behavior.</summary>
    internal static void VerifyMiscDustProjectileDefinitions(SuperMetroidAddressSpace rom)
    {
        const int instructionTable = 0x86e42c;
        const int placementTable = 0x86e47e;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (ushort index = 0; index < 30; index++)
        {
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord((instructionTable + index * 2));
            MiscDustProjectileProgram program = MiscDustProjectileDefinitions.Program((MiscDustAnimation)index);
            AssertEqual(native, MiscDustProjectileNativeListAddresses.Entry(program), $"misc-dust instruction selector {index}");

            var enemies = CreateMiscDustEnemySystem();
            var spawn = new Action<Position16<RoomPixels>, MiscDustAnimation>(enemies.Projectiles.SpawnRoomGraphicsDustExplosion);
            spawn(new(0x1234, 0x5678), (MiscDustAnimation)index);
            RoomEnemyProjectileSlot roomSlot = enemies.Projectiles.EnemyProjectiles.Single(slot => slot.IsActive);
            AssertTrue(roomSlot.Animation is MiscDustProjectileAnimation started && started.Cursor.Program == program,
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
            var enemies = CreateMiscDustEnemySystem();
            enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => random);
            enemies.VerificationServices().NextRandom = (Func<ushort>)(() => { advances++; return 0xbeef; });
            Action<RoomEnemyProjectileSlot, ushort, Position16<RoomBlocks>> initialize = enemies.EyeDoorBehavior.InitializeEyeDoorSmoke;
            RoomEnemyProjectileSlot smoke = enemies.Projectiles.EnemyProjectiles[0];
            initialize(smoke, (ushort)((index << 8) | 10), new(7, 9));
            AssertTrue(smoke.Animation is MiscDustProjectileAnimation { Cursor.Program: MiscDustProjectileProgram.CorpseDustCloud },
                $"eye-door smoke production instruction {index}");
            AssertEqual(unchecked((ushort)(7 * 16 + 8 + definition.Base.X +
                (random & definition.RandomMask.X))), smoke.Position.X.Whole,
                $"eye-door smoke production X {index}");
            AssertEqual(unchecked((ushort)(9 * 16 + 8 + definition.Base.Y +
                ((random >> 8) & definition.RandomMask.Y))), smoke.Position.Y.Whole,
                $"eye-door smoke production Y {index}");
            AssertEqual(1, advances, $"eye-door smoke RNG advance {index}");
        }

        var ridley = CreateMiscDustEnemySystem();
        Action<Position16<RoomPixels>, ushort> spawnRidley = ridley.RidleyBehavior.SpawnRidleyDust;
        spawnRidley(new(0x1234, 0x5678), ushort.MaxValue);
        AssertTrue(ridley.Projectiles.EnemyProjectiles.Single(slot => slot.IsActive).Animation is
                MiscDustProjectileAnimation { Cursor.Program: MiscDustProjectileProgram.BigExplosion },
            "Ridley production dust clamps oversized variants to native selector 29");

        var invalidRoom = CreateMiscDustEnemySystem();
        var spawnInvalidRoom = new Action<Position16<RoomPixels>, MiscDustAnimation>(invalidRoom.Projectiles.SpawnRoomGraphicsDustExplosion);
        AssertThrows<ArgumentOutOfRangeException>(
            () => spawnInvalidRoom(Position16<RoomPixels>.Zero, (MiscDustAnimation)30),
            "room-graphics selector beyond authored table");
        AssertEqual(0, invalidRoom.ActiveEnemyProjectileCount,
            "invalid room-graphics misc dust does not partially allocate a slot");

        AssertThrows<InvalidDataException>(
            () => MiscDustProjectileDefinitions.SmokePlacement(5),
            "misc-dust placement beyond authored table");
        Console.WriteLine(
            "Misc dust definitions: thirty selectors, five placement records, and Mother Brain, room-graphics, Eye Door, and Ridley production consumers pass with both ROM tables forbidden.");
    }

    /// <summary>Builds an enemy fixture that forbids runtime access to compiled misc-dust tables.</summary>
    private static RoomEnemySystem CreateMiscDustEnemySystem()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        return enemies;
    }

}
