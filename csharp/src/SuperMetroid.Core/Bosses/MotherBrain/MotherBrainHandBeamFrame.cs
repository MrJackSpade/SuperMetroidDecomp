namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One frame of the red hand beam's program: its stage and its position there.</summary>
/// <param name="Stage">One of the three identical stages.</param>
/// <param name="Phase">The frame's position in its stage.</param>
public readonly record struct MotherBrainHandBeamFrame(int Stage, int Phase);
