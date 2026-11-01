namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Result of the slope correction that follows a horizontal move.</summary>
/// <param name="Position">The probed whole-pixel position with its Y corrected onto the slope when one applies.</param>
/// <param name="Adjusted">Whether a slope corrected the Y.</param>
public readonly record struct SlopeAlignmentResult(
    Position16<RoomPixels> Position,
    bool Adjusted);
