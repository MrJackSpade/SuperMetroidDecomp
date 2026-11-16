using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>What Samus's weapons do to Ceres Ridley: each accepted shot or bomb counts toward the hundred that end his fight.</summary>
[RoomLifetime]
internal sealed class CeresRidleyHits
{
    /// <summary>The room's view of Ridley: Ceres Ridley as he attached himself, and the station's escape progress.</summary>
    private readonly RidleyEncounter _ridleyEncounter;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CeresRidleyHits(RidleyEncounter ridleyEncounter)
    {
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

        // Only a room Ceres Ridley has initialized in, before he gets away, meets these shots.
        if (_ridleyEncounter.CeresRidley is not { } ceres || _ridleyEncounter.CeresStatus != CeresEscapeStatus.RidleyNotEscaped)
            return 0;

        RoomEnemyPlace place = ceres.Body;
        if (place.Presence.HasAny(EnemyPresence.Hidden | EnemyPresence.Deleted) || place.Contact.HasAny(EnemyContact.IgnoresSamus))
            return 0;

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

            ResolveCeresRidleyShotAfterCollision(place, ceres.Ridley);
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Executes the Ceres half of Ridley's shot reaction after the projectile collision has accepted and marked an
    /// overlapping projectile. A shot and a normal bomb both run this exact reaction; keeping the reaction here prevents
    /// those two collisions from acquiring subtly different cinematic rules.
    /// </summary>
    /// <param name="place">Ceres Ridley's body.</param>
    /// <param name="ridley">His encounter.</param>
    internal static void ResolveCeresRidleyShotAfterCollision(RoomEnemyPlace place, CeresRidleyState ridley)
    {
        // The hurt flash starts at 13 and selects 14 only when the old nonzero timer is odd.
        // Testing any other property of it (or simply toggling the new value) changes repeated-hit
        // timing and eventually moves the escape threshold relative to the hurt flash.
        place.FlashTimer = place.FlashTimer != 0 && RidleyCadenceDefinitions.CeresHurtFlashOddCount.IsSetIn(place.FlashTimer)
            ? CeresRidleyFightDefinitions.HurtFlashKeepingPhase
            : CeresRidleyFightDefinitions.HurtFlash;
        ridley.HitCounter++;

        // The game draws the hurt flash from the enemy frame that just completed.
        // Collision runs afterward, so publish that same pre-increment frame immediately
        // for standalone renderers inspecting the colours before the next enemy update. Only the count's cadence
        // phase is read, and it wraps, so the count before zero reads as the last phase.
        RidleyBodyContact.UpdateHurtFlash(
            place,
            ridley,
            place.FrameCounter - 1);
        UpdateCeresRidleyHealthBand(ridley);
    }

    /// <summary>
    /// Shows the health colouring of the band his accepted hits have reached while he fights. Ceres counts hits in place
    /// of health (<see cref="CeresRidleyHealthBandDefinitions"/>); before his first hurt band his colours stay as they are.
    /// </summary>
    internal static void UpdateCeresRidleyHealthBand(CeresRidleyState ridley)
    {
        if (ridley.FightMode == RidleyFightMode.NotFighting)
            return;

        if (CeresRidleyHealthBand.Of(ridley.HitCounter).HealthShade is { } shade)
            ridley.Colours.ShowHealth(shade);
    }

    /// <summary>
    /// The shared rectangle walk used by projectile contact. Every component offset and every rectangle comes from the
    /// frame Ridley shows; the tiny 8x8 radius in Ridley's species definition is not his body and must never be
    /// substituted for this data.
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
