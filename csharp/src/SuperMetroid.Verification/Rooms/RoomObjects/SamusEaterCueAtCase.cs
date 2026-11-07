using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>The first, second and third of one samus eater cue at case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
internal readonly record struct SamusEaterCueAtCase(bool First, SamusEaterCue Second, int Third);
