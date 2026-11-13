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
    /// <summary>Provides room services, tiles, palette state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues the encounter's music and sound effects in enemy update order.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Resolves the head, body, projectile, and Samus contact bounds.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Holds the shared body/head state for the current Mother Brain room.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainCombat(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyAudioQueue audio, EnemyCollision collision, MotherBrainEncounter motherBrainEncounter)
    {
        _room = room;
        _projectiles = projectiles;
        _audio = audio;
        _collision = collision;
        _motherBrainEncounter = motherBrainEncounter;
    }

    /// <summary>Installs the handlers used when Samus's bombs contact Mother Brain.</summary>
    internal void AddMotherBrainBombReactions(EnemySharedReactions table)
    {
        table.AddBombSpecial(c => c.Enemy.DefinitionId == EnemyDefinitionId.MotherBrainHead &&
                                  c.SelectedShotAi == EnemyReaction.MotherBrainHeadShot,
            c => ResolveMotherBrainHeadNormalBomb(c.Enemy, c.Bomb));
    }

    /// <summary>Binds head and body shot responses for each combat phase.</summary>
    internal void AddMotherBrainShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.MotherBrainBody &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.MotherBrainBodyShot)
        {
            BeforeSuperMissileQuake = (in EnemyShotContext c) =>
            {
                // Every body rectangle's shot callback is exactly the dud shot;
                // only the dummy root's head-callback list could select anything else.
                if (c.HitboxShotAi != EnemyReaction.MotherBrainBodyShot)
                {
                    throw new InvalidDataException(
                        $"Mother Brain's body selected shot AI {c.HitboxShotAi?.ToString() ?? "(unnamed)"} from " +
                        $"frame {c.Enemy.Frame}; only the dud-shot callback is ported.");
                }
                // The common collision prelude still owns Super-Missile quake first.
                c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile, c.MarksCollision);
                _projectiles.CreateEnemyProjectileDudShot(c.Projectile);
                return true;
            },
        });

        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.MotherBrainHead &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.MotherBrainHeadShot)
        {
            BeforeSuperMissileQuake = (in EnemyShotContext c) =>
            {
                ResolveMotherBrainHeadShot(c.Enemy, c.Projectile, c.Projectiles, c.ProjectileType, c.ProjectileDamage);
                return true;
            },
        });
    }

    /// <summary>
    /// Runs the first-form branch of the head's shot AI. The glass encounter
    /// accepts only missile families; beams pass through without being converted into an
    /// impact, while every accepted missile increments the glass room object's hit count before common
    /// vulnerability damage is applied without the ordinary enemy-deletion tail.
    /// </summary>
    private void ResolveMotherBrainHeadShot(
        RoomEnemySlot head,
        SamusProjectile projectile,
        SamusProjectileSystem projectiles,
        SamusProjectileType projectileType,
        int projectileDamage)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "Mother Brain head shot AI ran without its multipart encounter state.");
        if (!ReferenceEquals(state.Head, head))
        {
            throw new InvalidOperationException(
                "Mother Brain head shot AI ran for a slot not linked to the loaded body.");
        }
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

        // The walker has marked every overlapping family before this callback;
        // the next projectile pass consumes the marker, removing a Super Missile's pair too.
        projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(
            projectile,
            head.Contact.HasAny(EnemyContact.BlocksPlasmaBeam) ||
                !projectileType.BeamCombination.HasPlasma);
        // The head's shot AI lets only missiles and Super Missiles reach the glass.
        if (family is not (SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile))
            return;

        _room.Services.MotherBrain.IncrementGlassRoomArgument();
        _audio.QueueEnemySound(EnemySound.ShotMotherBrain, priority: SoundRequestPriority.Normal);

        // The flash step deliberately alternates odd/even flash parity when a second hit
        // arrives during the existing flash. The common no-death damage helper then owns
        // health, hurt-AI, and the final authored hurt duration.
        head.FlashTimer = head.FlashTimer != 0 && MotherBrainCadenceDefinitions.HurtFlashWhite.IsSetIn(head.FlashTimer)
            ? 14
            : 13;

        EnemyWeaponVulnerability vulnerability = EnemyCollision.ReadProjectileVulnerability(head, projectileType);
        if (vulnerability.FreezesWithoutDamage)
        {
            head.FrozenTimer = 400;
            head.AiStates = head.AiStates | EnemyAiStates.Frozen;
            head.InvincibilityTimer = 10;
            return;
        }

        int damage = (projectileDamage >> 1) * vulnerability.Multiplier;
        if (damage == 0)
        {
            _projectiles.CreateEnemyProjectileDudShot(projectile);
            return;
        }

        head.FlashTimer = head.HurtFlashFrames + 8;
        head.AiStates = head.AiStates | EnemyAiStates.Hurt;
        head.Health = damage >= head.Health
            ? 0
            : head.Health - damage;
    }

    /// <summary>
    /// Runs the later forms of the head's shot AI plus the phase-two/three common no-death damage tail.
    /// Form one (the fake-death interval) still runs the recoil bookkeeping but converts the
    /// projectile into a dud. Forms two and later apply ordinary vulnerability damage while
    /// deliberately leaving the multipart boss records alive at zero health for body AI.
    /// </summary>
    internal void ResolveMotherBrainLaterFormHeadShot(
        MotherBrainEnemyState state,
        RoomEnemySlot head,
        SamusProjectile projectile,
        SamusProjectileSystem projectiles,
        SamusProjectileType projectileType,
        int projectileDamage)
    {
        // The walker marked the projectile before dispatching this callback.
        projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(
            projectile,
            head.Contact.HasAny(EnemyContact.BlocksPlasmaBeam) ||
                !projectileType.BeamCombination.HasPlasma);

        // The reaction step reads its eight-row reaction table at the family's row wrapped to eight
        // rows, so missile explosions alias beams.
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
            // The attached sequence shares the walk counter with the encounter. The recoil
            // clears the body function timer on recoil underflow and stores the counter
            // immediately, before common projectile damage runs.
            sequence.ApplyPhase2Or3ShotReaction(reactionProjectile);
            state.FunctionTimer = sequence.State.FunctionTimer;
        }
        else
        {
            MotherBrainShotReactionResult reaction =
                MotherBrainShotReaction.Resolve(state.Form, reactionProjectile, state.WalkCounter);
            if (reaction.HyperBeamRecoil)
            {
                throw new InvalidOperationException(
                    "Mother Brain received a form-four beam without its phase-three state owner.");
            }
            state.WalkCounter = reaction.WalkCredit;
        }

        if (state.Form == MotherBrainForm.FakeDeath)
        {
            _projectiles.CreateEnemyProjectileDudShot(projectile);
            return;
        }

        // The callback tail-calls common no-death shot damage. In particular,
        // charged beams use the separate charged vulnerability after the ordinary
        // beam freeze check; Hyper would otherwise inherit the immune plasma entry.
        EnemyCollision.NormalShotVulnerability vulnerability = EnemyCollision.ReadNormalShotVulnerability(head, projectileType);
        if (vulnerability.FreezeImmediately)
        {
            head.FrozenTimer = 400;
            head.AiStates = head.AiStates | EnemyAiStates.Frozen;
            head.InvincibilityTimer = 10;
            return;
        }

        int damage = (projectileDamage >> 1) * vulnerability.Multiplier;
        if (damage == 0)
        {
            _projectiles.CreateEnemyProjectileDudShot(projectile);
            return;
        }

        head.FlashTimer = head.HurtFlashFrames + 8;
        head.AiStates = head.AiStates | EnemyAiStates.Hurt;
        // The shared no-death tail still grants Plasma's hit immunity.
        // A surviving penetrating beam cannot damage this head again every frame.
        if (projectileType.BeamCombination.HasPlasma)
            head.InvincibilityTimer = EnemyShotTiming.PlasmaInvincibilityFrames;
        head.Health = damage >= head.Health
            ? 0
            : head.Health - damage;
    }

    /// <summary>
    /// Runs a normal bomb through Mother Brain's brain callback.
    /// The first form's eight-entry projectile-family table rejects bombs outright. Later
    /// forms always run the recoil step: family five selects reaction zero, subtracting
    /// 256 from the walk/recoil credit with a signed-zero clamp. Fake-death
    /// form one then creates only a dud, while forms two and later enter common no-death
    /// damage. The frame-hitbox caller already owns the bomb collision mark.
    /// </summary>
    private void ResolveMotherBrainHeadNormalBomb(
        RoomEnemySlot head,
        SamusBombProjectileSlot bomb)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "Mother Brain head normal-bomb AI ran without its multipart encounter state.");
        if (!ReferenceEquals(state.Head, head))
        {
            throw new InvalidOperationException(
                "Mother Brain head normal-bomb AI ran for a slot not linked to the loaded body.");
        }

        if (state.Form == MotherBrainForm.Glass)
            return;

        // A bomb is reaction type zero, which spends one step and empties the credit when the result is
        // negative.
        state.WalkCounter = state.WalkCounter.SpendForShot(MotherBrainWalkCredit.StepCredit);
        if (state.Form == MotherBrainForm.FakeDeath)
            return;

        _collision.ApplyCommonNormalBombDamage(head, bomb, runGenericDeath: false);
    }

    /// <summary>Registers the head-touch reaction for the active body/head pair.</summary>
    internal static void AddMotherBrainHeadTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.MotherBrainHead &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.MotherBrainHeadTouch,
            context =>
            {
                // The brain's touch is not ordinary touch damage. A spin-jumping Samus toggles
                // the head's visible flash length between the game's 13/14-frame
                // cadence; every other movement type returns without changing either actor.
                RoomEnemySlot slot = context.Slot;
                if (SamusPoseMetadata.ReadMovementType(context.Samus) == SamusMovementType.SpinJumping)
                    slot.FlashTimer = slot.FlashTimer != 0 && MotherBrainCadenceDefinitions.HurtFlashWhite.IsSetIn(slot.FlashTimer)
                        ? 14
                        : 13;
            });
    }

    /// <summary>Adds the body's bomb reactions to the AI dispatch table.</summary>
    /// <remarks>The body callback is the dud shot. The frame-hitbox bomb walker has already marked the physical explosion, and
    /// the body callback performs no health, phase, or multipart-state work of its own.</remarks>
    internal static void AddMotherBrainBodyBombReactions(EnemySharedReactions table)
    {
        table.AddBombSpecial(c => c.Enemy.DefinitionId == EnemyDefinitionId.MotherBrainBody &&
                                  c.SelectedShotAi == EnemyReaction.MotherBrainBodyShot,
            static _ => { });
    }

    /// <summary>The reaction of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindMotherBrainHitboxReaction(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        if (enemy.DefinitionId is not (EnemyDefinitionId.MotherBrainBody or EnemyDefinitionId.MotherBrainHead))
            return null;

        // Body and head walk the same roots; the head's dummy list keeps its dummy root.
        foreach (EnemyCollisionComponent component in
                 MotherBrainCollision.ComponentsAt(enemy.Frame))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (EnemyCollisionHitbox hitbox in component.Hitboxes)
            {
                if (!probe.Overlaps(
                        CoordinateAxis.Wrap(componentOrigin.X + hitbox.Left),
                        CoordinateAxis.Wrap(componentOrigin.Y + hitbox.Top),
                        CoordinateAxis.Wrap(componentOrigin.X + hitbox.Right),
                        CoordinateAxis.Wrap(componentOrigin.Y + hitbox.Bottom)))
                    continue;
                reaction = probe.SelectsShot ? hitbox.Shot : hitbox.Touch;
                return true;
            }
        }
        return false;
    
    }
}
