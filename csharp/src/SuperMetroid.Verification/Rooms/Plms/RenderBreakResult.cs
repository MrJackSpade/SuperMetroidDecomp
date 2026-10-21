namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>The physical, immediate and streamed of one render break result.</summary>
/// <param name="Physical">The physical.</param>
/// <param name="Immediate">The immediate.</param>
/// <param name="Streamed">The streamed.</param>
internal readonly record struct RenderBreakResult(ushort Physical, ushort Immediate, ushort Streamed);
