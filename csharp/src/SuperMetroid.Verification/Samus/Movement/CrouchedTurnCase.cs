using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>The source pose, generic turn, selected turn and destination of one crouched turn case.</summary>
/// <param name="SourcePose">The source pose.</param>
/// <param name="GenericTurn">The generic turn.</param>
/// <param name="SelectedTurn">The selected turn.</param>
/// <param name="Destination">The destination.</param>
internal readonly record struct CrouchedTurnCase(SamusPoseId SourcePose, SamusPoseId GenericTurn, SamusPoseId SelectedTurn, SamusPoseId Destination);
