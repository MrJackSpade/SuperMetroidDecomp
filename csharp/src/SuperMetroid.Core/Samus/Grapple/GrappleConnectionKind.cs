namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>The kinds of grapple connection a fired beam makes once it has put Samus in its pose; a swing's stops name their own phase.</summary>
public enum GrappleConnectionKind
{
    /// <summary>Samus is held in place at the anchor.</summary>
    LockedInPlace,
    /// <summary>Samus swings from the anchor.</summary>
    Swinging,
}
