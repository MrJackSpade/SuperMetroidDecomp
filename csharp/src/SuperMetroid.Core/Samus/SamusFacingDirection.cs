namespace SuperMetroid.Core.Samus;

/// <summary>
/// The way a pose faces. This is not a flags enum: a pose has one facing. The pose-definition record's facing byte
/// belongs to <see cref="Poses.SamusPoseRecordCodec"/>.
/// </summary>
public enum SamusFacingDirection : byte
{
    /// <summary>Pose-definition value zero, used by front-view artwork and poses without ordinary left/right facing.</summary>
    ForwardOrSpecial = 0,
    /// <summary>Pose-definition value four, selecting left-facing metadata and artwork.</summary>
    Left = 4,
    /// <summary>Pose-definition value eight, selecting right-facing metadata and artwork.</summary>
    Right = 8,
}
