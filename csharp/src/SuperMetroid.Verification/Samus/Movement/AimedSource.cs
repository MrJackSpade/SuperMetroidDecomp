using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>The source pose and definition of one aimed source.</summary>
/// <param name="SourcePose">The source pose.</param>
/// <param name="Definition">The definition.</param>
internal readonly record struct AimedSource(SamusPoseId SourcePose, byte[] Definition);
