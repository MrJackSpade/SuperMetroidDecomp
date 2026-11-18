using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's hitboxes, weapon reactions, intangibility and health colours.</summary>
internal static class RidleyInteractions
{

    /// <summary>
    /// Puts Norfair Ridley in the health band his health has reached and shows its health shade. Norfair and Ceres show
    /// the same installed health shades, so editing them changes both encounters.
    /// </summary>
    internal static void UpdateNorfairRidleyHealthBand(RoomEnemyPlace place, NorfairRidleyState state)
    {
        state.HealthBand = NorfairRidleyHealthBand.Of(place.Health);
        if (state.HealthBand.HealthShade is { } shade)
            state.Colours.ShowHealth(shade);
    }

    /// <summary>
    /// Runs the collision-relevant portion of Ridley's Samus-interaction gate.
    /// Off-screen bodies disable collision and suspend the release timer.
    /// On-screen fight AI clears the gate before ticking that timer. The enemy update has
    /// already consumed the previous gate value for this update's body collision.
    /// </summary>
    internal static void UpdateNorfairRidleyIntangibility(
        RoomEnemyPlace body,
        NorfairRidleyState state,
        Position16<RoomPixels> camera)
    {
        if (state.FightMode == RidleyFightMode.Dying)
            return;
        if (state.FightMode != RidleyFightMode.NotFighting)
        {
            if (RidleyCollision.IsOutsideInteractionWindow(body.Position.Whole, camera))
            {
                body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
                return;
            }
            body.Contact = body.Contact.Without(EnemyContact.IgnoresSamus);
        }
        if (state.ReleaseIntangibility is { } intangibility && intangibility.StepRunsOut())
        {
            state.ReleaseIntangibility = null;
            body.Contact = body.Contact.Without(EnemyContact.IgnoresSamus);
        }
    }

    /// <summary>Suppresses generic death on shots.</summary>
    /// <remarks>Ridley's shot reaction runs the common damage without the hit burst.</remarks>
    internal static EnemyShotReaction RidleyShotReaction { get; } = new()
    {
        ExemptFromGenericDeath = true,
        ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
    };

    /// <summary>Routes bombs through Ridley's shot reaction without generic death.</summary>
    internal static EnemyBombReaction RidleyBombReaction { get; } = new()
    {
        RunsGenericDeath = false,
        ShowsHitBurst = EnemyBombReaction.NeverShowsHitBurst,
    };

    /// <summary>
    /// The reaction of Ridley's body where the probe overlaps it, which both Ridley bodies' own entities give the shared
    /// collision (#2335): true with his touch or shot reaction, as the probe selects, or false when none of his body's
    /// hitboxes overlaps it.
    /// </summary>
    /// <exception cref="InvalidDataException">Ridley's frame has no collision.</exception>
    internal static bool FindRidleyHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = !BodyOverlaps(enemy, probe) ? null
            : probe.SelectsShot ? RidleyCollisionDefinitions.BodyShotReaction : RidleyCollisionDefinitions.BodyTouchReaction;
        return reaction is not null;
    }

    /// <summary>Whether <paramref name="probe"/> overlaps a hitbox of the body frame Ridley shows (#2410).</summary>
    /// <exception cref="InvalidDataException">Ridley's frame has no collision.</exception>
    internal static bool BodyOverlaps(RoomEnemyPlace enemy, EnemyHitboxProbe probe)
    {
        // An installed picture's composition may be edited; touch and shots still meet the hitboxes of the eleven
        // body frames the game defines.
        foreach (RidleyCollisionComponent component in
                 RidleyCollision.ComponentsAt(enemy.Frame))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (EnemyHitboxRectangle hitbox in component.Hitboxes)
            {
                if (probe.Overlaps(
                        componentOrigin.X + hitbox.Left,
                        componentOrigin.Y + hitbox.Top,
                        componentOrigin.X + hitbox.Right,
                        componentOrigin.Y + hitbox.Bottom))
                    return true;
            }
        }
        return false;
    }

    /// <summary>Ridley's Power Bomb reaction, exempt from the generic death.</summary>
    internal static EnemyPowerBombReaction RidleyPowerBombReaction { get; } = new()
    {
        ExemptFromGenericDeath = true,
    };
}
