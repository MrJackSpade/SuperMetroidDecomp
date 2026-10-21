namespace SuperMetroid.Core.Enemies;

/// <summary>The fireball launch velocities of a Nami or Fune facing left and right.</summary>
/// <param name="Left">The left.</param>
/// <param name="Right">The right.</param>
public readonly record struct NamiFuneLaunchVelocities(int Left, int Right);
