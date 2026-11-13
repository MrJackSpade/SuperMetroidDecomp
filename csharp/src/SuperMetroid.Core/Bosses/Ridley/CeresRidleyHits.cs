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
    /// <summary>Provides room randomness and Ceres artwork state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Performs Samus and projectile contact against Ridley's extended body.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Owns the shared Ridley encounter and its battle state.</summary>
    private readonly RidleyEncounter _ridleyEncounter;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CeresRidleyHits(EnemyRoomBinding room, EnemyCollision collision, RidleyEncounter ridleyEncounter)
    {
        _room = room;
        _collision = collision;
        _ridleyEncounter = ridleyEncounter;
    }

    /// <summary>
    /// Runs Ceres Ridley's shot-overlap seam against the five ordinary Samus
    /// projectile slots. The shot handler increments a dedicated counter and never
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

        RoomEnemySlot slot = _ridleyEncounter.Parts.Body;
        if (slot.DefinitionId != EnemyDefinitionId.RidleyCeres ||
            slot.Presence.HasAny(EnemyPresence.Hidden | EnemyPresence.Deleted) || slot.Contact.HasAny(EnemyContact.IgnoresSamus))
        {
            return 0;
        }

        foreach (SamusProjectile projectile in projectiles.Slots)
        {
            if (!projectile.HasEnemyCollisionPayload ||
                !FrameHitboxesOverlapRectangle(
                    slot,
                    projectile.Position.Whole,
                    projectile.Radius))
            {
                continue;
            }
            if (!projectiles.EnemyImpacts.TryStartEnemyImpact(sharedProjectiles, projectile))
                continue;

            ResolveCeresRidleyShotAfterCollision(slot);
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Executes the non-Norfair half of Ridley's shot reaction after the projectile
    /// collision has accepted and marked an overlapping projectile. Both the ordinary-shot and
    /// physical normal-bomb walkers dispatch this exact callback; keeping the reaction here
    /// prevents those two collision owners from acquiring subtly different cinematic rules.
    /// </summary>
    private void ResolveCeresRidleyShotAfterCollision(RoomEnemySlot slot)
    {
        RidleyEnemyState state = _ridleyEncounter.RequireRidley(slot);
        if (slot.DefinitionId != EnemyDefinitionId.RidleyCeres)
        {
            throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} entered Ceres Ridley's shot branch as " +
                $"definition {slot.DefinitionId}.");
        }

        // The hurt flash starts at 13 and selects 14 only when the old nonzero timer is odd.
        // Testing any other property of it (or simply toggling the new value) changes repeated-hit
        // timing and eventually moves the escape threshold relative to the hurt palette.
        slot.FlashTimer = slot.FlashTimer != 0 && RidleyCadenceDefinitions.CeresHurtFlashOddCount.IsSetIn(slot.FlashTimer)
            ? 14
            : 13;
        state.HitCounter++;

        // The game draws the hurt palette from the enemy frame that just completed.
        // Collision runs afterward, so publish that same pre-increment frame immediately
        // for standalone renderers inspecting the colours before the next enemy dispatcher call. Only the count's cadence
        // phase is read, and it wraps, so the count before zero reads as the last phase.
        UpdateRidleyHurtFlashPalettes(
            slot,
            state,
            slot.FrameCounter - 1);
        UpdateCeresRidleyHealthBand(state);
    }

    /// <summary>
    /// Runs Ridley's hurt flash and the common extended-enemy
    /// colour override. Ridley's composite is split between those two writers:
    /// the ordinary body uses the current execution counter, while the manual tail/wings
    /// deliberately test the counter after an increment.
    /// </summary>
    internal static void UpdateRidleyHurtFlashPalettes(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        int enemyMainExecutionCounter)
    {
        state.BodyFlashing = slot.FlashTimer != 0 && EnemyDrawPhaseDefinitions.HurtFlashShown.IsSetIn(enemyMainExecutionCounter);
        // His drawn parts take the phase one pass ahead of his body's colours.
        state.SpritemapFlashing = slot.FlashTimer > 1 && EnemyDrawPhaseDefinitions.HurtFlashShown.IsSetIn(enemyMainExecutionCounter + 1);
    }

    /// <summary>
    /// Updates the health palette. Ceres uses shot count in place of health, including the
    /// game's missing branch after the 90-shot comparison: 50-69 selects health band zero,
    /// and every value from 70 onward selects health band two.
    /// </summary>
    internal static void UpdateCeresRidleyHealthBand(RidleyEnemyState state)
    {
        if (state.FightMode == RidleyFightMode.NotFighting || state.HitCounter < 50)
            return;

        int band = state.HitCounter < 70
            ? CeresRidleyPaletteDefinitions.HealthMidBand
            : CeresRidleyPaletteDefinitions.HealthLateBand;
        state.Colours.ShowHealth(band);
    }

    /// <summary>Routes exploding normal bombs to Ceres Ridley's shot counter after common overlap.</summary>
    internal void AddCeresRidleyBombReactions(EnemySharedReactions table)
    {
        // Ridley's shot reaction branches on area index before it ever reaches common vulnerability damage. Ceres therefore
        // treats an exploding normal bomb exactly like an ordinary shot for its cinematic counter: the 
        // walker has already marked the physical bomb, while Ridley alternates hurt timing and advances the 100-hit
        // escape condition.
        table.AddBombSpecial(c => c.Enemy.DefinitionId == EnemyDefinitionId.RidleyCeres &&
                                  c.SelectedShotAi == EnemyReaction.RidleyShot,
            c => ResolveCeresRidleyShotAfterCollision(c.Enemy));
    }

    /// <summary>Tests Samus against Ridley's extended body and applies contact response after invincibility gating.</summary>
    internal bool ResolveRidleyBodySamusContact(SamusState samus)
    {
        Ensure.NotNull(samus);
        _room.EnsureLoaded();
        // Samus contact gates ordinary contact only on the contact-damage mode and
        // Samus's invincibility timer. It does not inspect the knockback-active
        // state. An extra guard previously made a stale/ongoing aerial knockback
        // suppress every later Ridley overlap even after invincibility had expired.
        RoomEnemySlot slot = _ridleyEncounter.Parts.Body;
        if (_ridleyEncounter.Ridley is null ||
            !RidleyInteractions.IsRidleyDefinition(slot.DefinitionId) ||
            slot.DefinitionId == EnemyDefinitionId.RidleyCeres && _ridleyEncounter.CeresStatus != CeresEscapeStatus.RidleyNotEscaped ||
            !samus.InvincibilityTimer.IsZero)
        {
            return false;
        }

        if (slot.Frame is null ||
            slot.Presence.HasAny(EnemyPresence.Hidden | EnemyPresence.Deleted) || slot.Contact.HasAny(EnemyContact.IgnoresSamus))
        {
            return false;
        }

        if (!EnemyCollision.TryFindFrameHitboxReaction(
                slot,
                samus.Position.Whole,
                samus.Kinematics.Radius,
                contactKind: HitboxContactKind.Touch,
                out EnemyReaction? touchAi))
        {
            return false;
        }

        if (touchAi != EnemyReaction.RidleyFrameHitboxTouch)
        {
            throw new InvalidDataException(
                $"Ridley extended body selected touch AI {touchAi?.ToString() ?? "(unnamed)"}, expected " +
                $"{EnemyReaction.RidleyFrameHitboxTouch} from map {slot.Frame}.");
        }

        // Ridley's touch enters the common no-death-check handler. That distinction matters for
        // Screw Attack: a lethal body touch leaves Lower Norfair Ridley alive long enough
        // to select the forced zero-health lunge and grab Samus for the authored death.
        _collision.ResolveNormalEnemyTouch(
            slot,
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
        RoomEnemySlot slot,
        Position16<RoomPixels> subject,
        Extent16<RoomPixels> subjectRadius)
    {
        int subjectLeft = CoordinateAxis.Wrap(subject.X - subjectRadius.X);
        int subjectRight = CoordinateAxis.Wrap(subject.X + subjectRadius.X);
        int subjectTop = CoordinateAxis.Wrap(subject.Y - subjectRadius.Y);
        int subjectBottom = CoordinateAxis.Wrap(subject.Y + subjectRadius.Y);
        var components =
            RidleyCollision.ComponentsAt(slot.Frame);
        foreach (RidleyCollisionComponent component in components)
        {
            Position16<RoomPixels> componentOrigin = slot.Position.Whole + component.Offset;
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
