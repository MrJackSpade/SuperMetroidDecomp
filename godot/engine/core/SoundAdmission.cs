namespace Engine.Core;

/// <summary>What became of a sound that was asked for.</summary>
/// <param name="Admitted">Whether it was let in, by its priority and by how many were waiting.</param>
/// <param name="Waiting">How many sounds were waiting to be heard where it would wait, as it was asked for.</param>
public readonly record struct SoundAdmission(bool Admitted, int Waiting);
