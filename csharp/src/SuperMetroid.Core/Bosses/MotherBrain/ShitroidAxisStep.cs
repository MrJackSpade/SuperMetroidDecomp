namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The result of accelerating the Shitroid along one axis.</summary>
/// <param name="Arrived">Whether the target was reached.</param>
/// <param name="Velocity">The new velocity.</param>
internal readonly record struct ShitroidAxisStep(bool Arrived, int Velocity);
