using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Samus.Projectiles;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Phantoon;

/// <summary>
/// The PhantoonSolidifyingCentre compatibility rule (#1937): on the update his body turns solid while it still shows its
/// invulnerable look, a beam at the exact centre of his body is spent without hurting him, and one beside it passes by.
/// </summary>
internal static class PhantoonSolidifyingCentreVerification
{
    /// <summary>
    /// Shows Phantoon's invulnerable look with his body no longer ignoring Samus, as the update after his eye starts to
    /// track her leaves it, and fires an uncharged beam at the centre of his body and then beside it.
    /// </summary>
    internal static void VerifyPhantoonSolidifyingCentre()
    {
        AssertEqual(1, Fire(new(128, 96)), "a beam at his body's centre meets the centre point");
        AssertEqual(0, Fire(new(128 + 5, 96)), "a beam whose reach misses the centre meets nothing on the invulnerable look");
        Console.WriteLine("  Phantoon solidifying centre: a beam at the centre is spent unharmed; one beside it passes by.");
    }

    /// <summary>Fires one uncharged Power Beam shot at <paramref name="at"/>; returns the hits his resolver reports.</summary>
    private static int Fire(Position16<RoomPixels> at)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance);
        // His four parts as his room's population sets them up; the encounter is the one his body's setup began. His body
        // stands at the beam's height, still showing its invulnerable look but no longer ignoring Samus, with his eye tracking her.
        PhantoonEnemyState state = enemies.PlacePhantoon();
        RoomEnemyPlace body = state.Body;
        body.Position = body.Position.WithWhole(new(128, 96));
        body.Frame = EnemyFrameId.PhantoonBody;
        body.Contact = body.Contact.Without(EnemyContact.IgnoresSamus);
        state.Phase = PhantoonPhase.EyeTracksSamus;
        state.PhaseWait.LastFor(60);
        int health = body.Health;

        var shots = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(projectileFrames: InstalledFixtures.Projectiles.FrameBindings), new global::SuperMetroid.Core.Fx.RoomEarthquake());
        SamusProjectile shot = shots.Shots[0];
        shot.Occupy(SamusProjectileType.Beam(SamusBeamCombination.Power, charged: false), 20);
        shot.HeldShot().Direction = SamusProjectileDirection.Right;
        shot.HeldShot().Position = shot.Position.WithWhole(at);
        shot.HeldShot().Radius = new(4, 4);
        shot.HeldShot().Animation = ProjectileFixturePlaybacks.Active();
        shot.HeldShot().Behavior = SamusShotBehaviors.Beam;

        enemies.PhantoonBehavior.Combat.ResolvePhantoonProjectileHits(state, shots, new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()));
        AssertEqual(health, body.Health, "the centre point hurts no one");
        // Only a beam that meets the centre is spent: the one projectile the pass resolves.
        return shot.IsSpent ? 1 : 0;
    }

    /// <summary>Runs the Phantoon solidifying-centre check.</summary>
    [VerificationCommand("--phantoon-solidifying-centre")]
    internal static int PhantoonSolidifyingCentreCommand()
    {
        Suite(nameof(VerifyPhantoonSolidifyingCentre), () => VerifyPhantoonSolidifyingCentre());
        return 0;
    }
}
