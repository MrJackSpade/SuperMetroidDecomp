namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A request to throw one fragment of the escape door
/// (<see cref="MotherBrainDoorFragmentDefinition"/>).</summary>
/// <param name="Fragment">The fragment the attempt throws: where it starts beside the door and how it flies.</param>
public readonly record struct MotherBrainEscapeDoorParticleSpawnRequest(MotherBrainDoorFragmentDefinition Fragment);
