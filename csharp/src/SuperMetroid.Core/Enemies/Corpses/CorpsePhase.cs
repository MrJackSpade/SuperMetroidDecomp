namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The phase a corpse that Samus stands on before it rots runs (#2557): the dead Torizo's, and the dead Zoomer's, Ripper's
/// and Skree's. Its values are arbitrary.
/// </summary>
public enum CorpsePhase
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
