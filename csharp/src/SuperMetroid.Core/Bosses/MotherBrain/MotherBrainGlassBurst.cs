
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The two cues a burst of Mother Brain's glass emits.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
internal readonly record struct MotherBrainGlassBurst(MotherBrainGlassPlmCue First, MotherBrainGlassPlmCue Second);
