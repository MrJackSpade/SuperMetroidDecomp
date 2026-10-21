namespace SuperMetroid.Verification.Fx;

/// <summary>The header, wait, frames and end of one original treadmill programs result.</summary>
/// <param name="Header">The header.</param>
/// <param name="Wait">The wait.</param>
/// <param name="Frames">The frames.</param>
/// <param name="End">The end.</param>
internal readonly record struct OriginalTreadmillProgramsResult(ushort Header, ushort Wait, ushort[] Frames, ushort End);
