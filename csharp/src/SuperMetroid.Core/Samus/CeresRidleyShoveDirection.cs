namespace SuperMetroid.Core.Samus;

/// <summary>The wall Ceres Ridley's getaway shoves Samus toward, chosen from her screen half when the shove starts.</summary>
public enum CeresRidleyShoveDirection
{
    /// <summary>No shove is in progress: before initialization and after wall contact ends it.</summary>
    None,

    /// <summary>Samus started on the left half of the screen and is shoved toward the left wall.</summary>
    Left,

    /// <summary>Samus started at or right of the screen centre and is shoved toward the right wall.</summary>
    Right,
}
