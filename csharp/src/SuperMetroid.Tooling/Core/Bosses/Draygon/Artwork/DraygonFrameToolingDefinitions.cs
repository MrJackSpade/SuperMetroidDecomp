using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// Development-tool members locating the cartridge roots of the frames Draygon's animations show; never linked by player
/// hosts.
/// </summary>
internal static class DraygonFrameToolingDefinitions
{
    /// <summary>The roots of one facing.</summary>
    /// <param name="OneComponent">The first of the 27 ten-byte one-component roots.</param>
    /// <param name="Tail">The first of the 8 eighteen-byte two-component tail roots.</param>
    /// <param name="WhipFour">The first of the 2 four-component whip roots.</param>
    /// <param name="WhipSix">The first of the 2 six-component whip roots.</param>
    /// <param name="WhipEight">The first of the 2 eight-component whip roots.</param>
    internal readonly record struct FacingRoots(ushort OneComponent, ushort Tail, ushort WhipFour, ushort WhipSix, ushort WhipEight);

    /// <summary>Frames per facing: 27 one-component roots, 8 tail roots and 6 whip roots.</summary>
    internal const int FramesPerFacing = 41;
    /// <summary>
    /// The frame each root shows, in the cartridge's root order: each facing's 27 one-part poses (arms, body, eye), eight
    /// two-part tail poses, then the tail whip's poses in pairs of four, six and eight parts. The shut jaw shows from two
    /// roots (#2268): one writes only the jaw's cells, the other the whole body.
    /// </summary>
    internal static readonly EnemyFrameId[] CartridgeOrder =
    [
        EnemyFrameId.DraygonArmsFacingLeftForward,
        EnemyFrameId.DraygonArmsFacingLeftLowering,
        EnemyFrameId.DraygonArmsFacingLeftDown,
        EnemyFrameId.DraygonArmsFacingLeftDownStretched,
        EnemyFrameId.DraygonArmsFacingLeftRaising,
        EnemyFrameId.DraygonArmsFacingLeftReaching,
        EnemyFrameId.DraygonLimpArmFacingLeftClawRaised,
        EnemyFrameId.DraygonLimpArmFacingLeftClawDropping,
        EnemyFrameId.DraygonLimpArmFacingLeftClawDropped,
        EnemyFrameId.DraygonLimpArmFacingLeftClawCurled,
        EnemyFrameId.DraygonBodyFacingLeftJawShut,
        EnemyFrameId.DraygonBodyFacingLeftJawParting,
        EnemyFrameId.DraygonBodyFacingLeftJawOpen,
        EnemyFrameId.DraygonBodyFacingLeftJawWide,
        EnemyFrameId.DraygonEyeFacingLeftShut,
        EnemyFrameId.DraygonEyeFacingLeftOpening,
        EnemyFrameId.DraygonEyeFacingLeftHalfOpen,
        EnemyFrameId.DraygonEyeFacingLeftWide,
        EnemyFrameId.DraygonEyeFacingLeftOpen,
        EnemyFrameId.DraygonEyeFacingLeftBloodshot,
        EnemyFrameId.DraygonEyeFacingLeftNarrowing,
        EnemyFrameId.DraygonEyeFacingLeftClosing,
        EnemyFrameId.DraygonBodyFacingLeftJawShut,
        EnemyFrameId.DraygonArmsFacingLeftSpreadWide,
        EnemyFrameId.DraygonArmsFacingLeftClosing,
        EnemyFrameId.DraygonArmsFacingLeftGathering,
        EnemyFrameId.DraygonArmsFacingLeftTucked,
        EnemyFrameId.DraygonTailFacingLeftTipCurledIn,
        EnemyFrameId.DraygonTailFacingLeftTipSwingingOut,
        EnemyFrameId.DraygonTailFacingLeftTipOut,
        EnemyFrameId.DraygonTailFacingLeftTipFurtherOut,
        EnemyFrameId.DraygonTailFacingLeftTipFarthestOut,
        EnemyFrameId.DraygonTailFacingLeftTipReturning,
        EnemyFrameId.DraygonTailFacingLeftTipBack,
        EnemyFrameId.DraygonTailFacingLeftWhipCocking,
        EnemyFrameId.DraygonTailFacingLeftWhipRaising,
        EnemyFrameId.DraygonTailFacingLeftWhipHangingLong,
        EnemyFrameId.DraygonTailFacingLeftWhipSwinging,
        EnemyFrameId.DraygonTailFacingLeftWhipLashedOut,
        EnemyFrameId.DraygonTailFacingLeftWhipCurling,
        EnemyFrameId.DraygonTailFacingLeftWhipRecoiling,
        EnemyFrameId.DraygonArmsFacingRightForward,
        EnemyFrameId.DraygonArmsFacingRightLowering,
        EnemyFrameId.DraygonArmsFacingRightDown,
        EnemyFrameId.DraygonArmsFacingRightDownStretched,
        EnemyFrameId.DraygonArmsFacingRightRaising,
        EnemyFrameId.DraygonArmsFacingRightReaching,
        EnemyFrameId.DraygonLimpArmFacingRightClawRaised,
        EnemyFrameId.DraygonLimpArmFacingRightClawDropping,
        EnemyFrameId.DraygonLimpArmFacingRightClawDropped,
        EnemyFrameId.DraygonLimpArmFacingRightClawCurled,
        EnemyFrameId.DraygonBodyFacingRightJawShut,
        EnemyFrameId.DraygonBodyFacingRightJawParting,
        EnemyFrameId.DraygonBodyFacingRightJawOpen,
        EnemyFrameId.DraygonBodyFacingRightJawWide,
        EnemyFrameId.DraygonEyeFacingRightShut,
        EnemyFrameId.DraygonEyeFacingRightOpening,
        EnemyFrameId.DraygonEyeFacingRightHalfOpen,
        EnemyFrameId.DraygonEyeFacingRightWide,
        EnemyFrameId.DraygonEyeFacingRightOpen,
        EnemyFrameId.DraygonEyeFacingRightBloodshot,
        EnemyFrameId.DraygonEyeFacingRightNarrowing,
        EnemyFrameId.DraygonEyeFacingRightClosing,
        EnemyFrameId.DraygonBodyFacingRightJawShut,
        EnemyFrameId.DraygonArmsFacingRightSpreadWide,
        EnemyFrameId.DraygonArmsFacingRightClosing,
        EnemyFrameId.DraygonArmsFacingRightGathering,
        EnemyFrameId.DraygonArmsFacingRightTucked,
        EnemyFrameId.DraygonTailFacingRightTipCurledIn,
        EnemyFrameId.DraygonTailFacingRightTipSwingingOut,
        EnemyFrameId.DraygonTailFacingRightTipOut,
        EnemyFrameId.DraygonTailFacingRightTipFurtherOut,
        EnemyFrameId.DraygonTailFacingRightTipFarthestOut,
        EnemyFrameId.DraygonTailFacingRightTipReturning,
        EnemyFrameId.DraygonTailFacingRightTipBack,
        EnemyFrameId.DraygonTailFacingRightWhipCocking,
        EnemyFrameId.DraygonTailFacingRightWhipRaising,
        EnemyFrameId.DraygonTailFacingRightWhipHangingLong,
        EnemyFrameId.DraygonTailFacingRightWhipSwinging,
        EnemyFrameId.DraygonTailFacingRightWhipLashedOut,
        EnemyFrameId.DraygonTailFacingRightWhipCurling,
        EnemyFrameId.DraygonTailFacingRightWhipRecoiling,
    ];
    /// <summary>The facing-left roots, from $A5:A2DF.</summary>
    internal static readonly FacingRoots FacingLeft = new(0xa2df, 0xa40b, 0xa4a3, 0xa4ef, 0xa55b);
    /// <summary>The facing-right roots, from $A5:A607.</summary>
    internal static readonly FacingRoots FacingRight = new(0xa607, 0xa779, 0xa811, 0xa85d, 0xa8c9);
    /// <summary>
    /// The extended spritemap the import pictures <paramref name="frame"/> from: of the roots that show it, the last, which
    /// for the shut jaw is the whole body's.
    /// </summary>
    internal static ushort Spritemap(EnemyFrameId frame)
    {
        int position = Array.LastIndexOf(CartridgeOrder, frame);
        if (position < 0)
            throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a Draygon frame.");
        return RootAt(position);
    }

    /// <summary>The extended spritemap at <paramref name="position"/> of the cartridge's root order.</summary>
    internal static ushort RootAt(int position)
    {
        FacingRoots roots = position < FramesPerFacing ? FacingLeft : FacingRight;
        int index = position % FramesPerFacing;
        return unchecked((ushort)(index switch
        {
            < 27 => roots.OneComponent + Root(1) * index,
            < 35 => roots.Tail + Root(2) * (index - 27),
            < 37 => roots.WhipFour + Root(4) * (index - 35),
            < 39 => roots.WhipSix + Root(6) * (index - 37),
            _ => roots.WhipEight + Root(8) * (index - 39),
        }));
    }
    /// <summary>The bytes of a root with <paramref name="components"/> components: a count word and eight bytes each.</summary>
    internal static int Root(int components) => sizeof(ushort) + 8 * components;
}
