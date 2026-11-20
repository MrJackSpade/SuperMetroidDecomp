using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
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
    internal void ReactToMotherBrainHeadBomb(EnemyBombContext c) =>
        ResolveMotherBrainHeadNormalBomb(c.Enemy, c.Bomb);

    /// <summary>The body's shot response for each combat phase.</summary>
    internal EnemyShotReaction BodyShotReaction => field ??= new()
    {
        BeforeSuperMissileQuake = (in EnemyShotContext c) =>
        {
            // Every body rectangle's shot reaction is exactly the dud shot; only her brain's rectangle, which her body
            // shows before its first animation (#2220), could select anything else.
            if (c.HitboxShot != MotherBrainReactions.BodyShot)
            {
                throw new InvalidDataException(
                    $"Mother Brain's body selected the shot reaction {c.HitboxShot?.ToString() ?? "(unnamed)"} from " +
                    $"frame {c.Enemy.Frame}; every body rectangle's shot reaction is the dud shot.");
            }
            // A Super Missile still shakes the room first.
            c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Projectile, c.MarksCollision);
            _projectiles.CreateEnemyProjectileDudShot(c.Projectile);
            return true;
        },
    };

    /// <summary>The head's shot response for each combat phase.</summary>
    internal EnemyShotReaction HeadShotReaction => field ??= new()
    {
        BeforeSuperMissileQuake = (in EnemyShotContext c) =>
        {
            ResolveMotherBrainHeadShot(c.Enemy, c.Projectile, c.Projectiles, c.ProjectileType, c.ProjectileDamage);
            return true;
        },
    };

    /// <summary>
    /// Reacts to a shot at the head in Mother Brain's first form. The glass encounter
    /// accepts only missile families; beams pass through without being converted into an
    /// impact, while every accepted missile increments the glass room object's hit count before common
    /// vulnerability damage is applied, which never deletes the head.
    /// </summary>
    private void ResolveMotherBrainHeadShot(
        RoomEnemyPlace head,
        SamusProjectile projectile,
        SamusProjectileSystem projectiles,
        SamusProjectileType projectileType,
        int projectileDamage)
    {
        MotherBrainEnemyState state = MotherBrainEntity.EncounterOf(head);
        SamusProjectileFamily family = projectileType.Family;
        if (state.Form != MotherBrainForm.Glass)
        {
            ResolveMotherBrainLaterFormHeadShot(
                state,
                head,
                projectile,
                projectiles,
                projectileType,
                projectileDamage);
            return;
        }

        // Every overlapping shot is marked as having struck before this reaction;
        // the next projectile pass consumes the marker, removing a Super Missile's pair too.
        TouchHeadWithoutImpact(head, projectile, projectiles, projectileType);
        // Only missiles and Super Missiles that strike her head reach the glass.
        if (family is not (SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile))
            return;

        _room.Services.MotherBrain.CountHeadShotOnGlass();
        _audio.QueueEnemySound(EnemySound.ShotMotherBrain, priority: SoundRequestPriority.Normal);

        // A second hit during a flash keeps the flash white or plain as it was. The common damage of a shot that does
        // not kill (#2255) then takes her health and starts her hurt reaction for its authored duration.
        RestartHurtFlash(head);

        _ = _collision.ApplyNormalShotDamage(head, projectile, projectileType, projectileDamage, _room.Area);
    }

    /// <summary>
    /// Runs the head's reaction to a shot after the glass phase, with the damage the second and third phases share. In
    /// <see cref="MotherBrainForm.FakeDeath"/> the shot still costs walk credit but is a dud; in the later forms it deals
    /// ordinary damage, leaving her parts alive at zero health for her body's phases to end.
    /// </summary>
    private void ResolveMotherBrainLaterFormHeadShot(
        MotherBrainEnemyState state,
        RoomEnemyPlace head,
        SamusProjectile projectile,
        SamusProjectileSystem projectiles,
        SamusProjectileType projectileType,
        int projectileDamage)
    {
        // The shot collision marked the projectile before running this reaction.
        TouchHeadWithoutImpact(head, projectile, projectiles, projectileType);

        // A missile's explosion reacts as a beam does.
        MotherBrainProjectileType reactionProjectile = projectileType.Family switch
        {
            SamusProjectileFamily.Beam or SamusProjectileFamily.MissileExplosion => MotherBrainProjectileType.Beam,
            SamusProjectileFamily.Missile => MotherBrainProjectileType.Missile,
            SamusProjectileFamily.SuperMissile => MotherBrainProjectileType.SuperMissile,
            SamusProjectileFamily.PowerBomb => MotherBrainProjectileType.PowerBomb,
            SamusProjectileFamily.Bomb => MotherBrainProjectileType.Bomb,
            SamusProjectileFamily.BeamExplosion => MotherBrainProjectileType.BeamExplosion,
            _ => throw new InvalidOperationException($"Undefined {nameof(SamusProjectileFamily)} {projectileType.Family}."),
        };
        if (state.RainbowBeamSequence is { } sequence)
        {
            // The attached sequence shares the walk counter with the encounter. A shot that takes the walk credit below
            // zero recoils her at once, ending the body's wait, before the common damage takes effect.
            sequence.ApplyPhase2Or3ShotReaction(reactionProjectile);
        }
        else
        {
            MotherBrainShotReactionResult reaction =
                MotherBrainShotReaction.Resolve(state.Form, reactionProjectile, state.WalkCounter);
            if (reaction.HyperBeamRecoil)
            {
                throw new InvalidOperationException(
                    "Mother Brain took a third-phase Hyper Beam recoil without her third-phase sequence.");
            }
            state.WalkCounter = reaction.WalkCredit;
        }

        if (state.Form == MotherBrainForm.FakeDeath)
        {
            _projectiles.CreateEnemyProjectileDudShot(projectile);
            return;
        }

        // The reaction ends in common no-death normal-shot damage (#2255). In particular,
        // charged beams use the separate charged vulnerability after the ordinary
        // beam freeze check; Hyper would otherwise inherit Plasma's immunity; and Plasma's
        // hit immunity keeps a surviving penetrating beam from damaging the head every frame.
        _ = _collision.ApplyNormalShotDamage(head, projectile, projectileType, projectileDamage, _room.Area);
    }

    /// <summary>
    /// The head's shot prelude: the shot touches her head without an impact, passing through when her head blocks Plasma
    /// or the shot carries none.
    /// </summary>
    private static void TouchHeadWithoutImpact(RoomEnemyPlace head, SamusProjectile projectile, SamusProjectileSystem projectiles,
        SamusProjectileType projectileType) =>
        projectiles.EnemyImpacts.TouchEnemyWithoutImpact(
            projectile,
            head.Contact.HasAny(EnemyContact.BlocksPlasmaBeam) ||
                !projectileType.BeamCombination.HasPlasma);

    /// <summary>
    /// Restarts her head's hurt flash: a second hit during a flash keeps the flash white or plain as it was.
    /// </summary>
    private static void RestartHurtFlash(RoomEnemyPlace head) =>
        head.HurtFlash.Restart(head.HurtFlash.IsRunning && MotherBrainCadenceDefinitions.HurtFlashWhite.IsSetIn(head.HurtFlash.UpdatesLeft)
            ? MotherBrainFightDefinitions.HurtFlashKeepingPhase
            : MotherBrainFightDefinitions.HurtFlash);

    /// <summary>
    /// Runs a normal bomb through the reaction of Mother Brain's brain. In her glass form a bomb does nothing. In every
    /// later form it spends one step's walk credit, an overdraw leaving none; her fake death then takes no damage, while
    /// her later forms take the common no-death bomb damage. The frame-hitbox collision has already marked the bomb.
    /// </summary>
    private void ResolveMotherBrainHeadNormalBomb(
        RoomEnemyPlace head,
        SamusBomb bomb)
    {
        MotherBrainEnemyState state = MotherBrainEntity.EncounterOf(head);

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
    internal static bool FindMotherBrainBodyHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction) =>
        FindHitboxReaction(enemy, enemy.Frame == EnemyFrameId.MotherBrainBrainHitArea
            ? MotherBrainCollisionDefinitions.BrainComponents
            : MotherBrainCollision.ComponentsAt(enemy.Frame), probe, out reaction);

    /// <summary>
    /// The reaction of the first hitbox the probe overlaps on her head: her brain, whose rectangle it owns (#2220). Her head's
    /// own entity asks (#2301).
    /// </summary>
    internal static bool FindMotherBrainHeadHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction) =>
        FindHitboxReaction(enemy, MotherBrainCollisionDefinitions.BrainComponents, probe, out reaction);

    /// <summary>The reaction of the first of <paramref name="components"/>' hitboxes the probe overlaps, placed from <paramref name="enemy"/>.</summary>
    private static bool FindHitboxReaction(RoomEnemyPlace enemy, ReadOnlySpan<EnemyCollisionComponent> components, EnemyHitboxProbe probe,
        out EnemyReaction? reaction)
    {
        reaction = null;
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
                reaction = probe.SelectsShot ? hitbox.Shot : hitbox.Touch;
                return true;
            }
        }
        return false;
    }
}
