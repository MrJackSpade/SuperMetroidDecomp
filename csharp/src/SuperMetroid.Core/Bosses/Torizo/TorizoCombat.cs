using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb and Golden Torizo's hitboxes, touch and hurt, and what Samus's weapons do to them.</summary>
[RoomLifetime]
internal sealed class TorizoCombat
{
    /// <summary>Applies shot, bomb, and touch damage through the shared collision rules.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Bomb and Golden Torizo's body: its behaviour flags, palette, animation start and the low-health drool it triggers.</summary>
    private readonly TorizoBody _body;
    /// <summary>Bomb and Golden Torizo's state lookups, facing, posture and the function their branches test.</summary>
    private readonly TorizoEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal TorizoCombat(EnemyCollision collision, TorizoBody body, TorizoEncounterAccess access)
    {
        _collision = collision;
        _body = body;
        _access = access;
    }

    /// <summary>
    /// Dispatches an already-colliding normal bomb through Torizo's hitbox
    /// reactions. The frame-hitbox walker has already set
    /// its explosion direction bit; this method owns only the species reaction and its common
    /// no-death damage tail.
    /// </summary>
    private void ResolveTorizoNormalBomb(
        RoomEnemySlot torizo,
        SamusBombProjectileSlot bomb,
        EnemyReaction? selectedShotAi)
    {
        bool isBombTorizo = torizo.DefinitionId == EnemyDefinitionId.BombTorizo;
        bool isGoldenTorizo = torizo.DefinitionId == EnemyDefinitionId.GoldenTorizo;
        if (!isBombTorizo && !isGoldenTorizo)
            throw new ArgumentException("Normal-bomb Torizo dispatch requires a Torizo body.");

        TorizoEnemyState state = _access.RequireBombTorizoState(torizo);
        if (selectedShotAi == EnemyReaction.TorizoStandUpSitDownShot)
        {
            // The transitional reaction is a complete no-op in Bomb Torizo's area. Golden Torizo jumps to
            // a guarded path, which accepts common damage only outside the active flash and while
            // while the body's animation is unlocked. This transitional reaction never arms the later
            // counterattack flags or captures a projectile family.
            if (isBombTorizo || torizo.FlashTimer != 0 || state.AnimationLocked)
                return;

            _collision.ApplyNormalBombDamage(torizo, bomb);
            if (torizo.Health == 0)
                BeginBombTorizoDeath(torizo, state);
            return;
        }

        bool validShotReaction = isBombTorizo
            ? selectedShotAi == EnemyReaction.BombTorizoShot
            : selectedShotAi is EnemyReaction.BombTorizoShot or EnemyReaction.GoldenTorizoShot;
        if (!validShotReaction)
        {
            throw new InvalidDataException(
                $"Torizo has no response to normal-bomb reaction {selectedShotAi?.ToString() ?? "(unnamed)"}.");
        }

        // Both normal reactions return during the hurt flash. Bomb Torizo also uses its
        // awakening guard as a hard rejection; Golden's nonzero guard deliberately falls
        // through to common damage instead.
        if (torizo.FlashTimer != 0 || isBombTorizo && state.AnimationLocked)
            return;

        if (isGoldenTorizo && !state.AnimationLocked)
        {
            // A bomb is neither Missile nor Super Missile, so Golden Torizo's shot reaction takes its stun branch and arms the
            // stun latch before common damage, whether or not the Super-catch latch is set.
            SetBehavior(torizo, TorizoBehaviorFlags.Stunned);
        }

        _collision.ApplyNormalBombDamage(torizo, bomb);
        if (torizo.Health == 0)
            BeginBombTorizoDeath(torizo, state);
    }

    /// <summary>Restores the normal body colours on alternating Bomb Torizo hurt frames.</summary>
    private static void LoadTorizoDeathPalette(RoomEnemySlot torizo, TorizoEnemyState state) =>
        TorizoBody.LoadTorizoBodyColours(torizo, state, new TorizoLook(TorizoLookKind.Normal));

    /// <summary>Selects Golden Torizo's body and belly health band from its health, one band per 2048.</summary>
    private static void LoadGoldenTorizoHealthPalette(RoomEnemySlot torizo, TorizoEnemyState state, int health)
    {
        // The health gradient selects one of eight health bands from health divided by the band size, clamping
        // the upper half to the last band; each band resolves its colours from the installed health palettes.
        int healthBand = Ensure.AtLeastZero(health) / TorizoCombatDefinitions.HealthPerPaletteBand % TorizoCombatDefinitions.HealthPaletteBands;
        int band = Math.Min(healthBand, TorizoCombatDefinitions.LastHealthBand);
        TorizoBody.LoadTorizoBodyColours(torizo, state, new TorizoLook(TorizoLookKind.GoldenHealth, band));
    }

    /// <summary>Runs Bomb Torizo's custom hurt behaviour.</summary>
    internal void ApplyBombTorizoHurt(RoomEnemySlot torizo)
    {
        _body.MaybeSpawnBombTorizoLowHealthDrool(torizo);
        TorizoEnemyState state = _access.RequireBombTorizoState(torizo);
        if (!TorizoCadenceDefinitions.HurtWhiteShown.IsSetIn(torizo.FlashTimer))
        {
            if (torizo.DefinitionId == EnemyDefinitionId.GoldenTorizo)
                LoadGoldenTorizoHealthPalette(torizo, state, torizo.Health);
            else
                LoadTorizoDeathPalette(torizo, state);
        }
        else
        {
            TorizoBody.LoadTorizoBodyColours(torizo, state, new TorizoLook(TorizoLookKind.White));
        }
    }

    /// <summary>Enters the authored death list once and removes the dying statue from Samus collision.</summary>
    internal void BeginBombTorizoDeath(RoomEnemySlot torizo, TorizoEnemyState state)
    {
        if (state.DeathStarted)
            return;
        state.DeathStarted = true;
        state.Phase = TorizoPhase.Idle;
        TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(TorizoBodySegment.DeathSequence0));
        SetBehavior(torizo, TorizoBehaviorFlags.GutBlownUp | TorizoBehaviorFlags.FaceBlownUp);
        // The normal Torizo shot reaction makes the actor intangible when
        // entering the death stream. This removes the dying actor from Samus contact and
        // solid-enemy collision while its flashing/invisible animation frames continue.
        torizo.Contact = torizo.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>Latches <paramref name="flags"/>.</summary>
    internal void SetBehavior(RoomEnemySlot torizo, TorizoBehaviorFlags flags) => _access.RequireBombTorizoState(torizo).Behavior |= flags;

    /// <summary>Routes overlapping normal bombs through the selected Torizo hitbox reaction.</summary>
    internal void AddBombTorizoBombReactions(EnemySharedReactions table)
    {
        table.AddBombSpecial(c => c.Enemy.DefinitionId is EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo,
            c => ResolveTorizoNormalBomb(c.Enemy, c.Bomb, c.SelectedShotAi));
    }

    /// <summary>Validates extended hitbox reactions, handles catches, and defers death to Torizo's own sequence.</summary>
    internal void AddBombTorizoShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId is EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo &&
                                                     EnemyCollision.ShotAi(enemy) == (enemy.DefinitionId == EnemyDefinitionId.BombTorizo
                                                         ? EnemyReaction.BombTorizoShot
                                                         : EnemyReaction.GoldenTorizoShot))
        {
            // Golden's definition installs its own shot reaction, but its ordinary extended body hitboxes call the shared
            // one; that wrapper dispatches to Golden's by area. Stand-up/sit-down maps deliberately use the transitional reaction instead. That
            // reaction is a no-op for area-zero Bomb Torizo and a guarded direct-damage path for Golden Torizo, so it
            // must remain distinguishable below.
            AfterSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (!c.UsesFrameHitboxes || c.HitboxShotAi is not (
                        EnemyReaction.BombTorizoShot or EnemyReaction.TorizoStandUpSitDownShot))
                {
                    throw new InvalidDataException(
                        $"Torizo has no response to shot reaction {c.HitboxShotAi?.ToString() ?? "(unnamed)"}.");
                }
                return false;
            },
            BeforeImpact = ResolveTorizoShotBeforeImpact,
            ExemptFromGenericDeath = true,
            // Both Torizos' shot reactions run the common damage without the hit burst.
            ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
            AfterCommon = c =>
            {
                if (c.Enemy.Health == 0)
                    BeginBombTorizoDeath(c.Enemy, _access.RequireBombTorizoState(c.Enemy));
            },
        });
    }

    /// <summary>Registers extended body touch damage without invoking the generic death animation.</summary>
    internal void AddBombTorizoTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId is EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.BombTorizoTouch,
            context =>
            {
                if (!context.UsesFrameHitboxes || context.HitboxTouchAi != EnemyReaction.BombTorizoTouch)
                {
                    throw new InvalidDataException(
                        $"Torizo has no response to touch reaction {context.HitboxTouchAi?.ToString() ?? "(unnamed)"}.");
                }
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, deathAnimation: TouchDeathAnimation.Skip);
                if (context.Slot.Health == 0)
                    BeginBombTorizoDeath(context.Slot, _access.RequireBombTorizoState(context.Slot));
            });
    }

    /// <summary>
    /// Handles flash/awakening guards and Golden Torizo missile catches before shared
    /// shot damage; returns true when the hit was consumed by a special reaction.
    /// </summary>
    internal bool ResolveTorizoShotBeforeImpact(in EnemyShotContext c)
    {
        RoomEnemySlot enemy = c.Enemy;
        bool isBombTorizo = enemy.DefinitionId == EnemyDefinitionId.BombTorizo;
        bool isGoldenTorizo = !isBombTorizo;
        TorizoEnemyState torizoState = _access.RequireBombTorizoState(enemy);
        if (c.HitboxShotAi == EnemyReaction.TorizoStandUpSitDownShot)
        {
            // The transitional reaction returns immediately in area zero. In nonzero areas it calls a guarded path, which admits common
            // no-death shot damage only while the flash timer is zero and the body's animation is unlocked. Crucially it
            // never enters Golden's shot reaction, so missiles are not caught, Supers are not reflected, and the
            // counterattack flags are not armed through these transitional body rectangles.
            if (isBombTorizo || enemy.FlashTimer != 0 || torizoState.AnimationLocked)
            {
                c.Projectiles.EnemyImpacts.ApplyFrameHitboxCollisionPrelude(c.Projectile, c.MarksCollision);
                return true;
            }
        }
        if (enemy.FlashTimer != 0 || isBombTorizo && torizoState.AnimationLocked)
        {
            // Both reactions return during a damage flash. Bomb Torizo also rejects shots while its awakening guard
            // is set; Golden Torizo's nonzero guard instead routes through common damage below.
            c.Projectiles.EnemyImpacts.ApplyFrameHitboxCollisionPrelude(c.Projectile, c.MarksCollision);
            return true;
        }

        // A caught Super bypasses family selection, but still reaches the stun write before common damage.
        // Animation lock and the transitional hitbox reaction bypass that write instead.
        if (c.HitboxShotAi == EnemyReaction.BombTorizoShot && isGoldenTorizo &&
            !torizoState.AnimationLocked &&
            _body.HasBehavior(enemy, TorizoBehaviorFlags.CaughtSuperMissile))
            SetBehavior(enemy, TorizoBehaviorFlags.Stunned);

        if (c.HitboxShotAi == EnemyReaction.BombTorizoShot &&
            isGoldenTorizo && !torizoState.AnimationLocked &&
            !_body.HasBehavior(enemy, TorizoBehaviorFlags.CaughtSuperMissile))
        {
            if (c.Family == SamusProjectileFamily.Missile)
            {
                c.Projectiles.EnemyImpacts.ApplyFrameHitboxCollisionPrelude(c.Projectile, c.MarksCollision);
                c.Projectile.IsSpent = false;
                torizoState.Phase = TorizoPhase.Idle;
                enemy.AnimationClock.StepOnNextUpdate();
                _access.RequireBombTorizoState(enemy).BodyPlayback = TorizoAnimations.Start(_access.FacesRight(enemy)
                    ? TorizoBodySegment.GoldenDodgeTurningRight
                    : TorizoBodySegment.GoldenDodgeTurningLeft);
                return true;
            }

            if (c.Family == SamusProjectileFamily.SuperMissile)
            {
                if (c.Samus is not null && _access.FacesSamus(enemy, c.Samus))
                {
                    c.Projectiles.EnemyImpacts.ApplyFrameHitboxCollisionPrelude(c.Projectile, c.MarksCollision);
                    SetBehavior(enemy, TorizoBehaviorFlags.CaughtSuperMissile);
                    torizoState.Phase = TorizoPhase.Idle;
                    c.Projectile.IsSpent = true;
                    enemy.AnimationClock.StepOnNextUpdate();
                    _access.RequireBombTorizoState(enemy).BodyPlayback = TorizoAnimations.Start(_access.Posture(enemy).HasFlag(TorizoPostureFlags.OppositeLegForward)
                        ? _access.FacesRight(enemy)
                            ? TorizoBodySegment.GoldenCaughtSuperFacingRightLeftLegFwd
                            : TorizoBodySegment.GoldenCaughtSuperFacingLeftRightLegFwd
                        : _access.FacesRight(enemy)
                            ? TorizoBodySegment.GoldenCaughtSuperFacingRightRightLegFwd
                            : TorizoBodySegment.GoldenCaughtSuperFacingLeftLeftLegFwd);
                    return true;
                }
            }
            else
            {
                // Beams and other common-damage families arm the high-health counterattack branch before entering
                // normal shot AI.
                SetBehavior(enemy, TorizoBehaviorFlags.Stunned);
            }
        }
        return false;
    }

    /// <summary>
    /// Adds the Torizo hurt reaction for each variant; the common animation runner still
    /// advances after the hurt behaviour, as the enemies' update does.
    /// </summary>
    internal void AddBombTorizoHurtAi(EnemySharedReactions table) =>
        table.AddHurt(slot => slot.DefinitionId is EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo,
            (slot, _) => ApplyBombTorizoHurt(slot));

    /// <summary>The reaction of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindTorizoHitboxReaction(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        if (enemy.DefinitionId is not (EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo))
            return null;

        foreach (GoldenTorizoCollisionComponent component in
                 TorizoCollision.ComponentsAt(enemy.Frame))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            IReadOnlyList<EnemyCollisionHitbox> rectangles = component.Hitboxes.Rectangles;
            for (int index = 0; index < rectangles.Count; index++)
            {
                EnemyCollisionHitbox hitbox = rectangles[index];
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
