using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>What Ceres Ridley's body does to Samus and what Samus's weapons do to Ceres Ridley.</summary>
[RoomLifetime]
internal sealed class CeresRidleyHits
{
    /// <summary>Performs Samus and projectile contact against Ridley's extended body.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Owns the shared Ridley encounter and its battle state.</summary>
    private readonly RidleyEncounter _ridleyEncounter;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CeresRidleyHits(EnemyCollision collision, RidleyEncounter ridleyEncounter)
    {
        _collision = collision;
        _ridleyEncounter = ridleyEncounter;
    }

    /// <summary>
    /// Runs Ceres Ridley's shot-overlap seam against Samus's five ordinary
    /// projectile places. The shot response increments a dedicated counter and never
    /// subtracts enemy health; the projectile owner performs its normal explosion change.
    /// </summary>
    public int ResolveCeresRidleyProjectileHits(
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles)
    {
        Ensure.NotNull(projectiles);
        Ensure.NotNull(sharedProjectiles);

        if (_ridleyEncounter.Ridley is null || _ridleyEncounter.CeresStatus != CeresEscapeStatus.RidleyNotEscaped)
            return 0;

        RoomEnemyPlace place = _ridleyEncounter.Parts.Body;
        if (place.DefinitionId != EnemyDefinitionId.RidleyCeres ||
            place.Presence.HasAny(EnemyPresence.Hidden | EnemyPresence.Deleted) || place.Contact.HasAny(EnemyContact.IgnoresSamus))
        {
            return 0;
        }

        foreach (SamusProjectile projectile in projectiles.Shots)
        {
            if (!projectile.HasEnemyCollisionPayload ||
                !FrameHitboxesOverlapRectangle(
                    place,
                    projectile.Position.Whole,
                    projectile.Radius))
            {
                continue;
            }
            if (!projectiles.EnemyImpacts.TryStartEnemyImpact(sharedProjectiles, projectile))
                continue;

            ResolveCeresRidleyShotAfterCollision(place);
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Executes the non-Norfair half of Ridley's shot reaction after the projectile
    /// collision has accepted and marked an overlapping projectile. A shot and a normal bomb both run this exact
    /// reaction; keeping the reaction here prevents those two collisions from acquiring subtly different cinematic
    /// rules.
    /// </summary>
    private void ResolveCeresRidleyShotAfterCollision(RoomEnemyPlace place)
    {
        RidleyEnemyState state = _ridleyEncounter.RequireRidley(place);
        if (place.DefinitionId != EnemyDefinitionId.RidleyCeres)
        {
            throw new InvalidOperationException(
                $"Enemy place {place.Ordinal} entered Ceres Ridley's shot branch as " +
                $"definition {place.DefinitionId}.");
        }

        // The hurt flash starts at 13 and selects 14 only when the old nonzero timer is odd.
        // Testing any other property of it (or simply toggling the new value) changes repeated-hit
        // timing and eventually moves the escape threshold relative to the hurt flash.
        place.FlashTimer = place.FlashTimer != 0 && RidleyCadenceDefinitions.CeresHurtFlashOddCount.IsSetIn(place.FlashTimer)
            ? 14
            : 13;
        state.HitCounter++;

        // The game draws the hurt flash from the enemy frame that just completed.
        // Collision runs afterward, so publish that same pre-increment frame immediately
        // for standalone renderers inspecting the colours before the next enemy update. Only the count's cadence
        // phase is read, and it wraps, so the count before zero reads as the last phase.
        UpdateRidleyHurtFlash(
            place,
            state,
            place.FrameCounter - 1);
        UpdateCeresRidleyHealthBand(state);
    }

    /// <summary>
    /// Runs Ridley's hurt flash and the common extended-enemy
    /// colour override. Ridley's composite is split between those two writers:
    /// the ordinary body uses the current execution counter, while the manual tail/wings
    /// deliberately test the counter after an increment.
    /// </summary>
    internal static void UpdateRidleyHurtFlash(
        RoomEnemyPlace place,
        RidleyEnemyState state,
        int enemyMainExecutionCounter)
    {
        state.BodyFlashing = place.FlashTimer != 0 && EnemyDrawPhaseDefinitions.HurtFlashShown.IsSetIn(enemyMainExecutionCounter);
        // His drawn parts take the phase one pass ahead of his body's colours.
        state.TailAndWingsFlashing = place.FlashTimer > 1 && EnemyDrawPhaseDefinitions.HurtFlashShown.IsSetIn(enemyMainExecutionCounter + 1);
    }

    /// <summary>
    /// Updates the health shade. Ceres uses shot count in place of health, including the
    /// game's missing branch after the 90-shot comparison: 50-69 selects health band zero,
    /// and every value from 70 onward selects health band two.
    /// </summary>
    internal static void UpdateCeresRidleyHealthBand(RidleyEnemyState state)
    {
        if (state.FightMode == RidleyFightMode.NotFighting || state.HitCounter < 50)
            return;

        int band = state.HitCounter < 70
            ? CeresRidleyShadeDefinitions.HealthMidBand
            : CeresRidleyShadeDefinitions.HealthLateBand;
        state.Colours.ShowHealth(band);
    }

    /// <summary>Routes exploding normal bombs to Ceres Ridley's shot counter after common overlap.</summary>
    internal void ReactToCeresRidleyBomb(EnemyBombContext c) =>
        // Ridley's shot reaction branches on area index before it ever reaches common vulnerability damage. Ceres therefore
        // treats an exploding normal bomb exactly like an ordinary shot for its cinematic counter: the
        // walker has already marked the physical bomb, while Ridley alternates hurt timing and advances the 100-hit
        // escape condition.
        ResolveCeresRidleyShotAfterCollision(c.Enemy);

    /// <summary>Tests Samus against Ridley's extended body and applies contact response after invincibility gating.</summary>
    internal bool ResolveRidleyBodySamusContact(SamusState samus)
    {
        Ensure.NotNull(samus);
        // Samus contact gates ordinary contact only on the contact-damage mode and Samus's invincibility timer, never on
        // whether her knockback is still running.
        if (_ridleyEncounter.Ridley is null)
            return false;

        RoomEnemyPlace place = _ridleyEncounter.Parts.Body;
        if (!RidleyInteractions.IsRidleyDefinition(place.DefinitionId) ||
            place.DefinitionId == EnemyDefinitionId.RidleyCeres && _ridleyEncounter.CeresStatus != CeresEscapeStatus.RidleyNotEscaped ||
            !samus.InvincibilityTimer.IsZero)
        {
            return false;
        }

        if (place.Frame is null ||
            place.Presence.HasAny(EnemyPresence.Hidden | EnemyPresence.Deleted) || place.Contact.HasAny(EnemyContact.IgnoresSamus))
        {
            return false;
        }

        if (!EnemyCollision.TryFindFrameHitboxReaction(
                place,
                samus.Position.Whole,
                samus.Kinematics.Radius,
                contactKind: HitboxContactKind.Touch,
                out EnemyReaction? bodyTouch))
        {
            return false;
        }

        if (bodyTouch != RidleyReactions.FrameHitboxTouch)
        {
            throw new InvalidDataException(
                $"Ridley extended body selected touch reaction {bodyTouch?.ToString() ?? "(unnamed)"}, expected " +
                $"{RidleyReactions.FrameHitboxTouch} from map {place.Frame}.");
        }

        // Ridley's touch enters the common touch response that does not check for death. That distinction matters for
        // Screw Attack: a lethal body touch leaves Lower Norfair Ridley alive long enough
        // to select the forced zero-health lunge and grab Samus for the authored death.
        _collision.ResolveNormalEnemyTouch(
            place,
            samus,
            deathAnimation: TouchDeathAnimation.Skip);
        return true;
    }

    /// <summary>
    /// The shared rectangle walk used by Samus contact and
    /// projectile contact. Every component offset and every rectangle comes from the
    /// frame Ridley shows; the tiny 8x8 radius in Ridley's species definition is not his
    /// body and must never be substituted for this data.
    /// </summary>
    internal static bool FrameHitboxesOverlapRectangle(
        RoomEnemyPlace place,
        Position16<RoomPixels> subject,
        Extent16<RoomPixels> subjectRadius)
    {
        int subjectLeft = CoordinateAxis.Wrap(subject.X - subjectRadius.X);
        int subjectRight = CoordinateAxis.Wrap(subject.X + subjectRadius.X);
        int subjectTop = CoordinateAxis.Wrap(subject.Y - subjectRadius.Y);
        int subjectBottom = CoordinateAxis.Wrap(subject.Y + subjectRadius.Y);
        var components =
            RidleyCollision.ComponentsAt(place.Frame);
        foreach (RidleyCollisionComponent component in components)
        {
            Position16<RoomPixels> componentOrigin = place.Position.Whole + component.Offset;
            foreach (EnemyHitboxRectangle hitbox in component.Hitboxes)
            {
                int left = CoordinateAxis.Wrap(componentOrigin.X + hitbox.Left);
                int top = CoordinateAxis.Wrap(componentOrigin.Y + hitbox.Top);
                int right = CoordinateAxis.Wrap(componentOrigin.X + hitbox.Right);
                int bottom = CoordinateAxis.Wrap(componentOrigin.Y + hitbox.Bottom);

                // The rectangle checks use signed comparisons and treat touching
                // right/bottom edges as non-overlap. Ceres coordinates remain in the low
                // positive room range, so these integer comparisons are the same result.
                if (CoordinateAxis.Difference(left, subjectRight) <= 0 &&
                    CoordinateAxis.Difference(right, subjectLeft) > 0 &&
                    CoordinateAxis.Difference(top, subjectBottom) <= 0 &&
                    CoordinateAxis.Difference(bottom, subjectTop) > 0)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
