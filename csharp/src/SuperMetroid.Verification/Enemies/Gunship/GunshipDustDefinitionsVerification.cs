using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Gunship;

/// <summary>Checks compiled gunship liftoff dust data and real projectile allocation against cartridge table values.</summary>
internal static class GunshipDustDefinitionsVerification
{
    /// <summary>Compares all six dust definitions and wrapped spawn positions, then checks full-pool behavior.</summary>
    internal static void VerifyCompiledGunshipDust(CartridgeImportAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(CompiledGunshipDustRomByte(rom, address), CompiledGunshipDustRomByte(rom, address + 1));
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var spawn = new Action<GunshipDustPlacement, SamusState>(enemies.Projectiles.SpawnGunshipLiftoffDustCloud);
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
        var slot = enemies.Projectiles.EnemyProjectiles[^1];
        AssertEqual(6, GunshipDustDefinitions.LiftoffClouds.Count, "Six liftoff dust clouds");
        for (int cloud = 0; cloud < GunshipDustDefinitions.LiftoffClouds.Count; cloud++)
        {
            // The gunship's six spawn parameters are the native tables' byte offsets, in the clouds' spawn order.
            int parameter = 2 * cloud;
            short dx = unchecked((short)Word(EnemyRomTablePointersGunship.DustXOffsetWords + parameter));
            ushort instruction = Word(EnemyRomTablePointersGunship.DustInstructionPointers + parameter);
            var definition = GunshipDustDefinitions.LiftoffClouds[cloud];
            AssertEqual(dx, definition.XOffset, "All native dust signed X offsets");
            AssertEqual(instruction, GunshipDustNativeListAddresses.Entry(definition.Animation), "All native dust instruction selections");
            for (int origin = 0; origin <= ushort.MaxValue; origin++)
            {
                slot.Delete();
                // Every world X and Y, including independent positive/negative wrap,
                // enters the actual allocator/header initializer with stale slot values.
                samus.Position = samus.Position.WithWhole(new((ushort)origin, (ushort)(ushort.MaxValue - origin)));
                slot.Position = slot.Position.WithFractionY(0xffff).WithFractionX(0xffff);
                slot.Velocity = new(new SignedEightEight(0x1234), new SignedEightEight(0x1234));
                spawn(definition, samus);
                // A spawn creates a new projectile in the place, which the place now holds (#1590).
                slot = enemies.Projectiles.EnemyProjectiles[^1];
                AssertEqual(RoomEnemyProjectileKind.GunshipLiftoffDustCloud, slot.Kind, "Native reverse allocation chooses last free slot");
                AssertEqual(unchecked((ushort)(samus.Position.X.Whole + dx)), slot.Position.X.Whole, "Actual dust signed X placement and wrap");
                AssertEqual(unchecked((ushort)(samus.Position.Y.Whole + Word(0x86a2c5))), slot.Position.Y.Whole, "Actual dust Y placement and wrap");
                AssertTrue(slot.Animation is GunshipDustAnimation started &&
                    started.Playback == new AnimationPlayback<GunshipDustSegment>(GunshipDustAnimations.StartingWith(definition.Animation), AnimationPlace.StartOfAnimation), "Actual dust program selection");
                AssertEqual((ushort)1, slot.FrameHold.RawValue, "Dust begins on first program tick");
                AssertEqual((ushort)0, slot.Position.X.Fraction, "Dust clears stale X fraction");
                AssertEqual((ushort)0, slot.Position.Y.Fraction, "Dust clears stale Y fraction");
                AssertEqual((ushort)0, slot.Velocity.X.RawValue, "Dust clears stale X velocity");
                AssertEqual((ushort)0, slot.Velocity.Y.RawValue, "Dust clears stale Y velocity");
            }
        }
        foreach (var occupied in enemies.Projectiles.EnemyProjectiles)
        {
            occupied.Kind = RoomEnemyProjectileKind.PhantoonStartingFlame;
            occupied.Position = occupied.Position.WithWholeX(0x1234);
            occupied.StartAnimation(EnemyProjectileDeleteAnimation.Instance);
        }
        foreach (GunshipDustPlacement cloud in GunshipDustDefinitions.LiftoffClouds) spawn(cloud, samus);
        foreach (var occupied in enemies.Projectiles.EnemyProjectiles)
        {
            AssertEqual(RoomEnemyProjectileKind.PhantoonStartingFlame, occupied.Kind, "Full pool retains owner");
            AssertEqual((ushort)0x1234, occupied.Position.X.Whole, "Full pool retains X");
            AssertTrue(ReferenceEquals(EnemyProjectileDeleteAnimation.Instance, occupied.Animation), "Full pool retains program");
        }
        Console.WriteLine("Gunship dust: 12 native words, 393216 real wrapped spawns and full-pool retention pass with migrated reads forbidden.");
    }

    /// <summary>Reads a native dust-table byte solely as the verification oracle at the CPU bus boundary.</summary>
    private static byte CompiledGunshipDustRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

}
