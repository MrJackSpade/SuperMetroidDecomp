using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Projectiles;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks misc-dust instruction and smoke-placement tables through their production projectile consumers.</summary>
internal static class MiscDustProjectileDefinitionsVerification
{
    /// <summary>
    /// Compares the fourteen selectors a spawner asks for and five placement records, including RNG advancement and invalid
    /// allocation behavior; the cartridge table's sixteen other selectors no spawner asks for (#1686).
    /// </summary>
    internal static void VerifyMiscDustProjectileDefinitions(CartridgeImportAddressSpace rom)
    {
        const int instructionTable = 0x86e42c;
        const int placementTable = 0x86e47e;

        int unrequested = 0;
        for (ushort index = 0; index < MiscDustAnimationIndices.SelectorCount; index++)
        {
            if (MiscDustAnimationIndices.TryDecode(index) is not { } animation)
            {
                unrequested++;
                continue;
            }
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord((instructionTable + index * 2));
            MiscDustProjectileSegment program = MiscDustProjectiles.Animation(animation);
            AssertEqual(native, MiscDustProjectileNativeListAddresses.Entry(program), $"misc-dust instruction selector {index}");

            var enemies = CreateMiscDustEnemySystem();
            var spawn = new Action<Position16<RoomPixels>, MiscDustAnimation>(enemies.Projectiles.SpawnRoomGraphicsDustExplosion);
            spawn(new(0x1234, 0x5678), animation);
            EnemyProjectile roomSlot = enemies.Projectiles.EnemyProjectiles.Single(slot => slot.IsActive);
            AssertTrue(roomSlot.Animation is MiscDustProjectileAnimation started && started.PlayingSegment == program,
                $"room-graphics misc-dust production selector {index}");
        }
        AssertEqual(16, unrequested, "misc-dust selectors no spawner asks for");
        AssertEqual(Enum.GetValues<MiscDustAnimation>().Length, MiscDustAnimationIndices.SelectorCount - unrequested,
            "every misc-dust animation has its selector");

        for (ushort index = 0; index < 5; index++)
        {
            int address = placementTable + index * 8;
            MiscDustPlacementDefinition definition =
                MiscDustProjectiles.SmokePlacement(index);
            // The native masks apply to the sample's low (X) and high (Y) bytes; each field reads exactly those bits.
            int nativeXMask = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            int nativeYMask = CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 2));
            AssertEqual(nativeXMask, (definition.XScatter.Outcomes - 1) << definition.XScatter.Shift,
                $"misc-dust placement X mask {index}");
            AssertEqual(nativeYMask << 8, (definition.YScatter.Outcomes - 1) << definition.YScatter.Shift,
                $"misc-dust placement Y mask {index}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 4))), definition.Base.X,
                $"misc-dust placement X base {index}");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 6))), definition.Base.Y,
                $"misc-dust placement Y base {index}");

            const ushort random = 0x5aad;
            int advances = 0;
            var enemies = CreateMiscDustEnemySystem();
            enemies.VerificationServices().ReadRandom = () => random;
            enemies.VerificationServices().NextRandom = () => { advances++; return 0xbeef; };
            Action<EnemyProjectile, MiscDustAnimation, int, Position16<RoomBlocks>> initialize = enemies.EyeDoorBehavior.InitializeEyeDoorSmoke;
            EnemyProjectile smoke = enemies.Projectiles.EnemyProjectiles[0];
            initialize(smoke, MiscDustAnimation.CorpseDustCloud, index, new(7, 9));
            AssertTrue(smoke.Animation is MiscDustProjectileAnimation { PlayingSegment: MiscDustProjectileSegment.CorpseDustCloud },
                $"eye-door smoke production instruction {index}");
            AssertEqual(unchecked((ushort)(7 * 16 + 8 + definition.Base.X +
                (random & nativeXMask))), smoke.Position.X.Whole,
                $"eye-door smoke production X {index}");
            AssertEqual(unchecked((ushort)(9 * 16 + 8 + definition.Base.Y +
                ((random >> 8) & nativeYMask))), smoke.Position.Y.Whole,
                $"eye-door smoke production Y {index}");
            AssertEqual(1, advances, $"eye-door smoke RNG advance {index}");
        }

        var ridley = CreateMiscDustEnemySystem();
        Action<Position16<RoomPixels>, MiscDustAnimation> spawnRidley = ridley.RidleyProjectiles.SpawnRidleyDust;
        spawnRidley(new(0x1234, 0x5678), MiscDustAnimation.BigExplosion);
        AssertTrue(ridley.Projectiles.EnemyProjectiles.Single(slot => slot.IsActive).Animation is
                MiscDustProjectileAnimation { PlayingSegment: MiscDustProjectileSegment.BigExplosion },
            "Ridley production dust starts the requested animation's program");

        var invalidRoom = CreateMiscDustEnemySystem();
        var spawnInvalidRoom = new Action<Position16<RoomPixels>, MiscDustAnimation>(invalidRoom.Projectiles.SpawnRoomGraphicsDustExplosion);
        AssertThrows<ArgumentOutOfRangeException>(
            () => spawnInvalidRoom(Position16<RoomPixels>.Zero, (MiscDustAnimation)byte.MaxValue),
            "room-graphics selector beyond authored table");
        AssertEqual(0, invalidRoom.ActiveEnemyProjectileCount,
            "invalid room-graphics misc dust does not partially allocate a slot");

        AssertThrows<InvalidDataException>(
            () => MiscDustProjectiles.SmokePlacement(5),
            "misc-dust placement beyond authored table");
        Console.WriteLine(
            "Misc dust definitions: fourteen selectors, five placement records, and Mother Brain, room-graphics, Eye Door, and Ridley production consumers pass with both ROM tables forbidden.");
    }

    /// <summary>Builds an enemy fixture that forbids runtime access to compiled misc-dust tables.</summary>
    private static RoomEnemySystem CreateMiscDustEnemySystem()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        return enemies;
    }

}
