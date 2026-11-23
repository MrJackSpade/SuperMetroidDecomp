namespace Engine.Shared;

/// <summary>Which way something faces. Every actor uses this one; none has a direction type of its own.</summary>
public enum Facing
{
    /// <summary>Toward the room's left edge.</summary>
    Left,

    /// <summary>Toward the room's right edge.</summary>
    Right,
}
