using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Nuclear;

/// <summary>#1292: the Nuclear Waffle's body links run their typed loop.</summary>
internal static class NuclearWaffleProjectileBehaviorVerification
{
    /// <summary>The initializer's four body links start the typed loop, which matches the native list update for update.</summary>
    internal static void VerifyNuclearWaffleProjectileBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, NuclearWaffleProjectileNativeListAddresses.Extent, "Nuclear Waffle body");
        var enemies = new RoomEnemySystem(new EnemySession());
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot owner = enemies.SlotTable.Slots[0];
        owner.Bind(EnemyDefinitionId.Puromi, owner.Definition);
        owner.SetSpawnParameter1(8193);
        owner.SetSpawnParameter2(8192);
        owner.Position = owner.Position.WithWhole(new(384, 512));
        enemies.NuclearWaffleBehavior.InitializeNuclearWaffle(owner);
        RoomEnemyProjectileSlot[] links = [.. (enemies.NuclearWaffleStates[0] ?? throw new InvalidOperationException("Verification requires the Nuclear Waffle's state."))
            .ProjectileSegments.OfType<RoomEnemyProjectileSlot>()];
        AssertEqual(4, links.Length, "the initializer spawns four body links");
        foreach (RoomEnemyProjectileSlot link in links)
        {
            AssertTrue(link.Animation is NuclearWaffleProjectileAnimation { Cursor.Program: NuclearWaffleProjectileProgram.Initial }, "a body link starts its loop");
            (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, link,
                NuclearWaffleProjectileNativeListAddresses.Entry(NuclearWaffleProjectileProgram.Initial), 120, "Nuclear Waffle body link");
            AssertTrue(!deleted, "a body link loops");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the body's native list bytes");
        Console.WriteLine("  Nuclear Waffle body behavior: all four links loop as the native list does.");
    }

    /// <summary>Runs the Nuclear Waffle body's program oracle and behavior suite.</summary>
    [VerificationCommand("--nuclear-waffle-projectile-programs")]
    internal static int NuclearWaffleProjectileProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(NuclearWaffleProjectileAnimationsVerification.VerifyNuclearWaffleProjectileAnimations), () => NuclearWaffleProjectileAnimationsVerification.VerifyNuclearWaffleProjectileAnimations(rom));
        Suite(nameof(VerifyNuclearWaffleProjectileBehavior), () => VerifyNuclearWaffleProjectileBehavior(rom));
        return 0;
    }
}
