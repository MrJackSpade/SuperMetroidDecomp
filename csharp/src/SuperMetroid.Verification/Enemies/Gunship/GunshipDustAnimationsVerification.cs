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
    internal static void VerifyGunshipDustAnimations(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int updates = 0;
        for (ushort parameter = 0; parameter <= 10; parameter += 2)
        {
            var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance);
            var guard = new MutableCartridgeReadGuard(rom);
            guard.Forbid(address => address is >= ListsStart and < ListsEnd,
                address => $"Production read a native gunship dust list at ${address:X6}.");
            enemies.Projectiles.SpawnGunshipLiftoffDustCloud(parameter, new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()));
            RoomEnemyProjectileSlot dust = enemies.Projectiles.EnemyProjectiles.Single(item => item.Kind == RoomEnemyProjectileKind.GunshipLiftoffDustCloud);
            GunshipDustProgram program = GunshipDust.ForParameter(parameter).Program;
            AssertTrue(dust.Animation is GunshipDustAnimation started && started.Cursor == AnimationProgramCursor<GunshipDustProgram>.Start(program),
                $"dust parameter {parameter} starts {program}");
            AssertEqual(Entry(program), cartridge.ReadCartridgeWord(SpawnListWord(parameter)), $"dust parameter {parameter} names the native list");
            (bool deleted, int compared) = NativeEnemyProjectileParity.Compare(cartridge, enemies, dust, Entry(program), 256, $"gunship dust {program}");
            AssertTrue(deleted, $"gunship dust {program} deletes itself");
            updates += compared;
        }

        var shown = Enum.GetValues<GunshipDustProgram>()
            .SelectMany(program => GunshipDustAnimations.Programs.Steps(program))
            .OfType<AnimationProgramStep<GunshipDustProgram, GunshipDustFrame, GunshipDustCue>.ShowStep>()
            .Select(step => step.Frame).ToHashSet();
        AssertTrue(shown.SetEquals(GunshipDustVisualDefinitions.Frames), "the dust programs show exactly the installed frames");
        Console.WriteLine($"  gunship dust programs: 6 spawns ({updates} updates, {shown.Count} installed frames) match bank $86.");
    }
}
