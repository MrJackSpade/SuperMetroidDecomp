namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ceres Ridley's position and velocity along one axis.</summary>
/// <param name="Position">The position.</param>
/// <param name="Velocity">The velocity.</param>
internal readonly record struct CeresRidleyAxisMotion(FixedCoordinate Position, int Velocity);
