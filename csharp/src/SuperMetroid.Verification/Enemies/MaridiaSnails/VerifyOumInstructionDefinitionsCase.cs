using SuperMetroid.Core.Enemies.MaridiaSnails;

namespace SuperMetroid.Verification.Enemies.MaridiaSnails;

/// <summary>The roll and window pose of one verify Oum instruction definitions case.</summary>
/// <param name="Roll">The roll.</param>
/// <param name="WindowPose">The window pose.</param>
internal readonly record struct VerifyOumInstructionDefinitionsCase(OumSegment Roll, int WindowPose);
