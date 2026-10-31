using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>#1291: the Wrecked Ship Chozo's dust and the Tourian statue's descent dust run typed programs.</summary>
internal static class ChozoTourianDustBehaviorVerification
{
    /// <summary>
    /// Each dust kind's definition starts its typed program, which matches the native list update for update until it
    /// deletes the dust; the descent dust returns to its origin every loop.
    /// </summary>
    internal static void VerifyChozoTourianDustBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, ChozoTourianDustNativeListAddresses.Extent, "Chozo/Tourian dust");
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        // The scatter draws again while a candidate falls below its center, so the fixture's random sequence advances.
        ushort random = 0;
        enemies.VerificationServices().NextRandom = () => random = unchecked((ushort)(random + 4951));
        foreach ((RoomEnemyProjectileKind kind, ChozoTourianDustProgram program) in new VerifyChozoTourianDustBehaviorCase[]
                 {
                     new VerifyChozoTourianDustBehaviorCase(RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep, ChozoTourianDustProgram.Footsteps),
                     new VerifyChozoTourianDustBehaviorCase(RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate, ChozoTourianDustProgram.SpikeClearingExplosions),
                     new VerifyChozoTourianDustBehaviorCase(RoomEnemyProjectileKind.TourianStatueDescentDust, ChozoTourianDustProgram.TourianDescentDust),
                 })
        {
            RoomEnemyProjectileSlot dust = enemies.Projectiles.EnemyProjectiles[0];
            dust.Clear();
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(dust, kind, SuperMetroid.Core.Assets.SpriteGraphics.Emptied);
            dust.State = new AnchoredProjectileState(new(128, 96));
            AssertTrue(dust.Animation is ChozoTourianDustAnimation animation && animation.Cursor.Program == program, $"{kind} starts {program}");
            (bool deleted, int updates) = NativeEnemyProjectileParity.Compare(cartridge, enemies, dust, ChozoTourianDustNativeListAddresses.Entry(program), 600, kind.ToString());
            AssertTrue(deleted, $"{kind} deletes itself after its program, at update {updates}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the dust's native list bytes");
        Console.WriteLine("  Chozo/Tourian dust behavior: all three dust kinds run their programs to deletion as their native lists do.");
    }

    /// <summary>Runs the dust's program oracle and behavior suite.</summary>
    [VerificationCommand("--chozo-tourian-dust-programs")]
    internal static int ChozoTourianDustProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(ChozoTourianDustAnimationsVerification.VerifyChozoTourianDustAnimations), () => ChozoTourianDustAnimationsVerification.VerifyChozoTourianDustAnimations(rom));
        Suite(nameof(VerifyChozoTourianDustBehavior), () => VerifyChozoTourianDustBehavior(rom));
        return 0;
    }
}
