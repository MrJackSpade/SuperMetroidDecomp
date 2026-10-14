namespace SuperMetroid.Core.Game;

/// <summary>One turret frame: its program and its direction or smoke phase.</summary>
/// <param name="Program">The program the frame belongs to.</param>
/// <param name="Index">The direction of a pose, or the phase of the smoke.</param>
public readonly record struct MotherBrainTurretFrame(MotherBrainTurretProgram Program, int Index);
