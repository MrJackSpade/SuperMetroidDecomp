using static SuperMetroid.Core.Bosses.MotherBrain.MotherBrainTurretDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's room turrets: each turret's placement and rotation range, and the bullet each direction fires.</summary>
internal static class MotherBrainTurrets
{
    /// <summary>Returns one of the twelve physical turret definitions.</summary>
    internal static MotherBrainTurretDefinition ForTurret(int parameter)
    {
        if (parameter is < 0 or >= TurretCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parameter), parameter, "Mother Brain turret parameter must be zero through eleven.");
        }

        int bay = parameter / 3;
        int column = parameter % 3;
        bool wideSector = (bay & 1) != 0;
        int xInset = column == 0 ? 0 : 48 + column * 32;
        MotherBrainTurretDirection initial = column switch
        {
            0 => MotherBrainTurretDirection.DownRight,
            1 => MotherBrainTurretDirection.Right,
            _ when bay == 3 => MotherBrainTurretDirection.DownLeft,
            _ => MotherBrainTurretDirection.Down,
        };
        MotherBrainTurretDirection first = column switch
        {
            0 => MotherBrainTurretDirection.DownLeft,
            1 => wideSector ? MotherBrainTurretDirection.DownLeft : MotherBrainTurretDirection.Down,
            _ => MotherBrainTurretDirection.Left,
        };
        MotherBrainTurretDirection last = column switch
        {
            0 => MotherBrainTurretDirection.DownRight,
            1 => MotherBrainTurretDirection.Right,
            _ => wideSector ? MotherBrainTurretDirection.DownRight : MotherBrainTurretDirection.Down,
        };
        return new(new((ushort)(FirstBayX - bay * BaySpacing - xInset), (ushort)(column == 0 ? 48 : 64)), initial, first, last);
    }

    /// <summary>Returns the bullet launch for one of the eight directions.</summary>
    internal static MotherBrainTurretDirectionDefinition ForDirection(
        MotherBrainTurretDirection direction)
    {
        int index = Ring.PositionOf(direction);
        double angle = index * Math.PI / 4;
        int muzzleY = MuzzleYs.TryGetValue(direction, out int y) ? y : throw new ArgumentOutOfRangeException(nameof(direction));
        return new(new((short)Math.Round(-MuzzleRadius * Math.Cos(angle)), (short)muzzleY),
            new((short)Math.Round(-BulletSpeed * Math.Cos(angle)), (short)Math.Round(BulletSpeed * Math.Sin(angle))));
    }

    /// <summary>Whether turret <paramref name="turret"/> may rotate to <paramref name="direction"/>.</summary>
    internal static bool IsRotationAllowed(int turret, MotherBrainTurretDirection direction)
    {
        MotherBrainTurretDefinition definition = ForTurret(turret);
        int position = Ring.PositionOf(direction);
        return position >= Ring.PositionOf(definition.FirstAllowedDirection) && position <= Ring.PositionOf(definition.LastAllowedDirection);
    }
}
