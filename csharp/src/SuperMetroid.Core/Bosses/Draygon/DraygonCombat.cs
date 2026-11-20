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
    /// <summary>Draygon grabbing Samus: the chase, the spiral carry, the tail whips and the release.</summary>
    private readonly DraygonGrab _grab;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonCombat(EnemyProjectileEngine projectiles, EnemyCollision collision, DraygonGrab grab)
    {
        _projectiles = projectiles;
        _collision = collision;
        _grab = grab;
    }

    /// <summary>Routes body bomb hits through Draygon's special shot reaction.</summary>
    internal EnemyBombReaction DraygonBombReaction => field ??= new()
    {
        RunsGenericDeath = false,
        // Draygon's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyBombReaction.NeverShowsHitBurst,
        BeforeCommon = c => AdvanceDraygonShotAcceleration(DraygonEntity.EncounterOf(c.Enemy)),
        AfterCommon = c => ResolveDraygonReaction(c.Enemy, c.Samus),
    };

    /// <summary>Routes power-bomb damage for Draygon's body through its encounter reaction.</summary>
    internal EnemyPowerBombReaction DraygonPowerBombReaction => field ??= new()
    {
        ExemptFromGenericDeath = true,
        AfterCommon = c => ResolveDraygonReaction(c.Enemy, c.Samus),
    };

    /// <summary>Runs the body's contact against Samus.</summary>
    internal void ReactToDraygonTouch(EnemyTouchContext context)
    {
        if (!context.UsesFrameHitboxes)
            throw new InvalidDataException("Draygon requires his authored extended touch hitboxes.");
        if (context.HitboxTouch == EnemyReaction.Ignore)
            return; // The vulnerable eye rectangle is harmless to Samus.
        if (context.HitboxTouch != DraygonReactions.Touch)
        {
            throw new InvalidDataException(
                $"Draygon has no response to touch reaction {context.HitboxTouch?.ToString() ?? "(unnamed)"}.");
        }
        _collision.ResolveOrdinaryTouch(context, TouchDeathAnimation.Skip);
        ResolveDraygonReaction(context.Place, context.Samus);
    }

    /// <summary>
    /// Updates Draygon while he is hurt: the hurt flash shows on his body and his sprites alike.
    /// </summary>
    internal void ApplyDraygonHurt(
        RoomEnemyPlace body,
        DraygonEnemyState state,
        SamusState samus)
    {
        bool whiteFrame = DraygonCadenceDefinitions.HurtWhiteFlashShown.IsSetIn(body.HurtFlash.UpdatesLeft);
        // A hurt update shows his material colours with his band, or their white flash, as the import's shades (#1512).
        state.Colours.ShowHurt(whiteFrame);
        if (!whiteFrame)
            state.Colours.ShowBand(state.HealthBand);
        state.ShadeSprites();

        // A grapple connected to the turrets electrocutes him. It is sampled only while he
        // is hurt and subtracts its grapple-shock damage at its shock cadence before
        // running the same fatal/nonfatal reaction as a shot.
        if (!DraygonGrab.IsDraygonGrappleConnected(samus) ||
            !DraygonCadenceDefinitions.GrappleShock.IsDue(body.FrameCounter))
            return;

        body.Health = body.Health <= DraygonArenaDefinitions.GrappleShockDamage
            ? 0
            : body.Health - DraygonArenaDefinitions.GrappleShockDamage;
        ResolveDraygonReaction(body, samus);
    }

    /// <summary>
    /// The vulnerable eye's shot reaction increases future swoop curvature before the shot's common damage; the
    /// curvature never grows past <see cref="DraygonSwoopDefinitions.AccelerationLimit"/>.
    /// </summary>
    private static void AdvanceDraygonShotAcceleration(DraygonEnemyState state)
    {
        SignedEightEight candidate = state.SwoopAcceleration + DraygonSwoopDefinitions.ShotAccelerationStep;
        if (candidate < DraygonSwoopDefinitions.AccelerationLimit)
            state.SwoopAcceleration = candidate;
    }

    /// <summary>Runs the common post-touch/shot/power-bomb reaction.</summary>
    private void ResolveDraygonReaction(RoomEnemyPlace body, SamusState samus)
    {
        DraygonEnemyState state = DraygonEntity.EncounterOf(body);
        if (body.Health != 0)
        {
            UpdateDraygonHealthBand(state);
            return;
        }

        BeginDraygonDeath(state, samus);
    }

    /// <remarks>
    /// Draygon's death starts again on every touch, shot or power-bomb reaction to a body with no
    /// health left, however long he has been dying: his dying body and eye animations
    /// restart, the drift phase is restarted and Samus is released.
    /// </remarks>
    private void BeginDraygonDeath(DraygonEnemyState state, SamusState samus)
    {
        samus.Grapple.Phase = GrapplePhase.Dropped;
        state.BodyPart.StartAnimation(state.Facing == HorizontalDirection.Right
                ? DraygonSegment.BodyFacingRightDying
                : DraygonSegment.BodyFacingLeftDying);
        state.EyePart.StartAnimation(state.Facing == HorizontalDirection.Right
                ? DraygonSegment.EyeFacingRightDying
                : DraygonSegment.EyeFacingLeftDying);
        state.EyeTracking.Action = DraygonEyeAction.Inert;
        state.Phase = DraygonPhase.Dying;

        // The death behaviour releases Samus unconditionally, even during an
        // unrelated movement owner. In particular, this can interrupt a shinespark.
        // Samus's grapple flags are cleared after this; Draygon sees his own release.
        _grab.ReleaseSamusDuringEnemyUpdate(samus);
        samus.Grapple.Phase = GrapplePhase.Dropped;
    }

    /// <summary>Changes Draygon's health color band only when the health threshold changes.</summary>
    private static void UpdateDraygonHealthBand(DraygonEnemyState state)
    {
        DraygonHealthBand band = DraygonHealthBand.Of(state.Body.Health);

        if (band == state.HealthBand)
            return;
        state.HealthBand = band;
        ShowDraygonHealthBand(state);
    }

    /// <summary>Shows his selected health band's colours.</summary>
    private static void ShowDraygonHealthBand(DraygonEnemyState state) => state.Colours.ShowBand(state.HealthBand);

    /// <summary>A bomb striking one of Draygon's body dud-shot hitboxes: the dud flash where the bomb lies and the dud sound.</summary>
    internal void CreateBombDudShot(EnemyBombContext context) => _projectiles.CreateBombDudShot(context.Bomb);

    /// <summary>Draygon's shot response, including acceleration and special death exemption.</summary>
    internal EnemyShotReaction DraygonShotReaction => field ??= new()
    {
        BeforeSuperMissileQuake = (in EnemyShotContext c) =>
        {
            if (!c.UsesFrameHitboxes)
                throw new InvalidDataException("Draygon requires his authored extended shot hitboxes.");
            if (c.HitboxShot == EnemyReaction.DudShot)
            {
                // Body, claws, and shell use the shared common touch; only the first
                // eye rectangle of the two eye-showing body frames reaches Draygon's damage reaction.
                c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Projectile, c.MarksCollision);
                _projectiles.CreateEnemyProjectileDudShot(c.Projectile);
                return true;
            }
            if (c.HitboxShot != DraygonReactions.Shot)
            {
                throw new InvalidDataException(
                    $"Draygon has no response to shot reaction {c.HitboxShot?.ToString() ?? "(unnamed)"}.");
            }
            return false;
        },
        AfterImpact = c => AdvanceDraygonShotAcceleration(DraygonEntity.EncounterOf(c.Enemy)),
        ExemptFromGenericDeath = true,
        // Draygon's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
        AfterCommon = c => ResolveDraygonReaction(c.Enemy, c.Samus),
    };

    /// <summary>
    /// The reaction of the first hitbox the probe overlaps on <paramref name="enemy"/>'s frame, one of Draygon's parts,
    /// which its own entity asks for (#2301).
    /// </summary>
    internal static bool FindDraygonHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;

        // Both sprite and body-layer art are replaceable presentation. Their
        // component offsets and hitboxes remain fixed gameplay data.
        foreach (EnemyCollisionComponent component in
                 DraygonCollision.ComponentsAt(enemy.Frame))
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
