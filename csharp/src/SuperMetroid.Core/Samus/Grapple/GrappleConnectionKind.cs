namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>The kinds of grapple connection a connection makes once it has put Samus in its pose.</summary>
public enum GrappleConnectionKind
{
    /// <summary>Samus is held in place at the anchor.</summary>
    LockedInPlace,
    /// <summary>Samus swings from the anchor.</summary>
    Swinging,
    /// <summary>Samus clings to the wall at the anchor.</summary>
    WallGrab,
}
