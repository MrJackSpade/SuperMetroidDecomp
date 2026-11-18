namespace SuperMetroid.Core.Samus.Death;

/// <summary>What one update of Samus's death draws of her.</summary>
public enum SamusDeathDraw
{
    /// <summary>Nothing: the explosion has played its last picture, or the sequence is complete.</summary>
    Nothing,

    /// <summary>Her death pose, with no invincibility flicker.</summary>
    Pose,

    /// <summary>The suit's explosion, on its current picture.</summary>
    Explosion,
}
