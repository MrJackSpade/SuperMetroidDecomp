namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>The physical, immediate and streamed of one grapple block programs render result.</summary>
/// <param name="Physical">The physical.</param>
/// <param name="Immediate">The immediate.</param>
/// <param name="Streamed">The streamed.</param>
internal readonly record struct GrappleBlockProgramsRenderResult(ushort Physical, ushort Immediate, ushort Streamed);
