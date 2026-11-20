namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>The yspeed, expected x and expected speed of one verify Baby Metroid drain carry case.</summary>
/// <param name="YSpeed">The yspeed.</param>
/// <param name="ExpectedX">The expected x.</param>
/// <param name="ExpectedSpeed">The expected speed.</param>
internal readonly record struct VerifyBabyMetroidDrainCarryCase(ushort YSpeed, ushort ExpectedX, ushort ExpectedSpeed);
