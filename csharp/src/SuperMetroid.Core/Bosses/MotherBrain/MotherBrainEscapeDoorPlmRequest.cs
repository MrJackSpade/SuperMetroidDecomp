namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The room mutation Mother Brain's death requests once the escape door's fragments spawn.</summary>
/// <param name="Block">The room block the door opens at.</param>
/// <param name="PlmEntry">The mutation that opens it: the escape door.</param>
public readonly record struct MotherBrainEscapeDoorPlmRequest(
    Position16<RoomBlocks> Block,
    MotherBrainRoomMutation PlmEntry);
