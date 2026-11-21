using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// One pose of Ridley's wing beat (#2329): the installed frame his wings show in it facing left, and the mirrored frame
/// facing right.
/// </summary>
/// <param name="FacingLeft">The frame the pose shows while he faces left.</param>
/// <param name="FacingRight">The frame the pose shows while he faces right.</param>
public readonly record struct RidleyWingPose(EnemyFrameId FacingLeft, EnemyFrameId FacingRight);
