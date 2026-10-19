namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The bank-$84 draw list each block-reaction image draws.</summary>
internal static class BlockReactionPlmDrawDefinitions
{
    /// <summary>Each image's draw list.</summary>
    internal static IReadOnlyDictionary<BlockReactionPlmFrame, ushort> DrawLists { get; } = new Dictionary<BlockReactionPlmFrame, ushort>
    {
        [BlockReactionPlmFrame.SingleBreakup0] = 0xa345,
        [BlockReactionPlmFrame.SingleBreakup1] = 0xa34b,
        [BlockReactionPlmFrame.SingleBreakup2] = 0xa351,
        [BlockReactionPlmFrame.SingleBreakup3] = 0xa357,
        [BlockReactionPlmFrame.HorizontalBreakup0] = 0xa35d,
        [BlockReactionPlmFrame.HorizontalBreakup1] = 0xa365,
        [BlockReactionPlmFrame.HorizontalBreakup2] = 0xa36d,
        [BlockReactionPlmFrame.HorizontalBreakup3] = 0xa375,
        [BlockReactionPlmFrame.VerticalBreakup0] = 0xa37d,
        [BlockReactionPlmFrame.VerticalBreakup1] = 0xa385,
        [BlockReactionPlmFrame.VerticalBreakup2] = 0xa38d,
        [BlockReactionPlmFrame.VerticalBreakup3] = 0xa395,
        [BlockReactionPlmFrame.SquareBreakup0] = 0xa39d,
        [BlockReactionPlmFrame.SquareBreakup1] = 0xa3ad,
        [BlockReactionPlmFrame.SquareBreakup2] = 0xa3bd,
        [BlockReactionPlmFrame.SquareBreakup3] = 0xa3cd,
        [BlockReactionPlmFrame.ShotRestoreHorizontal] = 0xa47b,
        [BlockReactionPlmFrame.ShotRestoreVertical] = 0xa483,
        [BlockReactionPlmFrame.ShotRestoreSquare] = 0xa48b,
        [BlockReactionPlmFrame.BombRestoreHorizontal] = 0xa4c7,
        [BlockReactionPlmFrame.BombRestoreVertical] = 0xa4cf,
        [BlockReactionPlmFrame.BombRestoreSquare] = 0xa4d7,
        [BlockReactionPlmFrame.CrumbleRevealSingle] = 0xa49b,
        [BlockReactionPlmFrame.CrumbleRevealHorizontal] = 0xa4a1,
        [BlockReactionPlmFrame.CrumbleRevealVertical] = 0xa4a9,
        [BlockReactionPlmFrame.CrumbleRevealSquare] = 0xa4b1,
        [BlockReactionPlmFrame.PowerBombReveal] = 0xa4e7,
        [BlockReactionPlmFrame.SuperMissileReveal] = 0xa4ed,
        [BlockReactionPlmFrame.GrappleBlock] = 0xa4f9,
        [BlockReactionPlmFrame.GrappleBreak0] = 0xa4ff,
        [BlockReactionPlmFrame.GrappleBreak1] = 0xa505,
        [BlockReactionPlmFrame.GrappleBreak2] = 0xa50b,
        [BlockReactionPlmFrame.GrappleBlank] = 0xa511,
    };
}
