namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One of the eight door-fragment allocation attempts, with the fragment it throws.</summary>
/// <param name="Fragment">The fragment the attempt throws: where it starts beside the door and how it flies.</param>
public readonly record struct MotherBrainEscapeDoorParticleSpawnRequest(MotherBrainDoorFragmentDefinition Fragment);
