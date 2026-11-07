using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>The first, second and third of one downward gate cue at case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
internal readonly record struct DownwardGateCueAtCase(bool First, DownwardGateCue Second, int Third);
