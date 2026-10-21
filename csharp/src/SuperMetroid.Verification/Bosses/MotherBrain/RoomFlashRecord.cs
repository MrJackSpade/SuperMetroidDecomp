namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>The frame and record of one room flash record.</summary>
/// <param name="Frame">The frame.</param>
/// <param name="Record">The record.</param>
internal readonly record struct RoomFlashRecord(int Frame, int Record);
