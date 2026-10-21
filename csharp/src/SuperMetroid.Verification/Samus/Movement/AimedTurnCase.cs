using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>The source pose, generic turn, selected turn, destination and destination delay of one aimed turn case.</summary>
/// <param name="SourcePose">The source pose.</param>
/// <param name="GenericTurn">The generic turn.</param>
/// <param name="SelectedTurn">The selected turn.</param>
/// <param name="Destination">The destination.</param>
/// <param name="DestinationDelay">The destination delay.</param>
internal readonly record struct AimedTurnCase(SamusPoseId SourcePose, SamusPoseId GenericTurn, SamusPoseId SelectedTurn, SamusPoseId Destination, byte DestinationDelay);
