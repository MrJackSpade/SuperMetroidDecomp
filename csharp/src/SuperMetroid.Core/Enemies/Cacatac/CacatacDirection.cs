namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// Population parameter one's direction, its lower half. The idle animation treats either nonzero
/// value as right, but the initializer's three-entry table distinguishes the shipped
/// value two as an initial stop.
/// </summary>
public enum CacatacDirection
{
    /// <summary>Population and direction value zero: leftward patrol, restored by the restore-patrol animation cue after an attack.</summary>
    Left = 0,
    /// <summary>Population and direction value one: rightward patrol; the restore cue also treats the separate initial-stop selector two as rightward.</summary>
    Right = 1,
    /// <summary>Population selector two: an initial stop. The value stays two until an attack; the restore cue then treats it as rightward.</summary>
    InitialStop = 2,
}
