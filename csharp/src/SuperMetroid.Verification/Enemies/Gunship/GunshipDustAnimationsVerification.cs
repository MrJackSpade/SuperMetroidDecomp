using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Enemies.Gunship.GunshipDustNativeListAddresses;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Enemies.Gunship.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Gunship;

/// <summary>
/// #1290: the gunship liftoff dust clouds' typed programs against the retail cartridge. Each spawn parameter's real
/// producer must start its program, which then matches the native bank-$86 list update by update until both delete the
/// cloud; every installed frame must be one the programs show.
/// </summary>
internal static class GunshipDustAnimationsVerification
{
    /// <summary>Every dust program matches its native list.</summary>
    internal static void VerifyGunshipDustAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int updates = 0;
        for (int cloud = 0; cloud < GunshipDustDefinitions.LiftoffClouds.Count; cloud++)
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            var guard = new MutableCartridgeReadGuard(rom);
            guard.Forbid(address => address is >= ListsStart and < ListsEnd,
                address => $"Production read a native gunship dust list at ${address:X6}.");
            enemies.Projectiles.SpawnGunshipLiftoffDustCloud(GunshipDustDefinitions.LiftoffClouds[cloud], new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()));
            EnemyProjectile dust = enemies.Projectiles.EnemyProjectiles.Single(item => item.Kind == RoomEnemyProjectileKind.GunshipLiftoffDustCloud);
            GunshipDustSegment program = GunshipDustDefinitions.LiftoffClouds[cloud].Animation;
            AssertTrue(dust.Animation is GunshipDustAnimation started && started.Playback == new AnimationPlayback<GunshipDustSegment>(GunshipDustAnimations.StartingWith(program), AnimationPlace.StartOfAnimation),
                $"dust cloud {cloud} starts {program}");
            AssertEqual(Entry(program), cartridge.ReadCartridgeWord(SpawnListWord(cloud)), $"dust cloud {cloud} names the native list");
            (bool deleted, int compared) = NativeEnemyProjectileParity.Compare(cartridge, enemies, dust, Entry(program), 256, $"gunship dust {program}");
            AssertTrue(deleted, $"gunship dust {program} deletes itself");
            updates += compared;
        }

        var shown = Enum.GetValues<GunshipDustSegment>()
            .SelectMany(program => GunshipDustAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<GunshipDustFrame, GunshipDustCue>.Frame>()
            .Select(frame => frame.Shown).ToHashSet();
        AssertTrue(shown.SetEquals(GunshipDustVisualDefinitions.Frames), "the dust programs show exactly the installed frames");
        Console.WriteLine($"  gunship dust programs: 6 spawns ({updates} updates, {shown.Count} installed frames) match bank $86.");
    }
}
