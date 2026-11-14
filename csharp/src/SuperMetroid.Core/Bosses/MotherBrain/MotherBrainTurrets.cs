using static SuperMetroid.Core.Bosses.MotherBrain.MotherBrainTurretDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's room turrets: the directions each turret may turn to, and the bullet each direction fires.</summary>
internal static class MotherBrainTurrets
{
    /// <summary>Returns the bullet launch for one of the eight directions.</summary>
    internal static MotherBrainTurretDirectionDefinition ForDirection(
        MotherBrainTurretDirection direction)
    {
        int index = Ring.PositionOf(direction);
        double angle = index * Math.PI / 4;
        int muzzleY = MuzzleYs.TryGetValue(direction, out int y) ? y : throw new ArgumentOutOfRangeException(nameof(direction));
        return new(new((int)Math.Round(-MuzzleRadius * Math.Cos(angle)), muzzleY),
            EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel((int)Math.Round(-BulletSpeed * Math.Cos(angle)), (int)Math.Round(BulletSpeed * Math.Sin(angle))));
    }

    /// <summary>Whether <paramref name="turret"/> may rotate to <paramref name="direction"/>: whether it lies in the turret's arc.</summary>
    internal static bool IsRotationAllowed(MotherBrainTurretDefinition turret, MotherBrainTurretDirection direction)
    {
        int position = Ring.PositionOf(direction);
        return position >= Ring.PositionOf(turret.FirstAllowedDirection) && position <= Ring.PositionOf(turret.LastAllowedDirection);
    }
}
