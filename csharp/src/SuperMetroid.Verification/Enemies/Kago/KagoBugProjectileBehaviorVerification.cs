using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.Kago;

/// <summary>#1292: the Kago's bugs run typed landing, falling and jumping programs.</summary>
internal static class KagoBugProjectileBehaviorVerification
{
    /// <summary>Each of the bug's landing, falling and jumping programs matches its native list update for update, its cues included.</summary>
    internal static void VerifyKagoBugProjectileBehavior(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, KagoBugProjectileNativeListAddresses.Extent, "Kago bug");
        foreach (KagoBugProjectileSegment program in new[] { KagoBugProjectileSegment.Landed, KagoBugProjectileSegment.Falling, KagoBugProjectileSegment.JumpStart })
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            enemies.VerificationServices().ReadRandom = () => 1;
            // The shell releases the bug with the typed state its cues keep: the source shell and its countdowns.
            AssertTrue(enemies.KagoBehavior.SpawnKagoBug(new RoomEnemyPlace(0, EnemyPlaceFixtures.Registry, new RemovedEnemyLeftovers<EnemyOccupantStates>())), "the real Kago producer allocates its bug");
            EnemyProjectile bug = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind == RoomEnemyProjectileKind.KagoBug);
            bug.StartAnimation(new KagoBugProjectileAnimation(program));
            (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, bug, KagoBugProjectileNativeListAddresses.Entry(program), 40, program.ToString());
            AssertTrue(!deleted, $"{program} keeps the bug");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the bugs' native list bytes");
        Console.WriteLine("  Kago bug behavior: the landing, falling and jumping programs match their native lists.");
    }

    /// <summary>
    /// #2045: a waiting Kago bug and Kraid's resting rocks play the same native list, but each sheet draws it in its own
    /// colours, so they are different pictures, one per appearance: the bug's waiting frame is its own identity, among its
    /// Kago's spawned pictures, and the rock's stays among Kraid's sets' shared ones. A recorded operand the two share reads
    /// as the frame of the sheet the projectile draws from.
    /// </summary>
    internal static void VerifyKagoBugWaitingPicture()
    {
        AssertTrue(KagoBugProjectileAnimations.Segments.Beats(KagoBugProjectileSegment.Waiting) is [{ ShownFrame: EnemyProjectileFrameId.KagoBugWaiting }],
            "a waiting Kago bug holds its own waiting frame");
        SpawnedObjectSheet kago = InstalledFixtures.EnemySheets.Spawned(SpawnedObjectFixtures.SetDrawing(EnemyDefinitionId.Kago),
            SpawnedObjectOwner.Of(EnemyDefinitionId.Kago));
        SpawnedObjectSheet kraid = InstalledFixtures.EnemySheets.Spawned(SpawnedObjectFixtures.SetDrawing(EnemyDefinitionId.Kraid), SpawnedObjectOwner.Shared);
        string waiting = EnemySheetDefinitions.ProjectileFrameName(EnemyProjectileFrameId.KagoBugWaiting);
        string resting = EnemySheetDefinitions.ProjectileFrameName(EnemyProjectileFrameId.KraidRock);
        // Each sheet's finished pictures, those that show nothing included: the bug's colours may ink none of the pose.
        AssertTrue(Pictures(kago).Contains(waiting) && !Pictures(kago).Contains(resting),
            "the Kago's spawned pictures hold the waiting bug and no picture of Kraid's rock");
        AssertTrue(Pictures(kraid).Contains(resting) && !Pictures(kraid).Contains(waiting),
            "Kraid's shared pictures hold the resting rock and no picture of the Kago's bug");
        AssertTrue(!kraid.ShowsNothing(resting), "the resting rock shows its picture");
        ushort shared = EnemyProjectileProgramFrameSources.Operand(EnemyProjectileFrameId.KraidRock);
        AssertEqual(shared, EnemyProjectileProgramFrameSources.Operand(EnemyProjectileFrameId.KagoBugWaiting), "the waiting bug and the resting rock draw one native operand");
        AssertEqual(EnemyProjectileFrameId.KagoBugWaiting, EnemyProjectileProgramFrameOperands.Frame(shared, SpawnedObjectOwner.Of(EnemyDefinitionId.Kago)),
            "the shared operand drawn from the Kago's sheet reads as the waiting bug");
        AssertEqual(EnemyProjectileFrameId.KraidRock, EnemyProjectileProgramFrameOperands.Frame(shared, SpawnedObjectOwner.Shared),
            "the shared operand drawn from the shared sheet reads as the resting rock");
        Console.WriteLine("  Kago bug waiting picture: its own frame among its Kago's pictures, apart from Kraid's resting rock.");

        static HashSet<string> Pictures(SpawnedObjectSheet sheet) => [.. sheet.frames.Keys, .. sheet.empty];
    }

    /// <summary>Runs the bugs' program oracle and behavior suite.</summary>
    [VerificationCommand("--kago-bug-programs")]
    internal static int KagoBugProjectileProgramsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(KagoBugProjectileAnimationsVerification.VerifyKagoBugProjectileAnimations), () => KagoBugProjectileAnimationsVerification.VerifyKagoBugProjectileAnimations(rom));
        Suite(nameof(VerifyKagoBugProjectileBehavior), () => VerifyKagoBugProjectileBehavior(rom));
        Suite(nameof(VerifyKagoBugWaitingPicture), VerifyKagoBugWaitingPicture);
        return 0;
    }
}
