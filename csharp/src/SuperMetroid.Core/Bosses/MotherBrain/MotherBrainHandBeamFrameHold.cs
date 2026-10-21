namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A hand-beam animation frame and the number of frames it shows.</summary>
/// <param name="Phase">The phase.</param>
/// <param name="Duration">The duration.</param>
internal readonly record struct MotherBrainHandBeamFrameHold(int Phase, int Duration);
