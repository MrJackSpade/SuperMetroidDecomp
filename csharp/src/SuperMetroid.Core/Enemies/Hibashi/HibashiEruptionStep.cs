namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>One step of a Hibashi eruption's rise: where its collision part stands and how tall and wide it is.</summary>
/// <param name="YOffset">How far above the pillar's spawn height the collision part stands, in pixels.</param>
/// <param name="YRadius">The collision part's half-height, in pixels.</param>
/// <param name="RestoresWidth">Whether the step gives the collision part back its eight-pixel half-width.</param>
internal readonly record struct HibashiEruptionStep(int YOffset, int YRadius, bool RestoresWidth = false);
