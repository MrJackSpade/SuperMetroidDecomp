using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>The turn pose and definition of one aimed turn.</summary>
/// <param name="TurnPose">The turn pose.</param>
/// <param name="Definition">The definition.</param>
internal readonly record struct AimedTurn(SamusPoseId TurnPose, byte[] Definition);
