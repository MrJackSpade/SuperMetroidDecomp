namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One glass frame: its program and its position there.</summary>
/// <param name="Program">The program the frame belongs to.</param>
/// <param name="Phase">The frame's position in that program.</param>
public readonly record struct MotherBrainGlassFrame(MotherBrainGlassProgram Program, int Phase);
