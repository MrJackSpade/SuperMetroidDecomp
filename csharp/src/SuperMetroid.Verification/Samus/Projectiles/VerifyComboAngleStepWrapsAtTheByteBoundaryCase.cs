using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>The pose, step, from and to of one verify combo angle step wraps at the byte boundary case.</summary>
/// <param name="Pose">The pose.</param>
/// <param name="Step">The step.</param>
/// <param name="From">The from.</param>
/// <param name="To">The to.</param>
internal readonly record struct VerifyComboAngleStepWrapsAtTheByteBoundaryCase(SamusPoseId Pose, int Step, ushort From, ushort To);
