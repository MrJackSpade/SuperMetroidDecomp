using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>The first, second and third of one blue door cue at case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
internal readonly record struct BlueDoorCueAtCase(bool First, BlueDoorCue Second, int Third);
