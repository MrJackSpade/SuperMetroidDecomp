using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Kago;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that Kago starts its slow loop and switches to the post-hit loop when first shot.</summary>
internal static class KagoBehaviorVerification
{
    /// <summary>
    /// The initializer starts the slow loop, which shows its four frames ten updates apiece; the first accepted shot
    /// starts the fast loop on the next update, three updates apiece, while keeping its bug-spawning side effect.
    /// </summary>
    internal static void VerifyKagoBehavior()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().ReadRandom = () => 0;
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Kago, place.KeptDefinition);
        place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        place.SetSpawnParameter1(EnemyDefinitionId.Kago, 10);
        // The Kago's setup builds the state its reaction and its animation, which the check plays, run on.
        KagoEnemyState state = KagoBehavior.InitializeKago(place);
        void Animate() => KagoBehavior.AdvanceKagoTypedAnimation(place, state);
        ushort[] slow = KagoAnimationsConstants.Spritemaps(KagoSegment.Slow).ToArray();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. slow, slow[0]], [10, 10, 10, 10, 10]),
            EnemyInstructionRunner.ShownFrames(place, 41, Animate), "Kago slow loop");

        enemies.KagoBehavior.ResolveKagoShotAfterCommon(place.HeldOccupant(), place, state);
        AssertEqual(1, state.HitsTaken, "Kago real shot handoff counts its first hit");
        AssertEqual(1, enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.KagoBug),
            "Kago real shot handoff retains its projectile side effect");
        ushort[] fast = KagoAnimationsConstants.Spritemaps(KagoSegment.Fast).ToArray();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. fast, fast[0]], [3, 3, 3, 3, 3]),
            EnemyInstructionRunner.ShownFrames(place, 13, Animate), "Kago post-hit loop");
        Console.WriteLine("  Kago behavior: the slow loop and the shot handoff to the post-hit loop run as the cartridge does.");
    }

    /// <summary>Runs the Kago behavior checks and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--kago-instruction-mechanics")]
    internal static int KagoInstructionMechanicsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyKagoBehavior), () => VerifyKagoBehavior());
        Suite(nameof(KagoAnimationsVerification.VerifyKagoAnimations), () => KagoAnimationsVerification.VerifyKagoAnimations(rom));
        return 0;
    }
}
