namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>The start, frames and restore word of one shot block native program layouts result.</summary>
/// <param name="Start">The start.</param>
/// <param name="Frames">The frames.</param>
/// <param name="RestoreWord">The restore word.</param>
internal readonly record struct ShotBlockNativeProgramLayoutsResult(ushort Start, int Frames, bool RestoreWord);
