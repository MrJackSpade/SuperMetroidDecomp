using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>The source pose and target pose of one samus morph ball verify samus morph ball movement case.</summary>
/// <param name="SourcePose">The source pose.</param>
/// <param name="TargetPose">The target pose.</param>
internal readonly record struct SamusMorphBallVerifySamusMorphBallMovementCase(SamusPoseId SourcePose, SamusPoseId TargetPose);
