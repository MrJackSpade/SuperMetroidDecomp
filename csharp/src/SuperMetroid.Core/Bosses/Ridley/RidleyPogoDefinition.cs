namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One of Ridley's pogo bounces: the bounce's launch velocity and how fast it rises and falls.</summary>
/// <param name="Velocity">The launch velocity: the horizontal magnitude and the upward vertical speed.</param>
/// <param name="UpwardAcceleration">The acceleration while rising.</param>
/// <param name="DownwardAcceleration">The acceleration while falling.</param>
public readonly record struct RidleyPogoDefinition(EightEightVector<RoomPixels> Velocity, SignedEightEight UpwardAcceleration, SignedEightEight DownwardAcceleration);
