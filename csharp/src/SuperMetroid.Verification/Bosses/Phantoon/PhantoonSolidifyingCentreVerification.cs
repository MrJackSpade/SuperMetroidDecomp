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
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        body.Bind(EnemyDefinitionId.PhantoonBody, body.Definition);
        body.Bind(body.DefinitionId, RoomEnemyDefinitionLookup.Get(body.DefinitionId));
        body.Position = body.Position.WithWhole(new(128, 96));
        body.Frame = EnemyFrameId.PhantoonInvulnerableBody;
        body.Contact = body.Contact.Without(EnemyContact.IgnoresSamus);
        body.Health = 2500;
        var state = new PhantoonEnemyState(body.HeldOccupant(), body, enemies.Places.InUpdateOrder[1], enemies.Places.InUpdateOrder[2], enemies.Places.InUpdateOrder[3]) { Phase = PhantoonPhase.EyeTracksSamus };
        state.PhaseWait.LastFor(60);
        enemies.BeginPhantoonAsSetUp(state);

        var shots = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(projectileFrames: InstalledFixtures.Projectiles.FrameBindings), new global::SuperMetroid.Core.Fx.RoomEarthquake());
        SamusProjectile shot = shots.Shots[0];
        shot.Type = SamusProjectileType.Beam(SamusBeamCombination.Power, charged: false);
        shot.Damage = 20;
        shot.Direction = SamusProjectileDirection.Right;
        shot.Position = shot.Position.WithWhole(at);
        shot.Radius = new(4, 4);
        shot.Animation = ProjectileFixturePlaybacks.Active();
        shot.Behavior = SamusShotBehaviors.Beam;

        int hits = enemies.PhantoonBehavior.Combat.ResolvePhantoonProjectileHits(state, shots, new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()));
        AssertEqual(2500, body.Health, "the centre point hurts no one");
        AssertEqual(hits == 1, shot.IsSpent, "only a beam that meets the centre is spent");
        return hits;
    }

    /// <summary>Runs the Phantoon solidifying-centre check.</summary>
    [VerificationCommand("--phantoon-solidifying-centre")]
    internal static int PhantoonSolidifyingCentreCommand()
    {
        Suite(nameof(VerifyPhantoonSolidifyingCentre), () => VerifyPhantoonSolidifyingCentre());
        return 0;
    }
}
