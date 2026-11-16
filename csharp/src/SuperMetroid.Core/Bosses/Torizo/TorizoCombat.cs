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
    /// <summary>Bomb and Golden Torizo's body: its behaviour flags, colours, animation start and the low-health drool it triggers.</summary>
    private readonly TorizoBody _body;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal TorizoCombat(EnemyCollision collision, TorizoBody body)
    {
        _collision = collision;
        _body = body;
    }

    /// <summary>
    /// Runs an already-colliding normal bomb through Torizo's hitbox
    /// reactions. The frame-hitbox collision has already set
    /// the bomb's explosion direction; this method owns only the species reaction and its common
    /// no-death damage.
    /// </summary>
    private void ResolveTorizoNormalBomb(
        RoomEnemyPlace torizo,
        SamusBomb bomb,
        EnemyReaction? selectedShot)
    {
        bool isBombTorizo = torizo.DefinitionId == EnemyDefinitionId.BombTorizo;
        bool isGoldenTorizo = torizo.DefinitionId == EnemyDefinitionId.GoldenTorizo;
        if (!isBombTorizo && !isGoldenTorizo)
            throw new ArgumentException("A normal bomb's Torizo reaction requires a Torizo body.");

        TorizoEnemyState state = torizo.RequireSpeciesState<TorizoEnemyState>();
        if (selectedShot == TorizoReactions.StandUpSitDownShot)
        {
            // The transitional reaction is a complete no-op in Bomb Torizo's area. Golden Torizo jumps to
            // a guarded path, which accepts common damage only outside the active flash and while
            // while the body's animation is unlocked. This transitional reaction never arms the later
            // counterattack flags or captures a projectile family.
            if (isBombTorizo || torizo.FlashTimer != 0 || state.AnimationLocked)
                return;

            _collision.ApplyNormalBombDamage(torizo, bomb);
            if (torizo.Health == 0)
                BeginTorizoDeath(torizo, state);
            return;
        }

        bool validShotReaction = isBombTorizo
            ? selectedShot == TorizoReactions.Shot
            : (selectedShot == TorizoReactions.Shot || selectedShot == TorizoReactions.GoldenShot);
        if (!validShotReaction)
        {
            throw new InvalidDataException(
                $"Torizo has no response to normal-bomb reaction {selectedShot?.ToString() ?? "(unnamed)"}.");
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
            BeginTorizoDeath(torizo, state);
    }

    /// <summary>Restores the normal body colours on alternating Bomb Torizo hurt frames.</summary>
    private static void RestoreTorizoNormalColours(RoomEnemyPlace torizo, TorizoEnemyState state) =>
        TorizoBody.LoadTorizoBodyColours(torizo, state, new TorizoLook(TorizoLookKind.Normal));

    /// <summary>Selects Golden Torizo's body and belly health band from its health, one band per 2048.</summary>
    private static void ShowGoldenTorizoHealthShade(RoomEnemyPlace torizo, TorizoEnemyState state, int health)
    {
        // The health gradient selects a band from health divided by the band size, clamping it to the last band; each
        // band resolves its colours from the installed health shades.
        int healthBand = Ensure.AtLeastZero(health) / TorizoCombatDefinitions.HealthPerBand;
        int band = Math.Min(healthBand, TorizoCombatDefinitions.LastHealthBand);
        TorizoBody.LoadTorizoBodyColours(torizo, state, new TorizoLook(TorizoLookKind.GoldenHealth, band));
    }

    /// <summary>Runs Bomb Torizo's custom hurt behaviour.</summary>
    internal void ApplyTorizoHurt(RoomEnemyPlace torizo)
    {
        _body.MaybeSpawnBombTorizoLowHealthDrool(torizo);
        TorizoEnemyState state = torizo.RequireSpeciesState<TorizoEnemyState>();
        if (!TorizoCadenceDefinitions.HurtWhiteShown.IsSetIn(torizo.FlashTimer))
        {
            if (torizo.DefinitionId == EnemyDefinitionId.GoldenTorizo)
                ShowGoldenTorizoHealthShade(torizo, state, torizo.Health);
            else
                RestoreTorizoNormalColours(torizo, state);
        }
        else
        {
            TorizoBody.LoadTorizoBodyColours(torizo, state, new TorizoLook(TorizoLookKind.White));
        }
    }

    /// <summary>Enters the authored death list once and removes the dying statue from Samus collision.</summary>
    internal static void BeginTorizoDeath(RoomEnemyPlace torizo, TorizoEnemyState state)
    {
        if (state.DeathStarted)
            return;
        state.DeathStarted = true;
        state.Phase = TorizoPhase.Idle;
        TorizoBody.StartTorizoBodyAnimation(torizo, state, TorizoAnimations.Start(TorizoAnimation.Dying));
        SetBehavior(torizo, TorizoBehaviorFlags.GutBlownUp | TorizoBehaviorFlags.FaceBlownUp);
        // The normal Torizo shot reaction makes the actor intangible when
        // entering the death animation. This removes the dying actor from Samus contact and
        // solid-enemy collision while its flashing/invisible animation frames continue.
        torizo.Contact = torizo.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>Latches <paramref name="flags"/>.</summary>
    internal static void SetBehavior(RoomEnemyPlace torizo, TorizoBehaviorFlags flags) => torizo.RequireSpeciesState<TorizoEnemyState>().Behavior |= flags;

    /// <summary>Routes overlapping normal bombs through the selected Torizo hitbox reaction.</summary>
    internal void ReactToTorizoBomb(EnemyBombContext c) =>
        ResolveTorizoNormalBomb(c.Enemy, c.Bomb, c.SelectedShot);

    /// <summary>Validates extended hitbox reactions, handles catches, and defers death to Torizo's own sequence.</summary>
    internal EnemyShotReaction TorizoShotReaction => field ??= new()
    {
        // Golden's definition installs its own shot reaction, but its ordinary extended body hitboxes call the shared
        // one, which hands Golden's area to Golden's own. Stand-up/sit-down frames deliberately use the transitional reaction instead. That
        // reaction is a no-op for Crateria's Bomb Torizo and a guarded direct-damage path for Golden Torizo, so it
        // must remain distinguishable below.
        AfterSuperMissileQuake = (in EnemyShotContext c) =>
        {
            if (!c.UsesFrameHitboxes || (c.HitboxShot != TorizoReactions.Shot && c.HitboxShot != TorizoReactions.StandUpSitDownShot))
            {
                throw new InvalidDataException(
                    $"Torizo has no response to shot reaction {c.HitboxShot?.ToString() ?? "(unnamed)"}.");
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
                BeginTorizoDeath(c.Enemy, c.Enemy.RequireSpeciesState<TorizoEnemyState>());
        },
    };

    /// <summary>Runs extended body touch damage without invoking the generic death animation.</summary>
    internal void ReactToTorizoTouch(EnemyTouchContext context)
    {
        if (!context.UsesFrameHitboxes || context.HitboxTouch != TorizoReactions.Touch)
        {
            throw new InvalidDataException(
                $"Torizo has no response to touch reaction {context.HitboxTouch?.ToString() ?? "(unnamed)"}.");
        }
        _collision.ResolveNormalEnemyTouch(context, TouchDeathAnimation.Skip);
        if (context.Place.Health == 0)
            BeginTorizoDeath(context.Place, context.Place.RequireSpeciesState<TorizoEnemyState>());
    }

    /// <summary>
    /// Handles flash/awakening guards and Golden Torizo missile catches before shared
    /// shot damage; returns true when the hit was consumed by a special reaction.
    /// </summary>
    internal bool ResolveTorizoShotBeforeImpact(in EnemyShotContext c)
    {
        RoomEnemyPlace enemy = c.Enemy;
        bool isBombTorizo = enemy.DefinitionId == EnemyDefinitionId.BombTorizo;
        bool isGoldenTorizo = !isBombTorizo;
        TorizoEnemyState torizoState = enemy.RequireSpeciesState<TorizoEnemyState>();
        if (c.HitboxShot == TorizoReactions.StandUpSitDownShot)
        {
            // The transitional reaction returns immediately in Crateria. In every other area it calls a guarded path, which admits common
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
        if (c.HitboxShot == TorizoReactions.Shot && isGoldenTorizo &&
            !torizoState.AnimationLocked &&
            TorizoBody.HasBehavior(enemy, TorizoBehaviorFlags.CaughtSuperMissile))
            SetBehavior(enemy, TorizoBehaviorFlags.Stunned);

        if (c.HitboxShot == TorizoReactions.Shot &&
            isGoldenTorizo && !torizoState.AnimationLocked &&
            !TorizoBody.HasBehavior(enemy, TorizoBehaviorFlags.CaughtSuperMissile))
        {
            if (c.Family == SamusProjectileFamily.Missile)
            {
                c.Projectiles.EnemyImpacts.ApplyFrameHitboxCollisionPrelude(c.Projectile, c.MarksCollision);
                c.Projectile.IsSpent = false;
                torizoState.Phase = TorizoPhase.Idle;
                enemy.AnimationClock.StepOnNextUpdate();
                enemy.RequireSpeciesState<TorizoEnemyState>().BodyPlayback = TorizoAnimations.Start(TorizoFacing.FacesRight(enemy)
                    ? TorizoAnimation.GoldenDodgeTurningRight
                    : TorizoAnimation.GoldenDodgeTurningLeft);
                return true;
            }

            if (c.Family == SamusProjectileFamily.SuperMissile)
            {
                if (TorizoFacing.FacesSamus(enemy, c.Samus))
                {
                    c.Projectiles.EnemyImpacts.ApplyFrameHitboxCollisionPrelude(c.Projectile, c.MarksCollision);
                    SetBehavior(enemy, TorizoBehaviorFlags.CaughtSuperMissile);
                    torizoState.Phase = TorizoPhase.Idle;
                    c.Projectile.IsSpent = true;
                    enemy.AnimationClock.StepOnNextUpdate();
                    enemy.RequireSpeciesState<TorizoEnemyState>().BodyPlayback = TorizoAnimations.Start(TorizoFacing.Posture(enemy).LeadsWithFarFoot
                        ? TorizoFacing.FacesRight(enemy)
                            ? TorizoAnimation.GoldenFacingRightCatchingSuperMissileLeftLegForward
                            : TorizoAnimation.GoldenFacingLeftCatchingSuperMissileRightLegForward
                        : TorizoFacing.FacesRight(enemy)
                            ? TorizoAnimation.GoldenFacingRightCatchingSuperMissileRightLegForward
                            : TorizoAnimation.GoldenFacingLeftCatchingSuperMissileLeftLegForward);
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

    /// <summary>The reaction of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindTorizoHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        if (enemy.DefinitionId is not (EnemyDefinitionId.BombTorizo or EnemyDefinitionId.GoldenTorizo))
            return null;

        // The body's rectangles lie about Torizo's origin; the probe compares coordinates by their difference, so a
        // rectangle edge computed past either end of the room compares as the wrapped coordinate would.
        Position16<RoomPixels> origin = enemy.Position.Whole;
        IReadOnlyList<EnemyCollisionHitbox> rectangles = TorizoCollision.HitboxesAt(enemy.Frame).Rectangles;
        for (int index = 0; index < rectangles.Count; index++)
        {
            EnemyCollisionHitbox hitbox = rectangles[index];
            if (!probe.Overlaps(origin.X + hitbox.Left, origin.Y + hitbox.Top, origin.X + hitbox.Right, origin.Y + hitbox.Bottom))
                continue;
            reaction = probe.SelectsShot ? hitbox.Shot : hitbox.Touch;
            return true;
        }
        return false;
    }
}
