namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The phase a dead Zoomer, Ripper or Skree corpse's main AI runs.</summary>
public enum DeadTourianCorpseFunction
{
    /// <summary>Waits for Samus to collide with the solid corpse.</summary>
    Waiting,
    /// <summary>Counts sixteen AI calls after the collision before rotting.</summary>
    PreRotDelay,
    /// <summary>Rots one step of the corpse's rotting table per AI call.</summary>
    Rotting,
    /// <summary>Does nothing: the rotting table is exhausted.</summary>
    Finished,
}
