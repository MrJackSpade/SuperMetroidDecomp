namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The phase a dead Zoomer, Ripper or Skree corpse's main AI runs.</summary>
public enum DeadTourianCorpsePhase
{
    /// <summary>Waits for Samus to collide with the solid corpse.</summary>
    Waiting,
    /// <summary>Counts the pre-rot delay's AI updates after the collision before rotting.</summary>
    PreRotDelay,
    /// <summary>Rots the corpse one step of its rot schedule each update, its pixel rows falling from the foot up.</summary>
    Rotting,
    /// <summary>Does nothing: every row of the corpse has fallen.</summary>
    Finished,
}
