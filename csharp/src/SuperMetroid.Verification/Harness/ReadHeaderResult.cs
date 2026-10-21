namespace SuperMetroid.Verification.Harness;

/// <summary>The frame and pc of one read header result.</summary>
/// <param name="Frame">The frame.</param>
/// <param name="Pc">The pc.</param>
internal readonly record struct ReadHeaderResult(int Frame, int Pc);
