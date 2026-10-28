using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The draw each block-reaction image draws.</summary>
internal static class BlockReactionPlmDrawDefinitions
{
    /// <summary>Each image's draw.</summary>
    internal static IReadOnlyDictionary<BlockReactionPlmFrame, PlmDraw> DrawLists { get; } = new Dictionary<BlockReactionPlmFrame, PlmDraw>
    {
        [BlockReactionPlmFrame.SingleBreakup0] = ShotBlockPlmDraws.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Single, 0),
        [BlockReactionPlmFrame.SingleBreakup1] = ShotBlockPlmDraws.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Single, 1),
        [BlockReactionPlmFrame.SingleBreakup2] = ShotBlockPlmDraws.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Single, 2),
        [BlockReactionPlmFrame.SingleBreakup3] = ShotBlockPlmDraws.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Single, 3),
        [BlockReactionPlmFrame.HorizontalBreakup0] = ShotBlockPlmDraws.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Horizontal, 0),
        [BlockReactionPlmFrame.HorizontalBreakup1] = ShotBlockPlmDraws.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Horizontal, 1),
        [BlockReactionPlmFrame.HorizontalBreakup2] = ShotBlockPlmDraws.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Horizontal, 2),
        [BlockReactionPlmFrame.HorizontalBreakup3] = ShotBlockPlmDraws.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Horizontal, 3),
        [BlockReactionPlmFrame.VerticalBreakup0] = ShotBlockPlmDraws.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Vertical, 0),
        [BlockReactionPlmFrame.VerticalBreakup1] = ShotBlockPlmDraws.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Vertical, 1),
        [BlockReactionPlmFrame.VerticalBreakup2] = ShotBlockPlmDraws.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Vertical, 2),
        [BlockReactionPlmFrame.VerticalBreakup3] = ShotBlockPlmDraws.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Vertical, 3),
        [BlockReactionPlmFrame.SquareBreakup0] = ShotBlockPlmDraws.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Square, 0),
        [BlockReactionPlmFrame.SquareBreakup1] = ShotBlockPlmDraws.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Square, 1),
        [BlockReactionPlmFrame.SquareBreakup2] = ShotBlockPlmDraws.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Square, 2),
        [BlockReactionPlmFrame.SquareBreakup3] = ShotBlockPlmDraws.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Square, 3),
        [BlockReactionPlmFrame.ShotRestoreHorizontal] = ShotBlockPlmDraws.Restoration(RoomPlmShotBlockDrawDefinitions.Shape.Horizontal),
        [BlockReactionPlmFrame.ShotRestoreVertical] = ShotBlockPlmDraws.Restoration(RoomPlmShotBlockDrawDefinitions.Shape.Vertical),
        [BlockReactionPlmFrame.ShotRestoreSquare] = ShotBlockPlmDraws.Restoration(RoomPlmShotBlockDrawDefinitions.Shape.Square),
        [BlockReactionPlmFrame.BombRestoreHorizontal] = PlmDraw.Of(BombBlockRestoreDraw.Horizontal),
        [BlockReactionPlmFrame.BombRestoreVertical] = PlmDraw.Of(BombBlockRestoreDraw.Vertical),
        [BlockReactionPlmFrame.BombRestoreSquare] = PlmDraw.Of(BombBlockRestoreDraw.Square),
        [BlockReactionPlmFrame.CrumbleRevealSingle] = PlmDraw.Of(RoomPlmBombedRevealDraw.CrumbleSingle),
        [BlockReactionPlmFrame.CrumbleRevealHorizontal] = PlmDraw.Of(ContactCrumbleRestoreDraw.Horizontal),
        [BlockReactionPlmFrame.CrumbleRevealVertical] = PlmDraw.Of(ContactCrumbleRestoreDraw.Vertical),
        [BlockReactionPlmFrame.CrumbleRevealSquare] = PlmDraw.Of(ContactCrumbleRestoreDraw.Square),
        [BlockReactionPlmFrame.PowerBombReveal] = PlmDraw.Of(RoomPlmBombedRevealDraw.PowerBomb),
        [BlockReactionPlmFrame.SuperMissileReveal] = PlmDraw.Of(RoomPlmBombedRevealDraw.SuperMissile),
        [BlockReactionPlmFrame.GrappleBlock] = PlmDraw.Of(GrappleBlockDraw.Intact),
        [BlockReactionPlmFrame.GrappleBreak0] = PlmDraw.Of(GrappleBlockDraw.Break0),
        [BlockReactionPlmFrame.GrappleBreak1] = PlmDraw.Of(GrappleBlockDraw.Break1),
        [BlockReactionPlmFrame.GrappleBreak2] = PlmDraw.Of(GrappleBlockDraw.Break2),
        [BlockReactionPlmFrame.GrappleBlank] = PlmDraw.Of(GrappleBlockDraw.Blank),
    };
}
