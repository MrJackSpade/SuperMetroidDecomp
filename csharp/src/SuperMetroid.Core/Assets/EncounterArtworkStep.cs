namespace SuperMetroid.Core.Assets;

/// <summary>A step an encounter's artwork sequence has reached, as a frame's picture names it (#1513).</summary>
/// <param name="Sequence">The encounter's artwork sequence.</param>
/// <param name="Step">The step of the sequence whose picture the frame shows.</param>
public readonly record struct EncounterArtworkStep(EncounterArtworkSequence Sequence, int Step);
