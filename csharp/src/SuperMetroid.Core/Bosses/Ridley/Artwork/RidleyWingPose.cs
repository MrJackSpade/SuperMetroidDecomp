using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// One pose of Ridley's wing beat (#2329): the installed frame his wings show in it facing left, and the mirrored frame
/// facing right.
/// </summary>
/// <param name="FacingLeft">The frame the pose shows while he faces left.</param>
/// <param name="FacingRight">The frame the pose shows while he faces right.</param>
public readonly record struct RidleyWingPose(EnemyFrameId FacingLeft, EnemyFrameId FacingRight)
{
    /// <summary>The frame the pose shows for <paramref name="facing"/>.</summary>
    /// <exception cref="InvalidOperationException">Ridley turns toward the screen, where his wings show no frame.</exception>
    internal EnemyFrameId For(RidleyFacing facing) => facing switch
    {
        RidleyFacing.Left => FacingLeft,
        RidleyFacing.Right => FacingRight,
        RidleyFacing.Turning => throw new InvalidOperationException("Ridley turning toward the screen shows no wings."),
        _ => throw new InvalidOperationException($"Undefined {nameof(RidleyFacing)} {facing}."),
    };
}
