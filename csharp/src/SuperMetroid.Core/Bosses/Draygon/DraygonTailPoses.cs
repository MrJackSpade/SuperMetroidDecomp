using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The poses of Draygon's tail lashing out and recoiling facing one way (#2465): its tip out, the lash from cocked to
/// curled and the recoil. His whips, his fake whip and his flail are written once over a facing's poses, as a ninja Space
/// Pirate's claw throw is written once over its facing's (#2451).
/// </summary>
/// <param name="Facing">The way Draygon faces.</param>
/// <param name="TipOut">The tail's tip swung out, before the lash.</param>
/// <param name="Cocking">The tail cocked to lash.</param>
/// <param name="Raising">The tail raised.</param>
/// <param name="HangingLong">The tail hanging long.</param>
/// <param name="Swinging">The tail swinging forward.</param>
/// <param name="LashedOut">The tail lashed out.</param>
/// <param name="Curling">The tail curling at the end of its lash.</param>
/// <param name="Recoiling">The tail recoiling from its lash.</param>
internal sealed record DraygonTailPoses(
    HorizontalDirection Facing, F TipOut, F Cocking, F Raising, F HangingLong, F Swinging, F LashedOut, F Curling, F Recoiling)
{
    /// <summary>The poses facing left.</summary>
    internal static DraygonTailPoses Left { get; } = new(
        HorizontalDirection.Left, F.DraygonTailFacingLeftTipOut, F.DraygonTailFacingLeftWhipCocking, F.DraygonTailFacingLeftWhipRaising,
        F.DraygonTailFacingLeftWhipHangingLong, F.DraygonTailFacingLeftWhipSwinging, F.DraygonTailFacingLeftWhipLashedOut,
        F.DraygonTailFacingLeftWhipCurling, F.DraygonTailFacingLeftWhipRecoiling);

    /// <summary>The poses facing right.</summary>
    internal static DraygonTailPoses Right { get; } = new(
        HorizontalDirection.Right, F.DraygonTailFacingRightTipOut, F.DraygonTailFacingRightWhipCocking, F.DraygonTailFacingRightWhipRaising,
        F.DraygonTailFacingRightWhipHangingLong, F.DraygonTailFacingRightWhipSwinging, F.DraygonTailFacingRightWhipLashedOut,
        F.DraygonTailFacingRightWhipCurling, F.DraygonTailFacingRightWhipRecoiling);
}
