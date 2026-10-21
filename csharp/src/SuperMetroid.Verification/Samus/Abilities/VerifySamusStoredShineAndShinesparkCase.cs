using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>The windup pose, target pose, expected phase, expects horizontal and expects vertical of one verify samus stored shine and shinespark case.</summary>
/// <param name="WindupPose">The windup pose.</param>
/// <param name="TargetPose">The target pose.</param>
/// <param name="ExpectedPhase">The expected phase.</param>
/// <param name="ExpectsHorizontal">The expects horizontal.</param>
/// <param name="ExpectsVertical">The expects vertical.</param>
internal readonly record struct VerifySamusStoredShineAndShinesparkCase(SamusPoseId WindupPose, SamusPoseId TargetPose, ShinesparkPhase ExpectedPhase, bool ExpectsHorizontal, bool ExpectsVertical);
