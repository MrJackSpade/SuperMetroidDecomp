namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One joint of Mother Brain's neck as its definitions place it (#2314, #2556).</summary>
/// <param name="Distance">The joint's pixel distance from its anchor along its half of the neck's angle.</param>
/// <param name="Exposure">Whether Samus can touch the joint.</param>
internal readonly partial record struct MotherBrainNeckJointDefinition(int Distance, MotherBrainNeckJointExposure Exposure);
