namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>Negative and positive signed 8.8 launch velocities for a Cacatac spike axis.</summary>
/// <param name="Negative">Velocity toward left or upward movement for the selected cardinal or diagonal magnitude.</param>
/// <param name="Positive">Velocity toward right or downward movement for the same magnitude.</param>
internal readonly record struct CacatacSpikeSpeedPair(
    SignedEightEight Negative,
    SignedEightEight Positive);
