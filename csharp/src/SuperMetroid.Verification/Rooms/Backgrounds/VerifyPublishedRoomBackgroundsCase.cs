using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Verification.Rooms.Backgrounds;

/// <summary>The room and streams of one verify published room backgrounds case.</summary>
/// <param name="Room">The room.</param>
/// <param name="Streams">The streams.</param>
internal readonly record struct VerifyPublishedRoomBackgroundsCase(RoomHeaderDefinition Room, bool Streams);
