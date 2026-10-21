namespace SuperMetroid.Verification.Rooms.LevelData;

/// <summary>The physical and streamed of one render restored parent result.</summary>
/// <param name="Physical">The physical.</param>
/// <param name="Streamed">The streamed.</param>
internal readonly record struct RenderRestoredParentResult(ushort Physical, ushort Streamed);
