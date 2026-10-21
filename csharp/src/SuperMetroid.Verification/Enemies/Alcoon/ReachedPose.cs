using SuperMetroid.Core.Enemies.Alcoon;

namespace SuperMetroid.Verification.Enemies.Alcoon;

/// <summary>The program and step of one reached pose.</summary>
/// <param name="Program">The program.</param>
/// <param name="Step">The step.</param>
internal readonly record struct ReachedPose(AlcoonProgram Program, int Step);
