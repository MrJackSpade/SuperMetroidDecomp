using SuperMetroid.Core.Enemies.MorphBallEye;

namespace SuperMetroid.Verification.Enemies.MorphBallEye;

/// <summary>The parameter1 and closed of one verify morph ball eye behavior case.</summary>
/// <param name="Parameter1">The parameter1.</param>
/// <param name="Closed">The closed.</param>
internal readonly record struct VerifyMorphBallEyeBehaviorCase(ushort Parameter1, MorphBallEyeProgram Closed);
