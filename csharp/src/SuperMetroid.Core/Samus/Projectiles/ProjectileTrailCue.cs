namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The beam-trail cues; either stream may run any of them.</summary>
public enum ProjectileTrailCue
{
    /// <summary>Left-stream step that moves its sprite one pixel down.</summary>
    MoveLeftDown,
    /// <summary>Right-stream step that moves its sprite one pixel down.</summary>
    MoveRightDown,
    /// <summary>Left-stream step that moves its sprite one pixel up.</summary>
    MoveLeftUp,
}
