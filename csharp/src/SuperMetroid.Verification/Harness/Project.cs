namespace SuperMetroid.Verification.Harness;

/// <summary>The definition and program of one project.</summary>
/// <param name="Definition">The definition.</param>
/// <param name="Program">The program.</param>
internal readonly record struct Project(ushort Definition, ushort Program);
