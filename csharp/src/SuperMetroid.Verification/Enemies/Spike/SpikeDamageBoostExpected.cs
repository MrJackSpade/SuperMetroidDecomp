using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Enemies.Spike;

/// <summary>The x, y, pose and base of one spike damage boost expected.</summary>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
/// <param name="Pose">The pose.</param>
/// <param name="Base">The base.</param>
internal readonly record struct SpikeDamageBoostExpected(uint X, uint Y, SamusPoseId Pose, uint Base);
