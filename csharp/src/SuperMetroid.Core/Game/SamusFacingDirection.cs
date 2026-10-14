namespace SuperMetroid.Core.Game;

/// <summary>
/// Verified pose-definition byte-zero values. This is not a flags enum: a pose has one
/// facing discriminator, and zero remains meaningful for front-view/special records.
/// Unnamed cartridge values remain losslessly representable through the underlying byte.
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
