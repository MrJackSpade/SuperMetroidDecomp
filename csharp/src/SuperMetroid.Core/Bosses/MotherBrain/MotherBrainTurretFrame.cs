namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One turret frame: its animation and its direction or smoke phase.</summary>
/// <param name="Animation">The animation the frame belongs to.</param>
/// <param name="Index">The direction of a pose, or the phase of the smoke.</param>
public readonly record struct MotherBrainTurretFrame(MotherBrainTurretSegment Animation, int Index);
