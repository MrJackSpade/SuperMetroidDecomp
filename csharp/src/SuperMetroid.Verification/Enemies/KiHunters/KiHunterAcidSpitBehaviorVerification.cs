using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Enemies.KiHunters;

/// <summary>#1291: the Ki-Hunter's acid spit runs its typed launch and splash programs as its native lists did.</summary>
internal static class KiHunterAcidSpitBehaviorVerification
{
    /// <summary>
    /// Each facing's spit starts its launch program and matches the native list update for update, including the
    /// movement routine it installs and its terminal sleep; a floor collision starts the splash, which clears the
    /// movement and deletes the spit on the native update.
    /// </summary>
    internal static void VerifyKiHunterAcidSpitBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, KiHunterAcidSpitNativeListAddresses.Extent, "Ki-Hunter acid");
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance);
        var body = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112)),
            TileIndexBase = 512,
            Colouring = SharedSpriteColouring.Frozen,
        };
        enemies.KiHunterBehavior.SpawnKiHunterAcidSpit(body, false);
        enemies.KiHunterBehavior.SpawnKiHunterAcidSpit(body, true);
        RoomEnemyProjectileSlot left = Find(RoomEnemyProjectileKind.KiHunterAcidSpitLeft);
        RoomEnemyProjectileSlot right = Find(RoomEnemyProjectileKind.KiHunterAcidSpitRight);
        foreach ((RoomEnemyProjectileSlot spit, KiHunterAcidSpitProgram program) in new[] { new VerifyKiHunterAcidSpitBehaviorCase(left, KiHunterAcidSpitProgram.Left), new VerifyKiHunterAcidSpitBehaviorCase(right, KiHunterAcidSpitProgram.Right) })
        {
            AssertEqual(program, ProgramOf(spit), $"the {spit.Kind} spit starts its launch program");
            (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, spit, Entry(program), 60, $"{spit.Kind} launch");
            AssertTrue(!deleted, $"the {spit.Kind} launch ends in its sleep");
        }

        RoomLevelData level = VerificationHelpersVerification.CreateSolidRowsRoom(new(8, 8), firstSolidRow: 4, solidRowCount: 4);
        left.Position = left.Position.WithWhole(new(64, 63));
        left.Radius = new(2, 2);
        left.Velocity = left.Velocity.WithY(SignedEightEight.FromSignedRaw(256));
        KiHunterBehavior.RunKiHunterAcidMovement(left, level);
        AssertEqual(KiHunterAcidSpitProgram.HitFloor, ProgramOf(left), "a floor collision starts the splash program");
        (bool splashed, int updates) = NativeEnemyProjectileParity.Compare(cartridge, enemies, left, Entry(KiHunterAcidSpitProgram.HitFloor), 60, "acid splash");
        AssertTrue(splashed, "the splash deletes the spit");
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the acid's native list bytes");
        Console.WriteLine($"  Ki-Hunter acid behavior: both launches and the {updates}-update splash match their native lists.");

        RoomEnemyProjectileSlot Find(RoomEnemyProjectileKind kind) =>
            enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind == kind);
    }

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    private static ushort Entry(KiHunterAcidSpitProgram program) =>
        NativeEnemyProjectileProgramOracle.EntryOf(KiHunterAcidSpitNativeListAddresses.Entries, program);

    /// <summary>The typed program <paramref name="projectile"/> runs.</summary>
    private static KiHunterAcidSpitProgram ProgramOf(RoomEnemyProjectileSlot projectile) =>
        projectile.Animation is KiHunterAcidSpitAnimation animation ? animation.Cursor.Program
            : throw new InvalidOperationException("The spit runs no typed program.");

    /// <summary>Runs the Ki-Hunter acid's program oracle and behavior suite.</summary>
    [VerificationCommand("--kihunter-acid-spit-programs")]
    internal static int KiHunterAcidSpitProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(KiHunterAcidSpitAnimationsVerification.VerifyKiHunterAcidSpitAnimations), () => KiHunterAcidSpitAnimationsVerification.VerifyKiHunterAcidSpitAnimations(rom));
        Suite(nameof(VerifyKiHunterAcidSpitBehavior), () => VerifyKiHunterAcidSpitBehavior(rom));
        return 0;
    }
}
