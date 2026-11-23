namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The room a knockback's hurt pose grows into, and the update its changed-pose collision runs on.</summary>
/// <param name="Terrain">The room's level and room objects the larger hurt pose is tested against.</param>
/// <param name="FrameCounter">The update's frame counter, which picks the collision's scan direction.</param>
internal readonly partial record struct KnockbackPoseCollision(
    SamusTerrain Terrain,
    int FrameCounter);
