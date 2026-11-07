using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>The first, second and third of one noob tube cue at case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
internal readonly record struct NoobTubeCueAtCase(bool First, NoobTubeCue Second, int Third);
