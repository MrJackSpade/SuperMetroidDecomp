namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Which of the grapple's directional connection policies a connecting beam reads (#1411).</summary>
public enum GrappleConnectionTable
{
    /// <summary>The default policy.</summary>
    Default,

    /// <summary>The policy while Samus moves vertically.</summary>
    MovingVertically,

    /// <summary>The policy while Samus crouches.</summary>
    Crouching,
}
