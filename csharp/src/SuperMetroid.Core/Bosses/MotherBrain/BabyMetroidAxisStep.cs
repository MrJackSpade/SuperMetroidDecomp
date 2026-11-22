namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The result of accelerating the Baby Metroid along one axis.</summary>
/// <param name="Arrived">Whether the target was reached.</param>
/// <param name="Velocity">The new velocity.</param>
internal readonly partial record struct BabyMetroidAxisStep(bool Arrived, SignedEightEight Velocity);
