using SuperMetroid.Core.Rooms;

namespace SuperMetroid.RenderVerification.Rooms;

/// <summary>A room and the FX it publishes.</summary>
/// <param name="Room">The room.</param>
/// <param name="Fx">The fx.</param>
internal readonly record struct PublishedRoom(RoomHeaderDefinition Room, RetailRoomPublicationTests.PublishedFx Fx);
