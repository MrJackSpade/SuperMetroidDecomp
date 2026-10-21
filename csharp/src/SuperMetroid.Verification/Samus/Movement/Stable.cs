using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>The pose, definition, fallback and direction of one stable.</summary>
/// <param name="Pose">The pose.</param>
/// <param name="Definition">The definition.</param>
/// <param name="Fallback">The fallback.</param>
/// <param name="Direction">The direction.</param>
internal readonly record struct Stable(SamusPoseId Pose, byte[] Definition, SamusPoseId Fallback, int Direction);
