using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The hardcoded room-object request made after the escape door fragments spawn.</summary>
public readonly record struct MotherBrainEscapeDoorPlmRequest(
    Position16<RoomBlocks> Block,
    PlmHeaderId PlmEntry);
