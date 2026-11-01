namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The body point whose block an inside-block reaction is running for.</summary>
public enum SamusInsideBlockPoint
{
    /// <summary><c>InsideBlockReactionSamusPoint</c> = 0: one pixel above the bottom boundary.</summary>
    Bottom,

    /// <summary><c>InsideBlockReactionSamusPoint</c> = 1: Samus's Y position.</summary>
    Center,

    /// <summary><c>InsideBlockReactionSamusPoint</c> = 2: Y position minus the Y radius.</summary>
    Top,
}
