using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The draw each block-reaction image draws.</summary>
internal static class BlockReactionObjectDrawDefinitions
{
    /// <summary>Each image's draw.</summary>
    internal static IReadOnlyDictionary<BlockReactionObjectFrame, RoomObjectDraw> DrawLists { get; } = new Dictionary<BlockReactionObjectFrame, RoomObjectDraw>
    {
        [BlockReactionObjectFrame.SingleBreakup0] = ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 0),
        [BlockReactionObjectFrame.SingleBreakup1] = ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 1),
        [BlockReactionObjectFrame.SingleBreakup2] = ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 2),
        [BlockReactionObjectFrame.SingleBreakup3] = ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 3),
        [BlockReactionObjectFrame.HorizontalBreakup0] = ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Horizontal, 0),
        [BlockReactionObjectFrame.HorizontalBreakup1] = ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Horizontal, 1),
        [BlockReactionObjectFrame.HorizontalBreakup2] = ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Horizontal, 2),
        [BlockReactionObjectFrame.HorizontalBreakup3] = ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Horizontal, 3),
        [BlockReactionObjectFrame.VerticalBreakup0] = ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Vertical, 0),
        [BlockReactionObjectFrame.VerticalBreakup1] = ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Vertical, 1),
        [BlockReactionObjectFrame.VerticalBreakup2] = ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Vertical, 2),
        [BlockReactionObjectFrame.VerticalBreakup3] = ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Vertical, 3),
        [BlockReactionObjectFrame.SquareBreakup0] = ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Square, 0),
        [BlockReactionObjectFrame.SquareBreakup1] = ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Square, 1),
        [BlockReactionObjectFrame.SquareBreakup2] = ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Square, 2),
        [BlockReactionObjectFrame.SquareBreakup3] = ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Square, 3),
        [BlockReactionObjectFrame.ShotRestoreHorizontal] = ShotBlockObjectDraws.Restoration(RoomObjectShotBlockDrawDefinitions.Shape.Horizontal),
        [BlockReactionObjectFrame.ShotRestoreVertical] = ShotBlockObjectDraws.Restoration(RoomObjectShotBlockDrawDefinitions.Shape.Vertical),
        [BlockReactionObjectFrame.ShotRestoreSquare] = ShotBlockObjectDraws.Restoration(RoomObjectShotBlockDrawDefinitions.Shape.Square),
        [BlockReactionObjectFrame.BombRestoreHorizontal] = RoomObjectDraw.Of(BombBlockRestoreDraw.Horizontal),
        [BlockReactionObjectFrame.BombRestoreVertical] = RoomObjectDraw.Of(BombBlockRestoreDraw.Vertical),
        [BlockReactionObjectFrame.BombRestoreSquare] = RoomObjectDraw.Of(BombBlockRestoreDraw.Square),
        [BlockReactionObjectFrame.CrumbleRevealSingle] = RoomObjectDraw.Of(RoomObjectBombedRevealDraw.CrumbleSingle),
        [BlockReactionObjectFrame.CrumbleRevealHorizontal] = RoomObjectDraw.Of(ContactCrumbleRestoreDraw.Horizontal),
        [BlockReactionObjectFrame.CrumbleRevealVertical] = RoomObjectDraw.Of(ContactCrumbleRestoreDraw.Vertical),
        [BlockReactionObjectFrame.CrumbleRevealSquare] = RoomObjectDraw.Of(ContactCrumbleRestoreDraw.Square),
        [BlockReactionObjectFrame.PowerBombReveal] = RoomObjectDraw.Of(RoomObjectBombedRevealDraw.PowerBomb),
        [BlockReactionObjectFrame.SuperMissileReveal] = RoomObjectDraw.Of(RoomObjectBombedRevealDraw.SuperMissile),
        [BlockReactionObjectFrame.GrappleBlock] = RoomObjectDraw.Of(GrappleBlockDraw.Intact),
        [BlockReactionObjectFrame.GrappleBreak0] = RoomObjectDraw.Of(GrappleBlockDraw.Break0),
        [BlockReactionObjectFrame.GrappleBreak1] = RoomObjectDraw.Of(GrappleBlockDraw.Break1),
        [BlockReactionObjectFrame.GrappleBreak2] = RoomObjectDraw.Of(GrappleBlockDraw.Break2),
        [BlockReactionObjectFrame.GrappleBlank] = RoomObjectDraw.Of(GrappleBlockDraw.Blank),
    };
}
