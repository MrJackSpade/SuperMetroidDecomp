namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>How one of Torizo's body rectangles reacts when Samus touches it.</summary>
internal enum TorizoHitboxTouch
{
    /// <summary>Ordinary body contact: Samus takes contact damage and Torizo's own death sequence handles a kill.</summary>
    Body = 1,
}
