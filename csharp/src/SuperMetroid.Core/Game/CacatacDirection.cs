namespace SuperMetroid.Core.Game;

/// <summary>
/// Population parameter-one's low-byte domain. The idle animation treats either nonzero
/// value as right, but the initializer's three-word pointer table distinguishes the shipped
/// value two as an initial stop.
/// </summary>
public enum CacatacDirection : ushort
{
    /// <summary>Population and native direction word zero: leftward patrol, restored by animation instruction $A2:A095 after an attack.</summary>
    Left = 0,
    /// <summary>Population and native direction word one: rightward patrol; the restore instruction also treats the separate initial-stop selector two as rightward.</summary>
    Right = 1,
    /// <summary>Population selector two: an initial stop. The word stays two until an attack; the restore instruction then treats it as rightward.</summary>
    InitialStop = 2,
}
