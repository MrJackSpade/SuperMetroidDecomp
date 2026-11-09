namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Where Mother Brain's glass is in its course (<see cref="MotherBrainGlassObject"/>).</summary>
internal enum MotherBrainGlassPhase
{
    /// <summary>Spawned: its first update picks the cleared glass, the shattered glass or its first stage.</summary>
    Starting,
    /// <summary>Holding a stage until the glass has taken that stage's hits.</summary>
    Holding,
    /// <summary>A stage broke and threw its first shards: the next image shows before the second shards.</summary>
    Bursting,
    /// <summary>The last stage broke: the shattered glass shows before the destroyed event.</summary>
    Shattering,
    /// <summary>The cleared or already-shattered glass was drawn: it leaves on its next update.</summary>
    Resolved,
}
