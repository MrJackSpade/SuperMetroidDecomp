namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One stage of Mother Brain's glass breaking (<see cref="MotherBrainGlassStageDefinitions"/>).</summary>
/// <param name="Image">The glass the stage shows, redrawn on every update the stage holds.</param>
/// <param name="BreaksAtHits">The hits the glass must have taken for the stage to break.</param>
/// <param name="Burst">The shards the stage throws as it breaks, around the next stage's image; null for none.</param>
internal sealed record MotherBrainGlassStage(MotherBrainGlassDraw Image, int BreaksAtHits, MotherBrainGlassBurst? Burst);
