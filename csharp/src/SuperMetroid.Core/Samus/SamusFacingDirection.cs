namespace SuperMetroid.Core.Samus;

/// <summary>The way a pose faces. This is not a flags enum: a pose has one facing.</summary>
public enum SamusFacingDirection
{
    /// <summary>Facing neither left nor right: the front views, and the unauthored poses that face neither way.</summary>
    ForwardOrSpecial,
    /// <summary>Facing left, selecting left-facing movement and artwork.</summary>
    Left,
    /// <summary>Facing right, selecting right-facing movement and artwork.</summary>
    Right,
}
