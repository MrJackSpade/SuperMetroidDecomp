namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>What a corpse-rotting step does to one pixel row.</summary>
public enum CorpseRowOperation
{
    /// <summary>Copies the row down, leaving its source in place.</summary>
    Copy,

    /// <summary>Moves the row down, clearing its source.</summary>
    Move,
}
