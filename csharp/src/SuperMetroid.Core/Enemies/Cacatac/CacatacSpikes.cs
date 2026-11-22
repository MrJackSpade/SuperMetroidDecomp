namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>Chooses a Cacatac spike's launch velocity by its direction.</summary>
internal static partial class CacatacSpikes
{
    /// <summary>
    /// The velocity a spike launched in <paramref name="direction"/> flies at: its kind's speed toward the side or sides
    /// its direction names, and none on an axis it does not move along.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="direction"/> is not one of the ten spike directions.</exception>
    internal static PixelVelocity<RoomPixels> Velocity(CacatacSpikeDirection direction)
    {
        SignedEightEight cardinal = CacatacProjectileDefinitions.CardinalSpeed;
        SignedEightEight diagonal = CacatacProjectileDefinitions.DiagonalSpeed;
        SignedEightEight still = SignedEightEight.Zero;
        return direction switch
        {
            CacatacSpikeDirection.LeftFacingUp or CacatacSpikeDirection.LeftFacingDown => new(-cardinal, still),
            CacatacSpikeDirection.RightFacingUp or CacatacSpikeDirection.RightFacingDown => new(cardinal, still),
            CacatacSpikeDirection.Up => new(still, -cardinal),
            CacatacSpikeDirection.Down => new(still, cardinal),
            CacatacSpikeDirection.UpLeft => new(-diagonal, -diagonal),
            CacatacSpikeDirection.UpRight => new(diagonal, -diagonal),
            CacatacSpikeDirection.DownLeft => new(-diagonal, diagonal),
            CacatacSpikeDirection.DownRight => new(diagonal, diagonal),
            _ => throw new InvalidDataException($"Cacatac spike direction {direction} is outside the ten spike directions."),
        };
    }
}
