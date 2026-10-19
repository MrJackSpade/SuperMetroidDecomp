namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>What a block-reaction PLM draws at its block: a breakup image by block shape, a restore or reveal image, a grapple block
/// image, or the saved block itself.</summary>
internal enum BlockReactionPlmFrame
{
    /// <summary>The SingleBreakup0 image.</summary>
    SingleBreakup0,
    /// <summary>The SingleBreakup1 image.</summary>
    SingleBreakup1,
    /// <summary>The SingleBreakup2 image.</summary>
    SingleBreakup2,
    /// <summary>The SingleBreakup3 image.</summary>
    SingleBreakup3,
    /// <summary>The HorizontalBreakup0 image.</summary>
    HorizontalBreakup0,
    /// <summary>The HorizontalBreakup1 image.</summary>
    HorizontalBreakup1,
    /// <summary>The HorizontalBreakup2 image.</summary>
    HorizontalBreakup2,
    /// <summary>The HorizontalBreakup3 image.</summary>
    HorizontalBreakup3,
    /// <summary>The VerticalBreakup0 image.</summary>
    VerticalBreakup0,
    /// <summary>The VerticalBreakup1 image.</summary>
    VerticalBreakup1,
    /// <summary>The VerticalBreakup2 image.</summary>
    VerticalBreakup2,
    /// <summary>The VerticalBreakup3 image.</summary>
    VerticalBreakup3,
    /// <summary>The SquareBreakup0 image.</summary>
    SquareBreakup0,
    /// <summary>The SquareBreakup1 image.</summary>
    SquareBreakup1,
    /// <summary>The SquareBreakup2 image.</summary>
    SquareBreakup2,
    /// <summary>The SquareBreakup3 image.</summary>
    SquareBreakup3,
    /// <summary>The ShotRestoreHorizontal image.</summary>
    ShotRestoreHorizontal,
    /// <summary>The ShotRestoreVertical image.</summary>
    ShotRestoreVertical,
    /// <summary>The ShotRestoreSquare image.</summary>
    ShotRestoreSquare,
    /// <summary>The BombRestoreHorizontal image.</summary>
    BombRestoreHorizontal,
    /// <summary>The BombRestoreVertical image.</summary>
    BombRestoreVertical,
    /// <summary>The BombRestoreSquare image.</summary>
    BombRestoreSquare,
    /// <summary>The CrumbleRevealSingle image.</summary>
    CrumbleRevealSingle,
    /// <summary>The CrumbleRevealHorizontal image.</summary>
    CrumbleRevealHorizontal,
    /// <summary>The CrumbleRevealVertical image.</summary>
    CrumbleRevealVertical,
    /// <summary>The CrumbleRevealSquare image.</summary>
    CrumbleRevealSquare,
    /// <summary>The PowerBombReveal image.</summary>
    PowerBombReveal,
    /// <summary>The SuperMissileReveal image.</summary>
    SuperMissileReveal,
    /// <summary>The GrappleBlock image.</summary>
    GrappleBlock,
    /// <summary>The GrappleBreak0 image.</summary>
    GrappleBreak0,
    /// <summary>The GrappleBreak1 image.</summary>
    GrappleBreak1,
    /// <summary>The GrappleBreak2 image.</summary>
    GrappleBreak2,
    /// <summary>The GrappleBlank image.</summary>
    GrappleBlank,
    /// <summary>The slot's saved level word, redrawn as a respawning block returns.</summary>
    RestoredBlock,
}
