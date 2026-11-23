namespace Engine.Shared;

/// <summary>An exact, signed speed in two dimensions: how far something moves in one update.</summary>
/// <param name="X">The distance moved rightward each update; negative is leftward.</param>
/// <param name="Y">The distance moved downward each update; negative is upward.</param>
public readonly record struct PixelVelocity(Pixels X, Pixels Y)
{
    /// <summary>At rest.</summary>
    public static PixelVelocity Zero => default;

    /// <summary>How far this speed carries something in one update.</summary>
    public PixelOffset OverOneUpdate => new(X, Y);

    /// <summary>This speed with its horizontal part replaced.</summary>
    public PixelVelocity WithX(Pixels x) => this with { X = x };

    /// <summary>This speed with its vertical part replaced.</summary>
    public PixelVelocity WithY(Pixels y) => this with { Y = y };

    /// <summary>This speed after an acceleration has acted on it for one update.</summary>
    public static PixelVelocity operator +(PixelVelocity velocity, PixelVelocity acceleration) =>
        new(velocity.X + acceleration.X, velocity.Y + acceleration.Y);

    public static PixelVelocity operator -(PixelVelocity velocity) => new(-velocity.X, -velocity.Y);
}
