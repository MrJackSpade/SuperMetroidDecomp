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
    /// <summary>Provides room scratch memory, tiles, palette state, and effects used by the encounter.</summary>
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
                                  c.SelectedShotAi == EnemyInteractionCallback.MotherBrainHeadShot,
            c => ResolveMotherBrainHeadNormalBomb(c.Enemy, c.Bomb));
    }

    /// <summary>Binds head and body shot responses for each combat phase.</summary>
    internal void AddMotherBrainShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.MotherBrainBody &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.MotherBrainBodyShot)
        {
            BeforeSuperMissileQuake = (in EnemyShotContext c) =>
            {
                // Every body rectangle's shot callback is `$A9:B503`, exactly CreateDudShot;
                // only the dummy root's head-callback list could select anything else.
                if (c.HitboxShotAi != EnemyInteractionCallback.MotherBrainBodyShot)
                {
                    throw new InvalidDataException(
                        $"Mother Brain's body selected shot AI {c.HitboxShotAi?.ToString() ?? "(unnamed)"} from " +
                        $"frame $A9:{c.Enemy.Frame}; only $A9:B503 is ported.");
                }
                // The common collision prelude still owns Super-Missile quake first.
                c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                _projectiles.CreateEnemyProjectileDudShot(c.Projectile);
                return true;
            },
        });

        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.MotherBrainHead &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.MotherBrainHeadShot)
        {
            BeforeSuperMissileQuake = (in EnemyShotContext c) =>
            {
                ResolveMotherBrainHeadShot(c.Enemy, c.Projectile, c.Projectiles, c.ProjectileType, c.ProjectileDamage);
                return true;
            },
        });
    }

    /// <summary>
    /// Ports the first-form branch of head shot AI <c>$A9:B507</c>. The glass encounter
    /// accepts only missile families; beams pass through without being converted into an
    /// impact, while every accepted missile increments the room PLM argument before common
    /// vulnerability damage is applied without the ordinary enemy-deletion tail.
    /// </summary>
    private void ResolveMotherBrainHeadShot(
        RoomEnemySlot head,
        SamusProjectileSlot projectile,
        SamusProjectileSystem projectiles,
        SamusProjectileTypeWord projectileType,
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

        // The bank-$A0 walker has marked every overlapping family before this callback;
        // the next projectile pass consumes the marker, removing a Super Missile's pair too.
        projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(
            projectile.SlotIndex,
            head.Properties.HasAny(EnemyProperties.BlocksPlasmaBeam) ||
                !projectileType.BeamCombination.HasPlasma);
        // `$A9:B519` lets only missiles and Super Missiles reach the glass.
        if (family is not (SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile))
            return;

        _room.Services.MotherBrain.IncrementGlassRoomArgument();
        _audio.QueueEnemySound(Library2Sound.ShotMotherBrain, maximumQueued: SoundQueueLimit.Max6);

        // `$B52D-$B539` deliberately alternates odd/even flash parity when a second hit
        // arrives during the existing flash. The common no-death damage helper then owns
        // health, hurt-AI, and the final ROM-authored hurt duration.
        head.FlashTimer = WrappingCountdown.FromRaw(!head.FlashTimer.IsZero && (head.FlashTimer.RawValue & 1) != 0
            ? (ushort)14
            : (ushort)13);

        byte vulnerability = EnemyCollision.ReadProjectileVulnerability(head, projectileType);
        if (vulnerability == byte.MaxValue)
        {
            head.FrozenTimer = WrappingCountdown.Start(400);
            head.AiStates = head.AiStates | EnemyAiStates.Frozen;
            head.InvincibilityTimer = WrappingCountdown.Start(10);
            return;
        }

        int damage = (projectileDamage >> 1) * (vulnerability & 127);
        if (damage == 0)
        {
            _projectiles.CreateEnemyProjectileDudShot(projectile);
            return;
        }

        head.FlashTimer = WrappingCountdown.Start(head.HurtFlashFrames + 8);
        head.AiStates = head.AiStates | EnemyAiStates.Hurt;
        head.Health = damage >= head.Health
            ? 0
            : head.Health - damage;
    }

    /// <summary>
    /// Ports <c>$A9:B54E-B5C4</c> plus the phase-two/three common no-death damage tail.
    /// Form one (the fake-death interval) still runs the recoil bookkeeping but converts the
    /// projectile into a dud. Forms two and later apply ordinary vulnerability damage while
    /// deliberately leaving the multipart boss records alive at zero health for body AI.
    /// </summary>
    internal void ResolveMotherBrainLaterFormHeadShot(
        MotherBrainEnemyState state,
        RoomEnemySlot head,
        SamusProjectileSlot projectile,
        SamusProjectileSystem projectiles,
        SamusProjectileTypeWord projectileType,
        int projectileDamage)
    {
        // The bank-$A0 walker marked the projectile before dispatching this callback.
        projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(
            projectile.SlotIndex,
            head.Properties.HasAny(EnemyProperties.BlocksPlasmaBeam) ||
                !projectileType.BeamCombination.HasPlasma);

        // `$B58E` indexes its reaction table with the type word's high byte masked to three
        // bits, so missile explosions (`$08`) alias beams exactly as natively.
        var reactionProjectile = (MotherBrainProjectileType)((SnesLittleEndian.HighByte(projectileType.Raw)) & 7);
        if (state.RainbowBeamSequence is { } sequence)
        {
            // The attached sequence shares walk counter `$7E:780E` with the encounter. `$B5BA/$B5BD`
            // clears the body function timer on recoil underflow and `$B5C0` stores the counter
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

        // The native callback tail-calls common no-death shot damage. In particular,
        // charged beams use the separate charged vulnerability byte after the ordinary
        // beam freeze check; Hyper would otherwise inherit the immune plasma entry.
        EnemyCollision.NormalShotVulnerability vulnerability = EnemyCollision.ReadNormalShotVulnerability(head, projectileType);
        if (vulnerability.FreezeImmediately)
        {
            head.FrozenTimer = WrappingCountdown.Start(400);
            head.AiStates = head.AiStates | EnemyAiStates.Frozen;
            head.InvincibilityTimer = WrappingCountdown.Start(10);
            return;
        }

        int damage = (projectileDamage >> 1) * vulnerability.Multiplier;
        if (damage == 0)
        {
            _projectiles.CreateEnemyProjectileDudShot(projectile);
            return;
        }

        head.FlashTimer = WrappingCountdown.Start(head.HurtFlashFrames + 8);
        head.AiStates = head.AiStates | EnemyAiStates.Hurt;
        // The shared native no-death tail still grants Plasma's hit immunity.
        // A surviving penetrating beam cannot damage this head again every frame.
        if (projectileType.BeamCombination.HasPlasma)
            head.InvincibilityTimer = WrappingCountdown.Start(EnemyShotTiming.PlasmaInvincibilityFrames);
        head.Health = damage >= head.Health
            ? 0
            : head.Health - damage;
    }

    /// <summary>
    /// Ports family <c>$0500</c> through Mother Brain brain callback <c>$A9:B507</c>.
    /// The first form's eight-entry projectile-family table rejects bombs outright. Later
    /// forms always run <c>$B562</c>: family five selects reaction zero, subtracting
    /// <c>$0100</c> from the walk/recoil accumulator with a signed-zero clamp. Fake-death
    /// form one then creates only a dud, while forms two and later enter common no-death
    /// damage. The bank-$A0 multibox caller already owns the bomb collision mark.
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

        // `$A9:B57C`: a bomb is reaction type zero, which spends one step and empties the credit when the result is
        // negative as a signed word.
        state.WalkCounter = state.WalkCounter.SpendForShot(MotherBrainWalkCredit.StepCredit);
        if (state.Form == MotherBrainForm.FakeDeath)
            return;

        _collision.ApplyCommonNormalBombDamage(head, bomb, runGenericDeath: false);
    }

    /// <summary>Registers the head-touch reaction for the active body/head pair.</summary>
    internal static void AddMotherBrainHeadTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.MotherBrainHead &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.MotherBrainHeadTouch,
            context =>
            {
                // `$A9:B5C6` is not ordinary touch damage. A spin-jumping Samus toggles
                // the head's visible flash length between the cartridge's 13/14-frame
                // cadence; every other movement type returns without changing either actor.
                RoomEnemySlot slot = context.Slot;
                if (SamusPoseMetadata.ReadMovementType(context.Samus) == SamusMovementType.SpinJumping)
                    slot.FlashTimer = WrappingCountdown.FromRaw(!slot.FlashTimer.IsZero && (slot.FlashTimer.RawValue & 1) != 0
                        ? (ushort)14
                        : (ushort)13);
            });
    }

    /// <summary>Registers body bomb reactions in the native AI dispatch table.</summary>
    /// <remarks>`$A9:B503` is `CreateDudShot`. The multibox bomb walker has already marked the physical explosion, and
    /// the body callback performs no health, phase, or multipart-state work of its own.</remarks>
    internal static void AddMotherBrainBodyBombReactions(EnemySharedReactions table)
    {
        table.AddBombSpecial(c => c.Enemy.DefinitionId == EnemyDefinitionId.MotherBrainBody &&
                                  c.SelectedShotAi == EnemyInteractionCallback.MotherBrainBodyShot,
            static _ => { });
    }

    /// <summary>The callback of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindMotherBrainHitboxCallback(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyInteractionCallback? callback)
    {
        callback = null;
        if (enemy.DefinitionId is not (EnemyDefinitionId.MotherBrainBody or EnemyDefinitionId.MotherBrainHead))
            return null;

        // Body and head walk the same bank-$A9 roots; the head's dummy list keeps $A320.
        foreach (MotherBrainCollisionComponent component in
                 MotherBrainCollisionDefinitions.ComponentsAt(enemy.Frame))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            ushort componentX = componentOrigin.X;
            ushort componentY = componentOrigin.Y;
            foreach (MotherBrainCollisionHitbox hitbox in component.Hitboxes)
            {
                ushort left = unchecked((ushort)(componentX + hitbox.Left));
                ushort top = unchecked((ushort)(componentY + hitbox.Top));
                ushort right = unchecked((ushort)(componentX + hitbox.Right));
                ushort bottom = unchecked((ushort)(componentY + hitbox.Bottom));
                if (!probe.Overlaps(left, top, right, bottom))
                    continue;
                callback = probe.SelectShotCallback
                    ? MotherBrainHitboxCallbackCodec.ToCallback(hitbox.Shot)
                    : MotherBrainHitboxCallbackCodec.ToCallback(hitbox.Touch);
                return true;
            }
        }
        return false;
    
    }
}
