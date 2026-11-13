using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's collision with solid enemies: which solid or frozen enemy stops a move, and how far she may still travel.
/// </summary>
/// <remarks>
/// This is a probe, not an enemy simulation. The caller supplies the current interactive-enemy list in slot order, and
/// that order matters: the first enemy that stops the move is returned, not the geometrically nearest one. Room
/// coordinates wrap past either end of the room's space (<see cref="CoordinateAxis"/>), so an enemy that lies just past
/// the room's edge from Samus is still close to her, and the gaps are read as signed distances across that space.
/// </remarks>
public static class SamusSolidEnemyCollision
{
    /// <summary>
    /// Runs one directional probe without moving Samus.
    /// </summary>
    /// <param name="state">Samus's current whole/subpixel position and collision radii.</param>
    /// <param name="interactiveEnemies">
    /// Active interactive enemies in slot order. Non-solid, unfrozen entries remain in
    /// the list because the loop encounters and rejects them individually.
    /// </param>
    /// <param name="direction">The probe's direction: left, right, up or down.</param>
    /// <param name="distance">Whole pixels of the requested collision distance.</param>
    /// <param name="distanceSubposition">Fraction of the requested collision distance.</param>
    /// <returns>The first qualifying body's current leading-edge gap, or the requested whole distance with no enemy when no collision is found.</returns>
    /// <remarks>No whole position or enemy body is changed. Exact current-edge contact clears <paramref name="state"/>'s Y subposition even for horizontal probes. The distance is a magnitude; direction supplies its sign.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="state"/> or <paramref name="interactiveEnemies"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is not one of the four directions; NonDirectionalProbe is not accepted here.</exception>
    public static SolidEnemyCollisionResult Probe(
        SamusKinematicsState state,
        IReadOnlyList<SolidEnemyCollisionBody> interactiveEnemies,
        SamusCollisionDirection direction,
        int distance,
        int distanceSubposition)
    {
        Ensure.NotNull(state);
        Ensure.NotNull(interactiveEnemies);

        if (direction is not (SamusCollisionDirection.Left or SamusCollisionDirection.Right or
            SamusCollisionDirection.Up or SamusCollisionDirection.Down))
            throw new ArgumentOutOfRangeException(nameof(direction));

        // The broad overlap test uses only a whole-pixel target, which the probe's reach rule places past Samus's
        // reached centre (SolidEnemyProbeReach).
        Position16<RoomPixels> target = BuildTarget(
            state, direction, new UnsignedSixteenSixteen(distance, distanceSubposition));

        foreach (SolidEnemyCollisionBody enemy in interactiveEnemies)
        {
            if (!enemy.Solid)
                continue;

            // The target centre must overlap the enemy on both axes. Tangency is not overlap: the centres must lie
            // strictly closer than the sum of both radii.
            if (!StrictlyOverlaps(target.X, state.Radius.X, enemy.Position.X, enemy.Radius.X) ||
                !StrictlyOverlaps(target.Y, state.Radius.Y, enemy.Position.Y, enemy.Radius.Y))
            {
                continue;
            }

            // The broad test says the future boxes overlap. The directional test now uses the *current* leading edges to
            // tell an approaching gap from an enemy Samus is already embedded in: a negative gap skips that enemy.
            int gap = direction switch
            {
                SamusCollisionDirection.Left => CoordinateAxis.AsSigned(
                    state.Position.X.Whole - state.Radius.X - enemy.Position.X - enemy.Radius.X),
                SamusCollisionDirection.Right => CoordinateAxis.AsSigned(
                    enemy.Position.X - enemy.Radius.X - state.Position.X.Whole - state.Radius.X),
                SamusCollisionDirection.Up => CoordinateAxis.AsSigned(
                    state.Position.Y.Whole - state.Radius.Y - enemy.Position.Y - enemy.Radius.Y),
                SamusCollisionDirection.Down => CoordinateAxis.AsSigned(
                    enemy.Position.Y - enemy.Radius.Y - state.Position.Y.Whole - state.Radius.Y),
                // Probe rejected the non-directional probe above. Keep a throwing arm anyway so this switch stays honest
                // if the public enum later grows.
                SamusCollisionDirection.NonDirectionalProbe => throw new InvalidOperationException("Unreachable collision direction."),
                _ => throw new InvalidOperationException("Unreachable collision direction."),
            };

            if (gap < 0)
                continue;

            bool wasTouching = gap == 0;
            if (wasTouching)
            {
                // Clears Y subposition for every direction. Yes, that clears
                // Samus's *Y* subposition even for a left/right touch. This is an
                // game bug; preserving it prevents one-pixel drift
                // differences when repeatedly pressing into a solid or frozen enemy.
                state.Position = state.Position.WithFractionY(0);
            }

            // Success publishes the whole gap and the enemy met; the caller records it for its direction.
            return new SolidEnemyCollisionResult(
                Collided: true,
                Distance: gap,
                Enemy: enemy.Enemy,
                WasTouching: wasTouching);
        }

        // No collision names no enemy and returns the requested whole distance.
        return new SolidEnemyCollisionResult(
            Collided: false,
            Distance: distance,
            Enemy: null,
            WasTouching: false);
    }

    /// <summary>The whole-pixel centre the broad test checks: Samus's centre with the probe's axis moved to the pixel the reach tests.</summary>
    private static Position16<RoomPixels> BuildTarget(
        SamusKinematicsState state,
        SamusCollisionDirection direction,
        UnsignedSixteenSixteen reach)
    {
        Position16<RoomPixels> target = state.Position.Whole;
        return direction switch
        {
            SamusCollisionDirection.Left => target.WithX(SolidEnemyProbeReach.TestedPixelBack(state.Position.X, reach)),
            SamusCollisionDirection.Right => target.WithX(SolidEnemyProbeReach.TestedPixelForward(state.Position.X, reach)),
            SamusCollisionDirection.Up => target.WithY(SolidEnemyProbeReach.TestedPixelBack(state.Position.Y, reach)),
            SamusCollisionDirection.Down => target.WithY(SolidEnemyProbeReach.TestedPixelForward(state.Position.Y, reach)),
            // The pose-observation probe targets Samus's current position.
            SamusCollisionDirection.NonDirectionalProbe => target,
            _ => throw new InvalidOperationException($"Undefined SamusCollisionDirection {direction}."),
        };
    }

    /// <summary>
    /// Whether two boxes overlap on one axis: their centres' distance across the wrapping room space is strictly less
    /// than the sum of their radii.
    /// </summary>
    private static bool StrictlyOverlaps(
        int firstPosition,
        int firstRadius,
        int secondPosition,
        int secondRadius) =>
        CoordinateAxis.Distance(secondPosition, firstPosition) < firstRadius + secondRadius;
}
