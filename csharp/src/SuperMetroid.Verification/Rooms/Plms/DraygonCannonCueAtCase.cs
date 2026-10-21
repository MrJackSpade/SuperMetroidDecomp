using SuperMetroid.Core.Bosses.Draygon;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>The first, second and third of one draygon cannon cue at case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
internal readonly record struct DraygonCannonCueAtCase(bool First, DraygonCannonCue Second, int Third);
