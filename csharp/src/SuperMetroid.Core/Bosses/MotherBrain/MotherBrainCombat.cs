using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's hitboxes and touch, and what Samus's weapons do to her head and body.</summary>
[RoomLifetime]
internal sealed class MotherBrainCombat
{
    /// <summary>Provides room services, pictures, colour state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues the encounter's music and sound effects in enemy update order.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Resolves the head, body, projectile, and Samus contact bounds.</summary>
    private readonly EnemyCollision _collision;



    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainCombat(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyAudioQueue audio, EnemyCollision collision)
    {
        _room = room;
        _projectiles = projectiles;
        _audio = audio;
        _collision = collision;
    }

    /// <summary>Runs the head's reaction to a bomb that selects its shot reaction.</summary>
    /// <param name="state">The encounter the head is part of.</param>
    /// <param name="c">The bomb's hit.</param>
    internal void ReactToMotherBrainHeadBomb(MotherBrainEnemyState state, EnemyBombContext c) =>
        ResolveMotherBrainHeadNormalBomb(state, c.Occupant, c.PlacedBomb);

    /// <summary>
    /// The body's shot response for each combat phase: the struck rectangle's own reaction, as the game runs the
    /// rectangle's shot reaction for the body. Her brain's rectangle, which the body shows before its first animation
    /// (#2220), runs the head's reaction on the body; every body rectangle's is the dud shot.
    /// </summary>
    /// <param name="body">Her body, whose encounter the brain's reaction runs in.</param>
    internal EnemyShotReaction BodyShotReactionOf(MotherBrainEntity body) => new()
    {
        BeforeSuperMissileQuake = (in EnemyShotContext c) =>
        {
            if (c.HitboxShot == MotherBrainReactions.HeadShot)
            {
                ResolveMotherBrainHeadShot(body.Encounter, c.Occupant, c.Shot, c.Projectiles, c.ProjectileType, c.ProjectileDamage);
                return true;
            }
            // A Super Missile still shakes the room first.
            c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
            _projectiles.CreateEnemyProjectileDudShot(c.Shot);
            return true;
        },
    };

    /// <summary>The head's shot response for each combat phase.</summary>
    /// <param name="head">Her head, whose encounter the reaction runs in.</param>
    internal EnemyShotReaction HeadShotReactionOf(MotherBrainEntity head) => new()
    {
        BeforeSuperMissileQuake = (in EnemyShotContext c) =>
        {
            ResolveMotherBrainHeadShot(head.Encounter, c.Occupant, c.Shot, c.Projectiles, c.ProjectileType, c.ProjectileDamage);
            return true;
        },
    };

    /// <summary>
    /// Runs her brain's shot reaction for a shot on the brain's rectangle of <paramref name="struck"/>, her head or, before
    /// its first animation, her body (#2220). The glass encounter accepts only missile families; beams pass through
    /// without being converted into an impact, while every accepted missile increments the glass room object's hit count
    /// and flashes her head, whichever part was struck, before common vulnerability damage is applied to the struck part,
    /// which never deletes it.
    /// </summary>
    private void ResolveMotherBrainHeadShot(
        MotherBrainEnemyState state,
        RoomEnemyOccupant struck,
        SamusProjectileOccupant held,
        SamusProjectileSystem projectiles,
        SamusProjectileType projectileType,
        int projectileDamage)
    {
        RoomEnemyPlace head = struck.Place;
        SamusProjectileFamily family = projectileType.Family;
        if (state.Form != MotherBrainForm.Glass)
        {
            ResolveMotherBrainLaterFormHeadShot(
                state,
                struck,
                held,
                projectiles,
                projectileType,
                projectileDamage);
            return;
        }

        // Every overlapping shot is marked as having struck before this reaction;
        // the next projectile pass consumes the marker, removing a Super Missile's pair too.
        TouchHeadWithoutImpact(head, held, projectiles, projectileType);
        // Only missiles and Super Missiles that strike her head reach the glass.
        if (family is not (SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile))
            return;

        _room.Services.MotherBrain.CountHeadShotOnGlass();
        _audio.QueueEnemySound(EnemySound.ShotMotherBrain, priority: SoundRequestPriority.Normal);

        // Her head flashes, whichever part's brain rectangle was struck; a second hit during a flash keeps the flash white
        // or plain as it was. The common damage of a shot that does not kill (#2255) then takes the struck part's health
        // and starts its hurt reaction for its authored duration.
        RestartHurtFlash(state.Head);

        _ = _collision.ApplyNormalShotDamage(struck, held, projectileType, projectileDamage, _room.Area);
    }

    /// <summary>
    /// Runs the head's reaction to a shot after the glass phase, with the damage the second and third phases share. In
    /// <see cref="MotherBrainForm.FakeDeath"/> the shot still costs walk credit but is a dud; in the later forms it deals
    /// ordinary damage, leaving her parts alive at zero health for her body's phases to end.
    /// </summary>
    private void ResolveMotherBrainLaterFormHeadShot(
        MotherBrainEnemyState state,
        RoomEnemyOccupant brain,
        SamusProjectileOccupant held,
        SamusProjectileSystem projectiles,
        SamusProjectileType projectileType,
        int projectileDamage)
    {
        RoomEnemyPlace head = brain.Place;
        _ = held.Projectile;
        // The shot collision marked the projectile before running this reaction.
        TouchHeadWithoutImpact(head, held, projectiles, projectileType);

        SamusProjectileFamily reactionProjectile = projectileType.Family;
        if (state.RainbowBeamSequence is { } sequence)
        {
            // The attached sequence shares the walk counter with the encounter. A shot that takes the walk credit below
            // zero recoils her at once, ending the body's wait, before the common damage takes effect.
            sequence.ApplyPhase2Or3ShotReaction(reactionProjectile);
        }
        else
        {
            // Her third phase begins only within her late fight's sequence, so without it she is in an earlier form.
            state.WalkCounter = MotherBrainShotReaction.BeforeThirdPhase(reactionProjectile, state.WalkCounter);
        }

        if (state.Form == MotherBrainForm.FakeDeath)
        {
            _projectiles.CreateEnemyProjectileDudShot(held);
            return;
        }

        // The reaction ends in common no-death normal-shot damage (#2255). In particular,
        // charged beams use the separate charged vulnerability after the ordinary
        // beam freeze check; Hyper would otherwise inherit Plasma's immunity; and Plasma's
        // hit immunity keeps a surviving penetrating beam from damaging the head every frame.
        _ = _collision.ApplyNormalShotDamage(brain, held, projectileType, projectileDamage, _room.Area);
    }

    /// <summary>
    /// The head's shot prelude: the shot touches her head without an impact, passing through when her head blocks Plasma
    /// or the shot carries none.
    /// </summary>
    private static void TouchHeadWithoutImpact(RoomEnemyPlace head, SamusProjectileOccupant held, SamusProjectileSystem projectiles,
        SamusProjectileType projectileType) =>
        projectiles.EnemyImpacts.TouchEnemyWithoutImpact(
            held,
            head.Contact.HasAny(EnemyContact.BlocksPlasmaBeam) ||
                !projectileType.BeamCombination.HasPlasma);

    /// <summary>
    /// Restarts her head's hurt flash: a second hit during a flash keeps the flash white or plain as it was.
    /// </summary>
    private static void RestartHurtFlash(RoomEnemyPlace head) =>
        head.HurtFlash.Restart(head.HurtFlash.IsRunning && MotherBrainCadenceDefinitions.HurtFlashWhite.IsInSecondHalfAt(head.HurtFlash.UpdatesLeft)
            ? MotherBrainFightDefinitions.HurtFlashKeepingPhase
            : MotherBrainFightDefinitions.HurtFlash);

    /// <summary>
    /// Runs a normal bomb through the reaction of Mother Brain's brain. In her glass form a bomb does nothing. In every
    /// later form it spends one step's walk credit, an overdraw leaving none; her fake death then takes no damage, while
    /// her later forms take the common no-death bomb damage. The frame-hitbox collision has already marked the bomb.
    /// </summary>
    private void ResolveMotherBrainHeadNormalBomb(
        MotherBrainEnemyState state,
        RoomEnemyOccupant head,
        SamusBombOccupant bomb)
    {
        if (state.Form == MotherBrainForm.Glass)
            return;

        // A bomb spends one step's credit and empties the credit when it overdraws it.
        state.WalkCounter = state.WalkCounter.SpendForShot(MotherBrainWalkCredit.StepCredit);
        if (state.Form == MotherBrainForm.FakeDeath)
            return;

        _collision.ApplyNormalBombDamage(head, bomb);
    }

    /// <summary>Runs the head's touch reaction for the active body/head pair.</summary>
    internal static void ReactToMotherBrainHeadTouch(EnemyTouchContext context)
    {
        // The brain's touch is not ordinary touch damage. A spin-jumping Samus toggles
        // the head's visible flash length between its two hurt flashes; every other movement type returns without changing either actor.
        RoomEnemyPlace place = context.Place;
        if (SamusPoseMetadata.ReadMovementType(context.Samus) == SamusMovementType.SpinJumping)
            RestartHurtFlash(place);
    }

    /// <summary>
    /// The reaction of the first hitbox the probe overlaps on her body's frame: her brain's rectangle until its first
    /// animation shows a body frame, then what its frame shows. Her body's own entity asks (#2301).
    /// </summary>
    internal static EnemyHitboxContact FindMotherBrainBodyHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe) =>
        FindHitboxReaction(enemy, MotherBrainCollision.ComponentsAt(enemy.Frame), probe);

    /// <summary>
    /// The reaction of the first hitbox the probe overlaps on her head: her brain, whose rectangle it owns (#2220). Her head's
    /// own entity asks (#2301).
    /// </summary>
    internal static EnemyHitboxContact FindMotherBrainHeadHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe) =>
        FindHitboxReaction(enemy, MotherBrainCollisionDefinitions.BrainComponents, probe);

    /// <summary>The reaction of the first of <paramref name="components"/>' hitboxes the probe overlaps, placed from <paramref name="enemy"/>.</summary>
    private static EnemyHitboxContact FindHitboxReaction(RoomEnemyPlace enemy, ReadOnlySpan<EnemyCollisionComponent> components, EnemyHitboxProbe probe)
    {
        foreach (EnemyCollisionComponent component in components)
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (EnemyCollisionHitbox hitbox in component.Hitboxes)
            {
                if (!probe.Overlaps(
                        componentOrigin.X + hitbox.Left,
                        componentOrigin.Y + hitbox.Top,
                        componentOrigin.X + hitbox.Right,
                        componentOrigin.Y + hitbox.Bottom))
                    continue;
                return EnemyHitboxContact.Struck(probe.Contact == HitboxContactKind.Shot ? hitbox.Shot : hitbox.Touch);
            }
        }
        return EnemyHitboxContact.Missed;
    }
}
