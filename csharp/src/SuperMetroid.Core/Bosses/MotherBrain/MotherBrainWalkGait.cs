namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The walking animations of one of Mother Brain's walking speeds (#2471), one each way.</summary>
/// <param name="Forwards">The body animation she walks forwards in at the speed.</param>
/// <param name="Backwards">The body animation she walks backwards in at the speed.</param>
internal readonly partial record struct MotherBrainWalkGait(MotherBrainBodySegment Forwards, MotherBrainBodySegment Backwards);
