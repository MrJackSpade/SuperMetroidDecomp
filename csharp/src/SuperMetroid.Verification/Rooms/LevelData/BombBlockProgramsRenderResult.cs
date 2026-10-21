namespace SuperMetroid.Verification.Rooms.LevelData;

/// <summary>The physical, immediate and streamed of one bomb block programs render result.</summary>
/// <param name="Physical">The physical.</param>
/// <param name="Immediate">The immediate.</param>
/// <param name="Streamed">The streamed.</param>
internal readonly record struct BombBlockProgramsRenderResult(ushort Physical, ushort Immediate, ushort Streamed);
