using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Projectiles;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks misc-dust instruction and smoke-placement tables through their production projectile consumers.</summary>
internal static class DustExplosionProjectileDefinitionsVerification
{
    /// <summary>
    /// Compares the fourteen selectors a spawner asks for and five placement records, including RNG advancement and invalid
    /// allocation behavior; the cartridge table's sixteen other selectors no spawner asks for (#1686).
    /// </summary>
    internal static void VerifyDustExplosionProjectileDefinitions(CartridgeImportAddressSpace rom)
    {
        const int instructionTable = 0x86e42c;
        const int placementTable = 0x86e47e;

        int unrequested = 0;
        for (ushort index = 0; index < DustExplosionAnimationIndices.SelectorCount; index++)
        {
            if (DustExplosionAnimationIndices.TryDecode(index) is not { } animation)
            {
                unrequested++;
                continue;
            }
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord((instructionTable + index * 2));
            AssertEqual(native, DustExplosionProjectileNativeListAddresses.Entry(animation), $"misc-dust instruction selector {index}");

            var enemies = CreateDustExplosionEnemySystem();
            var spawn = new Action<Position16<RoomPixels>, DustExplosionAnimation>(enemies.Projectiles.SpawnDustOrExplosion);
            spawn(new(0x1234, 0x5678), animation);
            EnemyProjectile roomSlot = enemies.Projectiles.EnemyProjectiles.Single(slot => slot.IsActive);
            AssertTrue(roomSlot.Animation is DustExplosionProjectileAnimation started && started.PlayingSegment == animation,
                $"room-graphics misc-dust production selector {index}");
        }
        AssertEqual(16, unrequested, "misc-dust selectors no spawner asks for");
        AssertEqual(Enum.GetValues<DustExplosionAnimation>().Length, DustExplosionAnimationIndices.SelectorCount - unrequested,
            "every misc-dust animation has its selector");

        // The cartridge's five placement records, in its order (#2044).
        DustExplosionPlacement[] placements =
        [
            SmokePuffPlacements.Point,
            SmokePuffPlacementsConstants.CentredEightPixelSquare,
            SmokePuffPlacementsConstants.CentredSixteenPixelSquare,
            SmokePuffPlacements.CentredBox,
            SmokePuffPlacementsConstants.CentredThirtyTwoBySixtyFourBox,
        ];
        for (int index = 0; index < placements.Length; index++)
        {
            int address = placementTable + index * 8;
            DustExplosionPlacement definition = placements[index];
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
        }

        // The eye door spawns its two smokes as the cartridge's room-object instructions do: each instruction's parameter
        // names the placement record (high byte) and the misc-dust selector (low byte) of the dust projectile it spawns.
        foreach ((EyeDoorProjectileDefinition smoke, int parameterAddress) in new[]
        {
            (EyeDoorProjectileDefinition.ScatteredSmoke, 0x84d7a1), // $84:D7A0 LDA #$030A, spawning the two scattered smokes.
            (EyeDoorProjectileDefinition.CenteredSmoke, 0x84d7b8), // $84:D7B7 LDA #$000B, spawning the smoke on the eye.
        })
        {
            ushort parameter = CartridgeImportSource.Require(rom).ReadCartridgeWord(parameterAddress);
            int address = placementTable + (parameter >> 8) * 8;
            int nativeXMask = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            int nativeYMask = CartridgeImportSource.Require(rom).ReadCartridgeWord(address + 2);
            short nativeXBase = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address + 4));
            short nativeYBase = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address + 6));
            DustExplosionAnimation animation = DustExplosionAnimationIndices.Decode(parameter & 0xff);

            const ushort random = 0x5aad;
            int advances = 0;
            var enemies = CreateDustExplosionEnemySystem();
            enemies.VerificationServices().ReadRandom = () => random;
            enemies.VerificationServices().NextRandom = () => { advances++; return 0xbeef; };
            enemies.EyeDoorBehavior.SpawnEyeDoorProjectile(new EyeDoorProjectileRequest(smoke, EyeDoorOrientation.Left, new(7, 9)));
            EnemyProjectile spawned = enemies.Projectiles.EnemyProjectiles.Single(slot => slot.IsActive);
            AssertTrue(spawned.Animation is DustExplosionProjectileAnimation started && started.PlayingSegment == animation,
                $"eye-door {smoke} production instruction");
            AssertEqual(unchecked((ushort)(7 * 16 + 8 + nativeXBase + (random & nativeXMask))), spawned.Position.X.Whole,
                $"eye-door {smoke} production X");
            AssertEqual(unchecked((ushort)(9 * 16 + 8 + nativeYBase + ((random >> 8) & nativeYMask))), spawned.Position.Y.Whole,
                $"eye-door {smoke} production Y");
            AssertEqual(1, advances, $"eye-door {smoke} RNG advance");
        }

        var ridley = CreateDustExplosionEnemySystem();
        Action<Position16<RoomPixels>, DustExplosionAnimation> spawnRidley = ridley.RidleyProjectiles.SpawnRidleyDust;
        spawnRidley(new(0x1234, 0x5678), DustExplosionAnimation.BigExplosion);
        AssertTrue(ridley.Projectiles.EnemyProjectiles.Single(slot => slot.IsActive).Animation is
                DustExplosionProjectileAnimation { PlayingSegment: DustExplosionAnimation.BigExplosion },
            "Ridley production dust starts the requested animation's program");

        var invalidRoom = CreateDustExplosionEnemySystem();
        var spawnInvalidRoom = new Action<Position16<RoomPixels>, DustExplosionAnimation>(invalidRoom.Projectiles.SpawnDustOrExplosion);
        AssertThrows<ArgumentOutOfRangeException>(
            () => spawnInvalidRoom(Position16<RoomPixels>.Zero, (DustExplosionAnimation)byte.MaxValue),
            "room-graphics selector beyond authored table");
        AssertEqual(0, invalidRoom.ActiveEnemyProjectileCount,
            "invalid room-graphics misc dust does not partially allocate a slot");

        Console.WriteLine(
            "Misc dust definitions: fourteen selectors, five placement records, and Mother Brain, room-graphics, Eye Door, and Ridley production consumers pass with both ROM tables forbidden.");
    }

    /// <summary>Builds an enemy fixture that forbids runtime access to compiled misc-dust tables.</summary>
    private static RoomEnemySystem CreateDustExplosionEnemySystem()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        return enemies;
    }

}
