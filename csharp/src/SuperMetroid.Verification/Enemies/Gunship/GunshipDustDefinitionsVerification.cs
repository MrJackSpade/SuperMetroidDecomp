using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Gunship;

/// <summary>Checks compiled gunship liftoff dust data and real projectile allocation against cartridge table values.</summary>
internal static class GunshipDustDefinitionsVerification
{
    /// <summary>
    /// Compares all six dust definitions and their spawn positions for Samus anywhere a cloud stays inside the room space
    /// (#2341: the clouds are placed by plain arithmetic), then checks full-pool behavior.
    /// </summary>
    internal static void VerifyCompiledGunshipDust(CartridgeImportAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(CompiledGunshipDustRomByte(rom, address), CompiledGunshipDustRomByte(rom, address + 1));
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var spawn = new Action<GunshipDustPlacement, SamusState>((placement, spawnSamus) => GunshipLiftoffDust.SpawnCloud(enemies.Projectiles, placement, spawnSamus));
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        var slot = enemies.Projectiles.ProjectileInPlace(17);
        AssertEqual(6, GunshipDustDefinitions.LiftoffClouds.Count, "Six liftoff dust clouds");
        // Samus at least 128 pixels from either end of the room space on both axes keeps every cloud inside it.
        const int margin = 128;
        int spawns = 0;
        for (int cloud = 0; cloud < GunshipDustDefinitions.LiftoffClouds.Count; cloud++)
        {
            // The gunship's six spawn parameters are the native tables' byte offsets, in the clouds' spawn order.
            int parameter = 2 * cloud;
            short dx = unchecked((short)Word(EnemyRomTablePointersGunship.DustXOffsetWords + parameter));
            ushort instruction = Word(EnemyRomTablePointersGunship.DustInstructionPointers + parameter);
            var definition = GunshipDustDefinitions.LiftoffClouds[cloud];
            AssertEqual(dx, definition.XOffset, "All native dust signed X offsets");
            AssertEqual(instruction, GunshipDustNativeListAddresses.Entry(definition.Animation), "All native dust instruction selections");
            for (int origin = margin; origin <= ushort.MaxValue - margin; origin++)
            {
                slot.Delete();
                // Every such world X and Y enters the actual allocator/header initializer with stale slot values.
                samus.Position = samus.Position.WithWhole(new((ushort)origin, (ushort)(ushort.MaxValue - origin)));
                slot.Position = slot.Position.WithFractionY(0xffff).WithFractionX(0xffff);
                slot.Velocity = new(new SignedEightEight(0x1234), new SignedEightEight(0x1234));
                spawn(definition, samus);
                // A spawn creates a new projectile in the place, which the place now holds (#1590).
                slot = enemies.Projectiles.ProjectileInPlace(17);
                AssertEqual(RoomEnemyProjectileKind.GunshipLiftoffDustCloud, slot.Kind, "Native reverse allocation chooses last free slot");
                AssertEqual(samus.Position.X.Whole + dx, (int)slot.Position.X.Whole, "Actual dust signed X placement");
                AssertEqual(samus.Position.Y.Whole + Word(0x86a2c5), (int)slot.Position.Y.Whole, "Actual dust Y placement");
                spawns++;
                AssertTrue(slot.Animation is GunshipDustAnimation started &&
                    started.Playback == new AnimationPlayback<GunshipDustSegment>(GunshipDustAnimations.StartingWith(definition.Animation), AnimationPlace.StartOfAnimation), "Actual dust program selection");
                AssertEqual((ushort)1, slot.FrameHold.RawValue, "Dust begins on first program tick");
                AssertEqual((ushort)0, slot.Position.X.Fraction, "Dust clears stale X fraction");
                AssertEqual((ushort)0, slot.Position.Y.Fraction, "Dust clears stale Y fraction");
                AssertEqual((ushort)0, slot.Velocity.X.RawValue, "Dust clears stale X velocity");
                AssertEqual((ushort)0, slot.Velocity.Y.RawValue, "Dust clears stale Y velocity");
            }
        }
        foreach (var occupied in enemies.Projectiles.EveryPlace())
        {
            global::SuperMetroid.Core.PrivateState.Set(occupied, "Kind", (global::SuperMetroid.Core.Enemies.RoomEnemyProjectileKind)(RoomEnemyProjectileKind.PhantoonStartingFlame));
            occupied.Position = occupied.Position.WithWholeX(0x1234);
            occupied.StartAnimation(EnemyProjectileDeleteAnimation.Instance);
        }
        foreach (GunshipDustPlacement cloud in GunshipDustDefinitions.LiftoffClouds) spawn(cloud, samus);
        foreach (var occupied in enemies.Projectiles.EveryPlace())
        {
            AssertEqual(RoomEnemyProjectileKind.PhantoonStartingFlame, occupied.Kind, "Full pool retains owner");
            AssertEqual((ushort)0x1234, occupied.Position.X.Whole, "Full pool retains X");
            AssertTrue(ReferenceEquals(EnemyProjectileDeleteAnimation.Instance, occupied.Animation), "Full pool retains program");
        }
        Console.WriteLine($"Gunship dust: 12 native words, {spawns} real spawns and full-pool retention pass with migrated reads forbidden.");
    }

    /// <summary>Reads a native dust-table byte solely as the verification oracle at the CPU bus boundary.</summary>
    private static byte CompiledGunshipDustRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

}
