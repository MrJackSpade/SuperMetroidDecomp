using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's hitboxes, touch and hurt, and what Samus's weapons do to it.</summary>
[RoomLifetime]
internal sealed class DraygonCombat
{
    /// <summary>Allocates Draygon's goop, eye drops, and other enemy projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Evaluates Samus and projectile contacts against Draygon's linked parts.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Draygon's body, eye and tail: their setup, animations and cues.</summary>
    private readonly DraygonParts _parts;
    /// <summary>Draygon grabbing Samus: the chase, the spiral carry, the tail whips and the release.</summary>
    private readonly DraygonGrab _grab;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonCombat(EnemyProjectileEngine projectiles, EnemyCollision collision, DraygonParts parts, DraygonGrab grab)
    {
        _projectiles = projectiles;
        _collision = collision;
        _parts = parts;
        _grab = grab;
    }

    /// <summary>Recognizes the four enemy definitions that form one Draygon encounter.</summary>
    internal static bool IsDraygonDefinition(EnemyDefinitionId definition) => definition is
        EnemyDefinitionId.DraygonBody or EnemyDefinitionId.DraygonEye or
        EnemyDefinitionId.DraygonTail or EnemyDefinitionId.DraygonArms;

    /// <summary>Returns the encounter state owned by <paramref name="body"/>; each part property requires its linked slot when read.</summary>
    internal DraygonEnemyState RequireCompleteDraygonState(RoomEnemySlot body)
    {
        DraygonEnemyState state = _parts.Draygon ??
            throw new InvalidDataException("Draygon body has no encounter state.");
        if (!ReferenceEquals(state.Body, body))
            throw new InvalidDataException("Draygon's encounter state belongs to a different body slot.");
        return state;
    }

    /// <summary>Routes body bomb hits through Draygon's special shot reaction.</summary>
    internal void AddDraygonBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.DraygonBody &&
                                                       c.SelectedShotAi == EnemyReaction.DraygonShot)
        {
            RunsGenericDeath = false,
            BeforeCommon = c => AdvanceDraygonShotAcceleration(RequireCompleteDraygonState(c.Enemy)),
            AfterCommon = c => ResolveDraygonReaction(c.Enemy, c.Samus),
        });
    }

    /// <summary>Routes power-bomb damage for Draygon's body through its encounter reaction.</summary>
    internal void AddDraygonPowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.DraygonBody &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyReaction.DraygonPowerBomb)
        {
            ExemptFromGenericDeath = true,
            AfterCommon = (enemy, samus) => ResolveDraygonReaction(enemy, samus),
        });
    }

    /// <summary>Registers the body and linked-part contact behavior against Samus.</summary>
    internal void AddDraygonTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.DraygonBody &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.DraygonTouch,
            context =>
            {
                if (!context.UsesFrameHitboxes)
                    throw new InvalidDataException("Draygon requires his authored extended touch hitboxes.");
                if (context.HitboxTouchAi == EnemyReaction.Ignore)
                    return; // The vulnerable eye rectangle is harmless to Samus.
                if (context.HitboxTouchAi != EnemyReaction.DraygonTouch)
                {
                    throw new InvalidDataException(
                        $"Draygon hitbox touch AI {context.HitboxTouchAi?.ToString() ?? "(unnamed)"} is not translated.");
                }
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, deathAnimation: TouchDeathAnimation.Skip);
                ResolveDraygonReaction(context.Slot, context.Samus);
            });
    }

    /// <summary>
    /// Runs Draygon's hurt AI. The body is drawn partly as a background layer and partly
    /// as sprites, so a valid flash must update the colours of both surfaces.
    /// </summary>
    internal void ApplyDraygonHurt(
        RoomEnemySlot body,
        DraygonEnemyState state,
        SamusState? samus)
    {
        bool whiteFrame = DraygonCadenceDefinitions.HurtWhiteFlashShown.IsSetIn(body.FlashTimer);
        // A hurt update shows his material colours with his band, or their white flash, as the import's shades (#1512).
        state.Colours.ShowHurt(whiteFrame);
        if (!whiteFrame)
            state.Colours.ShowBand(RequireHealthBand(state));
        state.ShadeSprites();

        // A grapple connected to the turrets is the game's electrocution channel. It is
        // sampled only while hurt AI owns the body and subtracts exactly 256 HP on every
        // eighth body frame before running the same fatal/nonfatal reaction as a shot.
        if (samus is null || !DraygonGrab.IsDraygonGrappleConnected(samus) ||
            !DraygonCadenceDefinitions.GrappleShockEveryEighthUpdate.IsDue(body.FrameCounter))
            return;

        body.Health = body.Health <= 256
            ? 0
            : body.Health - 256;
        ResolveDraygonReaction(body, samus);
    }

    /// <summary>
    /// The vulnerable eye's shot reaction increases future swoop curvature before common shot AI.
    /// The comparison keeps the stored value from ever advancing beyond 152.
    /// </summary>
    internal static void AdvanceDraygonShotAcceleration(DraygonEnemyState state)
    {
        EightEightVector<RoomPixels> candidate = state.SwoopAcceleration + DraygonBehavior.DraygonShotSwoopAccelerationStep;
        if (candidate.Y < DraygonBehavior.DraygonSwoopAccelerationLimit)
            state.SwoopAcceleration = candidate;
    }

    /// <summary>Runs the common post-touch/shot/power-bomb reaction.</summary>
    internal void ResolveDraygonReaction(RoomEnemySlot body, SamusState? samus)
    {
        DraygonEnemyState state = RequireCompleteDraygonState(body);
        if (body.Health != 0)
        {
            UpdateDraygonHealthBand(state);
            return;
        }

        BeginDraygonDeath(state, samus);
    }

    /// <remarks>
    /// Draygon's shared reaction has no "already dying" test in its dead branch. Every
    /// touch, shot or power-bomb reaction on a zero-health body runs it again: the dying
    /// arm and eye animations restart, the drift function is reinstalled and Samus is released.
    /// </remarks>
    private void BeginDraygonDeath(DraygonEnemyState state, SamusState? samus)
    {
        RoomEnemySlot body = state.Body;
        samus?.Grapple.Phase = GrapplePhase.Dropped;
        _parts.StartDraygonAnimation(
            body,
            state.FacingRight
                ? DraygonSegment.ArmsFacingRightDying
                : DraygonSegment.ArmsFacingLeftDying);
        _parts.StartDraygonAnimation(
            state.Eye,
            state.FacingRight
                ? DraygonSegment.EyeFacingRightDying
                : DraygonSegment.EyeFacingLeftDying);
        (DraygonParts.RequirePart(state.Eye)).RequireSpeciesState<DraygonEyeState>().Function = DraygonEyeFunction.Inert;
        state.Phase = DraygonPhase.Dying;

        // The death behaviour releases Samus unconditionally, even during an
        // unrelated movement handler. In particular, this can interrupt a shinespark.
        // The caller then clears Samus's grapple flags, including the owner-release signal.
        if (samus is not null)
        {
            _grab.ReleaseSamusDuringEnemyMain(samus);
            samus.DraygonGrabbed.ConsumeOwnerReleaseSignal();
            samus.Grapple.Phase = GrapplePhase.Dropped;
        }
    }

    /// <summary>Changes Draygon's health color band only when the health threshold changes.</summary>
    internal static void UpdateDraygonHealthBand(DraygonEnemyState state)
    {
        int band = DraygonHealthPalette.BandForHealth(state.Body.Health);

        if (band == state.HealthBand)
            return;
        state.HealthBand = band;
        ShowDraygonHealthBand(state);
    }

    /// <summary>The installed health band; code reads it only after initialization installs one.</summary>
    private static int RequireHealthBand(DraygonEnemyState state) => state.HealthBand
        ?? throw new InvalidOperationException("Draygon has no installed health palette band.");

    /// <summary>Shows his selected health band's colours.</summary>
    internal static void ShowDraygonHealthBand(DraygonEnemyState state)
    {
        state.Colours.ShowBand(RequireHealthBand(state));
    }

    /// <summary>A bomb striking one of Draygon's body dud-shot hitboxes: the dud flash where the bomb lies and the dud sound.</summary>
    internal void CreateBombDudShot(EnemyBombContext context) => _projectiles.CreateBombDudShot(context.Bomb);

    /// <summary>Registers Draygon's shot response, including acceleration and special death exemption.</summary>
    internal void AddDraygonShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.DraygonBody &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.DraygonShot)
        {
            BeforeSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (!c.UsesFrameHitboxes)
                    throw new InvalidDataException("Draygon requires his authored extended shot hitboxes.");
                if (c.HitboxShotAi == EnemyReaction.DudShot)
                {
                    // Body, claws, and shell use the shared common touch; only the first
                    // eye rectangle of the two eye-showing body frames reaches Draygon's damage reaction.
                    c.Projectiles.EnemyImpacts.ApplyFrameHitboxCollisionPrelude(c.Projectile, c.MarksCollision);
                    _projectiles.CreateEnemyProjectileDudShot(c.Projectile);
                    return true;
                }
                if (c.HitboxShotAi != EnemyReaction.DraygonShot)
                {
                    throw new InvalidDataException(
                        $"Draygon hitbox shot AI {c.HitboxShotAi?.ToString() ?? "(unnamed)"} is not translated.");
                }
                return false;
            },
            AfterImpact = c => AdvanceDraygonShotAcceleration(RequireCompleteDraygonState(c.Enemy)),
            ExemptFromGenericDeath = true,
            AfterCommon = c => ResolveDraygonReaction(c.Enemy, c.Samus),
        });
    }

    /// <summary>The reaction of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindDraygonHitboxReaction(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        if (!DraygonCombat.IsDraygonDefinition(enemy.DefinitionId))
            return null;

        // Both sprite and body-layer art are replaceable presentation. Their
        // component offsets and hitboxes remain fixed gameplay data.
        foreach (EnemyCollisionComponent component in
                 DraygonCollision.ComponentsAt(enemy.Frame))
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
