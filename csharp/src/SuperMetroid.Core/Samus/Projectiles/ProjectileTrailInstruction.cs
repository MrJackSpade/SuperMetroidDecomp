namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The beam-trail instructions; either stream may execute any of them.</summary>
public enum ProjectileTrailInstruction
{
    /// <summary>Left-stream step that moves its sprite one pixel down.</summary>
    MoveLeftDown,
    /// <summary>Right-stream step that moves its sprite one pixel down.</summary>
    MoveRightDown,
    /// <summary>Left-stream step that moves its sprite one pixel up.</summary>
    MoveLeftUp,
}
