using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Enemies.Gunship.GunshipDustNativeListAddresses;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Gunship;

/// <summary>
/// #1290: the gunship liftoff dust clouds' typed programs against the retail cartridge. Each spawn parameter's real
/// producer must start its program, which then matches the native bank-$86 list update by update until both delete the
/// cloud; every installed frame must be one the programs show.
/// </summary>
internal static class GunshipDustAnimationsVerification
{
    /// <summary>Samus's X as the liftoff raises its dust: the landed ship's, where she boarded.</summary>
    private const int LiftoffSamusX = GunshipMotionDefinitions.LandedX;

    /// <summary>Samus's Y as the liftoff raises its dust: the height below the landed top hull she rides at.</summary>
    private const int LiftoffSamusY = GunshipMotionDefinitions.LandedTopY + GunshipMotionDefinitions.TopHullAboveSamus;

    /// <summary>Every dust program matches its native list.</summary>
    internal static void VerifyGunshipDustAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int updates = 0;
        for (int cloud = 0; cloud < GunshipDustDefinitions.LiftoffClouds.Count; cloud++)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            var guard = new MutableCartridgeReadGuard(rom);
            guard.Forbid(address => address is >= ListsStart and < ListsEnd,
                address => $"Production read a native gunship dust list at ${address:X6}.");
            GunshipDustPlacement placement = GunshipDustDefinitions.LiftoffClouds[cloud];
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
            // #2392: the clouds rise while Samus rides the landed ship, so she stands where the Landing Site places her.
            samus.Position = samus.Position.WithWhole(new(LiftoffSamusX, LiftoffSamusY));
            GunshipLiftoffDust.SpawnCloud(enemies.Projectiles, placement, samus);
            EnemyProjectile dust = enemies.Projectiles.EnemyProjectiles.Single(item => item.Kind == RoomEnemyProjectileKind.GunshipLiftoffDustCloud);
            AssertEqual(LiftoffSamusX + placement.XOffset, (int)dust.Position.X.Whole, $"dust cloud {cloud} sits its offset beside Samus");
            AssertEqual(LiftoffSamusY + GunshipDustDefinitions.YOffset, (int)dust.Position.Y.Whole, $"dust cloud {cloud} sits below Samus");
            GunshipDustSegment program = placement.Animation;
            AssertTrue(dust.Animation is GunshipDustAnimation started && started.Playback == new AnimationPlayback<GunshipDustSegment>(GunshipDustAnimations.StartingWith(program), AnimationPlace.StartOfAnimation),
                $"dust cloud {cloud} starts {program}");
            AssertEqual(Entry(program), cartridge.ReadCartridgeWord(SpawnListWord(cloud)), $"dust cloud {cloud} names the native list");
            (bool deleted, int compared) = NativeEnemyProjectileParity.Compare(cartridge, enemies, dust, Entry(program), 256, $"gunship dust {program}");
            AssertTrue(deleted, $"gunship dust {program} deletes itself");
            updates += compared;
        }

        var shown = Enum.GetValues<GunshipDustSegment>()
            .SelectMany(program => GunshipDustAnimations.Segments.Beats(program))
            .OfType<AnimationBeat<EnemyProjectileFrameId, GunshipDustCue>.Frame>()
            .Select(frame => frame.Shown).ToHashSet();
        AssertEqual(46, shown.Count, "the dust programs show 46 distinct frames");
        Console.WriteLine($"  gunship dust programs: 6 spawns ({updates} updates, {shown.Count} installed frames) match bank $86.");
    }
}
