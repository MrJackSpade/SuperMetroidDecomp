using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>The pose and definition of one samus special states pose.</summary>
/// <param name="Pose">The pose.</param>
/// <param name="Definition">The definition.</param>
internal readonly record struct SamusSpecialStatesPose(SamusPoseId Pose, byte[] Definition);
