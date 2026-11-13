namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One glass frame: its program and its position there.</summary>
/// <param name="Animation">The program the frame belongs to.</param>
/// <param name="Phase">The frame's position in that program.</param>
public readonly record struct MotherBrainGlassFrame(MotherBrainGlassSegment Animation, int Phase);
