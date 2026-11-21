using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Samus.Projectiles;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Phantoon;

/// <summary>
/// Phantoon's shot reaction runs common normal-shot damage (#2255): a beam his eye shrugs off takes the common dud,
/// spent with the dud flash, as the cartridge's shot reaction does through the shared routine, and a charged beam hurts
/// him through the shared hurt flash and damage before his own stage counts the round's damage.
/// </summary>
internal static class PhantoonShotDamageVerification
{
    /// <summary>Fires an uncharged and then a charged Power Beam shot at his open eye while he swoops.</summary>
    internal static void VerifyPhantoonCommonShotDamage()
    {
        (RoomEnemySystem dudEnemies, RoomEnemyPlace dudBody, SamusProjectile dud, int dudHits) = Fire(charged: false, damage: 20);
        AssertEqual(1, dudHits, "an uncharged beam meets his eye");
        AssertEqual(2500, dudBody.Health, "an uncharged beam does him no harm");
        AssertTrue(dud.IsSpent, "the common dud spends the uncharged beam");
        AssertEqual(1, dudEnemies.RoomEffectActors.Count(actor => actor.IsActive && actor.Kind == RoomEffectActorKind.DudShot),
            "the common dud shows its flash where the beam struck");
        AssertTrue((dudBody.AiStates & EnemyAiStates.Hurt) == EnemyAiStates.None, "a dud does not hurt him");

        (RoomEnemySystem hurtEnemies, RoomEnemyPlace hurtBody, SamusProjectile _, int hurtHits) = Fire(charged: true, damage: 60);
        AssertEqual(1, hurtHits, "a charged beam meets his eye");
        AssertEqual(2500 - 60, hurtBody.Health, "a charged beam takes its full damage");
        AssertEqual(hurtBody.HurtFlashFrames + 8, hurtBody.HurtFlash.UpdatesLeft, "the common damage starts his hurt flash");
        AssertTrue((hurtBody.AiStates & EnemyAiStates.Hurt) != EnemyAiStates.None, "the common damage hurts him");
        AssertEqual(0, hurtEnemies.RoomEffectActors.Count(actor => actor.IsActive && actor.Kind == RoomEffectActorKind.DudShot),
            "a hurting beam shows no dud");
        PhantoonEnemyState state = hurtEnemies.BossEncounters.Phantoon
            ?? throw new InvalidOperationException("The fixture's room holds Phantoon's encounter.");
        AssertEqual(60, state.Round.Damage, "his own stage after the common damage counts the round's damage");
        Console.WriteLine("  Phantoon shot damage: an uncharged beam takes the common dud, a charged one the common hurt and damage.");
    }

    /// <summary>Fires one Power Beam shot at his open eye; returns the room, his body, the shot and the hits his resolver reports.</summary>
    private static (RoomEnemySystem Enemies, RoomEnemyPlace Body, SamusProjectile Shot, int Hits) Fire(bool charged, int damage)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        body.Bind(EnemyDefinitionId.PhantoonBody, RoomEnemyDefinitionLookup.Get(EnemyDefinitionId.PhantoonBody));
        body.Position = body.Position.WithWhole(new(128, 96));
        body.Frame = EnemyFrameId.PhantoonEyeHitboxBody;
        body.Contact = body.Contact.Without(EnemyContact.IgnoresSamus);
        body.Health = 2500;
        PhantoonEnemyState state = enemies.BeginPhantoonAsSetUp(new PhantoonEnemyState(body.HeldOccupant(), body, enemies.Places.InUpdateOrder[1],
            enemies.Places.InUpdateOrder[2], enemies.Places.InUpdateOrder[3]) { Phase = PhantoonPhase.Swooping });
        state.PhaseWait.LastFor(60);

        Position16<RoomPixels> eye = EyeCentre(body);
        var shots = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads,
            new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(),
            InstalledFixtures.SamusArtwork(projectileFrames: InstalledFixtures.Projectiles.FrameBindings), new global::SuperMetroid.Core.Fx.RoomEarthquake());
        SamusProjectile shot = shots.Shots[0];
        shot.Occupy(SamusProjectileType.Beam(SamusBeamCombination.Power, charged), damage);
        shot.HeldShot().Direction = SamusProjectileDirection.Right;
        shot.HeldShot().Position = shot.Position.WithWhole(eye);
        shot.HeldShot().Radius = new(4, 4);
        shot.HeldShot().Animation = ProjectileFixturePlaybacks.Active();
        shot.HeldShot().Behavior = SamusShotBehaviors.Beam;

        enemies.PhantoonBehavior.Combat.ResolvePhantoonProjectileHits(state, shots,
            new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), InstalledFixtures.SamusArtwork()));
        // A Power Beam shot that meets one of his hitboxes is spent: the one projectile the pass resolves.
        return (enemies, body, shot, shot.IsSpent ? 1 : 0);
    }

    /// <summary>The centre of the hitbox on his body's frame whose shot reaction is his own: his eye.</summary>
    private static Position16<RoomPixels> EyeCentre(RoomEnemyPlace body)
    {
        foreach (EnemyCollisionComponent component in PhantoonCollision.ComponentsAt(body.Frame))
        {
            foreach (EnemyCollisionHitbox hitbox in component.Hitboxes)
            {
                if (hitbox.Shot != PhantoonReactions.Shot)
                    continue;
                Position16<RoomPixels> origin = body.Position.Whole + component.Offset;
                return new(origin.X + (hitbox.Left + hitbox.Right) / 2, origin.Y + (hitbox.Top + hitbox.Bottom) / 2);
            }
        }
        throw new InvalidOperationException("His eye-hitbox body frame has no hitbox with his own shot reaction.");
    }

    /// <summary>Runs the Phantoon shot damage check.</summary>
    [VerificationCommand("--phantoon-shot-damage")]
    internal static int PhantoonShotDamageCommand()
    {
        Suite(nameof(VerifyPhantoonCommonShotDamage), () => VerifyPhantoonCommonShotDamage());
        return 0;
    }
}
