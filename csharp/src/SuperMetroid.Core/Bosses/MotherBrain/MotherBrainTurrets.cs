using static SuperMetroid.Core.Bosses.MotherBrain.MotherBrainTurretDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's room turrets: the directions each turret may turn to, its turns, and the bullet each direction fires.</summary>
internal static class MotherBrainTurrets
{
    /// <summary>The bullet launch for one of the eight directions.</summary>
    internal static MotherBrainTurretDirectionDefinition ForDirection(MotherBrainTurretDirection direction) =>
        Launches.TryGetValue(direction, out MotherBrainTurretDirectionDefinition launch)
            ? launch
            : throw new ArgumentOutOfRangeException(nameof(direction), direction, "A turret has no such direction.");

    /// <summary>
    /// The direction one turn from <paramref name="direction"/> the way <paramref name="turning"/> says, round the compass
    /// (<see cref="Ring"/>): counterclockwise from left toward down, clockwise from left toward up.
    /// </summary>
    internal static MotherBrainTurretDirection Turned(MotherBrainTurretDirection direction, RotationDirection turning)
    {
        int step = turning == RotationDirection.Counterclockwise ? 1 : Ring.Members.Count - 1;
        return Ring.At((Ring.PositionOf(direction) + step) % Ring.Members.Count);
    }

    /// <summary>Whether <paramref name="turret"/> may rotate to <paramref name="direction"/>: whether it lies in the turret's arc.</summary>
    internal static bool IsRotationAllowed(MotherBrainTurretDefinition turret, MotherBrainTurretDirection direction)
    {
        int position = Ring.PositionOf(direction);
        return position >= Ring.PositionOf(turret.FirstAllowedDirection) && position <= Ring.PositionOf(turret.LastAllowedDirection);
    }
}
