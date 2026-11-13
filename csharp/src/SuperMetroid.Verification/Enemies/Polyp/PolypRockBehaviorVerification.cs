using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.Polyp.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.Polyp;

/// <summary>#1291: the Polyp's lava rock runs its typed single-frame program and resolves its selector through its frame.</summary>
internal static class PolypRockBehaviorVerification
{
    /// <summary>
    /// A spawned rock starts its typed program, shows its frame and sleeps update for update beside the native list; the
    /// shared shot list still deletes it; its selector is its native spritemap.
    /// </summary>
    internal static void VerifyPolypRockBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, PolypRockNativeListAddresses.Extent, "Polyp-rock");
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var source = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112)),
            Shading = SuperMetroid.Core.Enemies.Drawing.EnemyShades.FrozenLook,
        };
        enemies.PolypBehavior.SpawnPolypRock(source, 16, SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-256));
        EnemyProjectile rock = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind == RoomEnemyProjectileKind.PolypRock);
        AssertTrue(rock.Animation is PolypRockAnimation { PlayingSegment: PolypRockProgram.Initial }, "a spawned rock starts its typed program");
        (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, rock,
            NativeEnemyProjectileProgramOracle.EntryOf(PolypRockNativeListAddresses.Entries, PolypRockProgram.Initial), 5, "Polyp rock");
        AssertTrue(!deleted, "the rock holds its frame and sleeps");
        rock.StartAnimation(EnemyProjectileDeleteAnimation.Instance);
        enemies.Projectiles.ProcessEnemyProjectileInstructions(rock, null, Position16<RoomPixels>.Zero);
        AssertTrue(!rock.IsActive, "the shared shot list still deletes the rock");
        AssertTrue(CompiledEnemyVisualSelectors.TryGet((byte)EnemyInstructionBank.Bank86, EnemyProjectileProgramFrameSources.Operand(EnemyProjectileFrameId.PolypRockInitial), out ushort selected),
            "the rock's frame has a selector");
        AssertEqual(PolypRockFrameToolingDefinitions.Spritemap(EnemyProjectileFrameId.PolypRockInitial), selected, "the rock's selector is its native spritemap");
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the rock's native list bytes");
        Console.WriteLine("  Polyp rock behavior: the rock shows its frame and sleeps as its native list does; its selector matches.");
    }

    /// <summary>Runs the Polyp rock's program oracle and behavior suite.</summary>
    [VerificationCommand("--polyp-rock-programs")]
    internal static int PolypRockProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(PolypRockAnimationsVerification.VerifyPolypRockAnimations), () => PolypRockAnimationsVerification.VerifyPolypRockAnimations(rom));
        Suite(nameof(VerifyPolypRockBehavior), () => VerifyPolypRockBehavior(rom));
        return 0;
    }
}
