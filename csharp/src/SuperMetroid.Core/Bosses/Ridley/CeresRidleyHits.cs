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
    /// Checks Samus's shots against Ceres Ridley. A shot that strikes him adds to his hits and takes no health; the shot
    /// explodes as it would on any enemy.
    /// </summary>
    public int ResolveCeresRidleyProjectileHits(
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem samusBombs)
    {
        Ensure.NotNull(projectiles);
        Ensure.NotNull(samusBombs);

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
            if (!projectiles.EnemyImpacts.TryStartEnemyImpact(samusBombs, projectile))
                continue;

            ResolveCeresRidleyShotAfterCollision(place, ceres.Ridley);
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// What a shot or a normal bomb that strikes Ceres Ridley does to him, once it has exploded on him: both react
    /// alike.
    /// </summary>
    /// <param name="place">Ceres Ridley's body.</param>
    /// <param name="ridley">His encounter.</param>
    internal static void ResolveCeresRidleyShotAfterCollision(RoomEnemyPlace place, CeresRidleyState ridley)
    {
        // A hit restarts his hurt flash; a hit while the flash runs on an odd count restarts it one update longer,
        // so repeated hits keep the flash's place in its alternation.
        place.HurtFlash.Restart(place.HurtFlash.IsRunning && RidleyCadenceDefinitions.CeresHurtFlashOddCount.IsSetIn(place.HurtFlash.UpdatesLeft)
            ? CeresRidleyFightDefinitions.HurtFlashKeepingPhase
            : CeresRidleyFightDefinitions.HurtFlash);
        ridley.HitCounter++;

        // His hurt flash takes its phase from the update that has just run, before this hit, and shows at once. Only the
        // count's phase is read, so the count before zero reads as the cycle's last phase.
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
        // His retreat carries him above his chamber, so both rectangles are measured from the room's origin, where a place
        // above the chamber lies at a negative height.
        Offset16<RoomPixels> subjectPlace = RidleyFlight.SignedPlace(subject);
        int subjectLeft = subjectPlace.X - subjectRadius.X;
        int subjectRight = subjectPlace.X + subjectRadius.X;
        int subjectTop = subjectPlace.Y - subjectRadius.Y;
        int subjectBottom = subjectPlace.Y + subjectRadius.Y;
        Offset16<RoomPixels> body = RidleyFlight.SignedPlace(place);
        var components =
            RidleyCollision.ComponentsAt(place.Frame);
        foreach (RidleyCollisionComponent component in components)
        {
            int originX = body.X + component.Offset.X;
            int originY = body.Y + component.Offset.Y;
            foreach (EnemyHitboxRectangle hitbox in component.Hitboxes)
            {
                int left = originX + hitbox.Left;
                int top = originY + hitbox.Top;
                int right = originX + hitbox.Right;
                int bottom = originY + hitbox.Bottom;

                // Touching right or bottom edges do not overlap.
                if (left <= subjectRight &&
                    right > subjectLeft &&
                    top <= subjectBottom &&
                    bottom > subjectTop)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
