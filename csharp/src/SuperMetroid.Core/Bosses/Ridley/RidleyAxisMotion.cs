namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's position and velocity along one axis.</summary>
/// <param name="Position">The position.</param>
/// <param name="Velocity">The velocity.</param>
internal readonly partial record struct RidleyAxisMotion(FixedCoordinate Position, SignedEightEight Velocity);
