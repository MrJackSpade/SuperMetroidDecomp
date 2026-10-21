using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Verification.Rooms;

/// <summary>The name, expected room and use real landing door of one verify room artwork render parity case.</summary>
/// <param name="Name">The name.</param>
/// <param name="ExpectedRoom">The expected room.</param>
/// <param name="UseRealLandingDoor">The use real landing door.</param>
internal readonly record struct VerifyRoomArtworkRenderParityCase(string Name, RoomHeaderDefinition ExpectedRoom, bool UseRealLandingDoor);
