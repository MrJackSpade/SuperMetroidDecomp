namespace SuperMetroid.Verification.Rooms.LevelData;

/// <summary>The collision, reaction, tail, frames and end of one bomb block native layouts result.</summary>
/// <param name="Collision">The collision.</param>
/// <param name="Reaction">The reaction.</param>
/// <param name="Tail">The tail.</param>
/// <param name="Frames">The frames.</param>
/// <param name="End">The end.</param>
internal readonly record struct BombBlockNativeLayoutsResult(ushort Collision, ushort Reaction, ushort Tail, int Frames, ushort End);
