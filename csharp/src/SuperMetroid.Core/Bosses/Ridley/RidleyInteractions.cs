using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's hitboxes, weapon reactions, intangibility and health colours.</summary>
internal static class RidleyInteractions
{
    /// <summary>Includes both the Ceres and the Lower Norfair Ridley definitions.</summary>
    internal static bool IsRidleyDefinition(EnemyDefinitionId definitionId) =>
        definitionId is EnemyDefinitionId.RidleyCeres or EnemyDefinitionId.Ridley;

    /// <summary>
    /// Puts Norfair Ridley in the health band his health has reached and shows its health shade. Norfair and Ceres show
    /// the same installed health shades, so editing them changes both encounters.
    /// </summary>
    internal static void UpdateNorfairRidleyHealthBand(RoomEnemySlot slot, RidleyEnemyState state)
    {
        state.HealthBand = NorfairRidleyHealthBand.Of(slot.Health);
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
        RoomEnemySlot body,
        RidleyEnemyState state,
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
        if (state.IntangibilityTimer != 0)
        {
            if (--state.IntangibilityTimer == 0)
                body.Contact = body.Contact.Without(EnemyContact.IgnoresSamus);
        }
    }

    /// <summary>Requires the extended Ridley hitbox reaction and suppresses generic death on shots.</summary>
    /// <remarks>Ridley's shot reaction runs the common damage without the hit burst.</remarks>
    internal static EnemyShotReaction RidleyShotReaction { get; } = new()
    {
        BeforeSuperMissileQuake = (in EnemyShotContext c) =>
        {
            if (!c.UsesFrameHitboxes || c.HitboxShot != RidleyReactions.Shot)
            {
                throw new InvalidDataException(
                    $"Lower Norfair Ridley requires extended shot AI " +
                    $"{RidleyReactions.Shot}; extended={c.UsesFrameHitboxes}, " +
                    $"selected={c.HitboxShot?.ToString() ?? "(unnamed)"}, frame={c.Enemy.Frame}.");
            }
            return false;
        },
        ExemptFromGenericDeath = true,
        ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
    };

    /// <summary>Routes bombs through Ridley's shot reaction without generic death.</summary>
    internal static EnemyBombReaction RidleyBombReaction { get; } = new()
    {
        RunsGenericDeath = false,
        ShowsHitBurst = EnemyBombReaction.NeverShowsHitBurst,
    };

    /// <summary>The reaction of the first Ridley hitbox the probe overlaps, or null when the occupant is not Ridley.</summary>
    /// <exception cref="InvalidDataException">Ridley's frame has no compiled collision.</exception>
    internal static bool? FindRidleyHitboxReaction(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        if (!RidleyInteractions.IsRidleyDefinition(enemy.DefinitionId))
            return null;

        // The sprite component offsets in an installed composition are editable.
        // Contact and shot collision instead use the eleven fixed authored
        // body frames and their engine-owned hitbox lists.
        foreach (RidleyCollisionComponent component in
                 RidleyCollision.ComponentsAt(enemy.Frame))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (EnemyHitboxRectangle hitbox in component.Hitboxes)
            {
                if (!probe.Overlaps(
                        CoordinateAxis.Wrap(componentOrigin.X + hitbox.Left),
                        CoordinateAxis.Wrap(componentOrigin.Y + hitbox.Top),
                        CoordinateAxis.Wrap(componentOrigin.X + hitbox.Right),
                        CoordinateAxis.Wrap(componentOrigin.Y + hitbox.Bottom)))
                    continue;
                reaction = probe.SelectsShot
                    ? RidleyCollisionDefinitions.BodyShotReaction
                    : RidleyCollisionDefinitions.BodyTouchReaction;
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
