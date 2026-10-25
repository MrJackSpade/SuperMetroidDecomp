namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One record of Ridley's pogo table: the bounce's launch velocity and how fast it rises and falls.</summary>
/// <param name="Velocity">The launch velocity: the horizontal magnitude and the upward vertical speed.</param>
/// <param name="UpwardAcceleration">The acceleration while rising.</param>
/// <param name="DownwardAcceleration">The acceleration while falling.</param>
public readonly record struct RidleyPogoDefinition(EightEightVector<RoomPixels> Velocity, int UpwardAcceleration, int DownwardAcceleration);
