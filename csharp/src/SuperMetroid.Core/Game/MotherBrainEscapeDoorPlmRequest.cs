using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The hardcoded room-object request made after the escape door fragments spawn.</summary>
public readonly record struct MotherBrainEscapeDoorPlmRequest(
    Position16<RoomBlocks> Block,
    PlmHeaderId PlmEntry);
