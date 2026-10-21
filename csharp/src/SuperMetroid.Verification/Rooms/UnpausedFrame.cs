namespace SuperMetroid.Verification.Rooms;

/// <summary>The source and frame of one unpaused frame.</summary>
/// <param name="Source">The source.</param>
/// <param name="Frame">The frame.</param>
internal readonly record struct UnpausedFrame(ushort Source, int Frame);
