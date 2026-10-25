using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.FuneNamihe.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.FuneNamihe;

/// <summary>#1291: Fune's and Namihe's fireballs run their typed programs and resolve their selectors through its frames.</summary>
internal static class FuneNamiheFireballBehaviorVerification
{
    /// <summary>
    /// Both kinds in both directions start the facing's typed program, which loops update for update beside the native
    /// list; every pose's calculated selector is its native spritemap.
    /// </summary>
    internal static void VerifyFuneNamiheFireballBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, FuneNamiheFireballNativeListAddresses.Extent, "Fune/Namihe fireball");
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot source = enemies.SlotTable.Slots[0];
        source.Position = source.Position.WithWhole(new(288, 128));
        source.SetSpawnParameter2(0);
        foreach (RoomEnemyProjectileKind kind in new[] { RoomEnemyProjectileKind.FuneFireball, RoomEnemyProjectileKind.NamiheFireball })
        {
            source.SetSpawnParameter1(kind == RoomEnemyProjectileKind.FuneFireball ? (ushort)0 : (ushort)1);
            foreach (bool movingRight in new[] { false, true })
            {
                foreach (RoomEnemyProjectileSlot candidate in enemies.Projectiles.EnemyProjectiles)
                    candidate.Clear();
                enemies.FuneNamiheBehavior.SpawnFuneNamiheFireball(source, movingRight ? HorizontalDirection.Right : HorizontalDirection.Left, kind);
                RoomEnemyProjectileSlot fireball = enemies.Projectiles.EnemyProjectiles.Single(candidate => candidate.Kind == kind);
                FuneNamiheFireballProgram program = movingRight ? FuneNamiheFireballProgram.Right : FuneNamiheFireballProgram.Left;
                AssertTrue(fireball.Animation is FuneNamiheFireballAnimation animation && animation.Cursor.Program == program,
                    $"the {kind} fireball moving {(movingRight ? "right" : "left")} starts {program}");
                (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, fireball,
                    NativeEnemyProjectileProgramOracle.EntryOf(FuneNamiheFireballNativeListAddresses.Entries, program), 30, $"{kind} {program}");
                AssertTrue(!deleted, $"{program} loops");
            }
        }
        for (int pose = 0; pose < FuneNamiheFireballFrameToolingDefinitions.PoseCount; pose++)
        {
            FuneNamiheFireballFrame frame = new(pose);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet((byte)EnemyInstructionBank.Bank86, EnemyProjectileProgramFrameSources.Operand(FuneNamiheFireballFrameDefinitions.ShownProjectileFrames[frame]), out ushort selected),
                $"pose {pose} has a calculated selector");
            AssertEqual(FuneNamiheFireballFrameToolingDefinitions.Spritemap(frame), selected, $"pose {pose}'s calculated selector is its native spritemap");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the fireball's native list bytes");
        Console.WriteLine("  Fune/Namihe fireball behavior: both kinds and directions loop as their native lists do and every selector matches.");
    }

    /// <summary>Runs the Fune/Namihe fireball's program oracle and behavior suite.</summary>
    [VerificationCommand("--fune-namihe-fireball-programs")]
    internal static int FuneNamiheFireballProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(FuneNamiheFireballAnimationsVerification.VerifyFuneNamiheFireballAnimations), () => FuneNamiheFireballAnimationsVerification.VerifyFuneNamiheFireballAnimations(rom));
        Suite(nameof(VerifyFuneNamiheFireballBehavior), () => VerifyFuneNamiheFireballBehavior(rom));
        return 0;
    }
}
