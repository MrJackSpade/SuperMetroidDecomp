namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>What a grapple connection holds on to, which decides who validates it on following frames.</summary>
public enum GrappleAnchorKind
{
    /// <summary>A grapple block in the room.</summary>
    Block,

    /// <summary>An enemy, such as a Powamp.</summary>
    Enemy,
}
