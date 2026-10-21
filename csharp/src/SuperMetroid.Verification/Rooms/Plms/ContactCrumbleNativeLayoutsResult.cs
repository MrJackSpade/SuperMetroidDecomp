namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>The start, frames and end of one contact crumble native layouts result.</summary>
/// <param name="Start">The start.</param>
/// <param name="Frames">The frames.</param>
/// <param name="End">The end.</param>
internal readonly record struct ContactCrumbleNativeLayoutsResult(ushort Start, int Frames, ushort End);
