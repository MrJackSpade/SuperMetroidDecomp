namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>Development-tool members of <see cref="DraygonFrameDefinitions"/>; never linked by player hosts.</summary>
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
    /// Every frame in the cartridge's root order: each facing's 27 one-part poses (arms, body, eye), eight two-part tail
    /// poses, then the tail whip's poses in pairs of four, six and eight parts.
    /// </summary>
    internal static readonly DraygonFrame[] CartridgeOrder =
    [
        DraygonFrame.ArmsFacingLeftIdleFirst,
        DraygonFrame.ArmsFacingLeftIdleSecond,
        DraygonFrame.ArmsFacingLeftIdleThird,
        DraygonFrame.ArmsFacingLeftIdleFourth,
        DraygonFrame.ArmsFacingLeftIdleFifth,
        DraygonFrame.ArmsFacingLeftIdleSixth,
        DraygonFrame.ArmsFacingLeftDyingFirst,
        DraygonFrame.ArmsFacingLeftDyingSecond,
        DraygonFrame.ArmsFacingLeftDyingThird,
        DraygonFrame.ArmsFacingLeftDyingFourth,
        DraygonFrame.BodyFacingLeftFireGoopFirst,
        DraygonFrame.BodyFacingLeftFireGoopSecond,
        DraygonFrame.BodyFacingLeftFireGoopThird,
        DraygonFrame.BodyFacingLeftFireGoopFourth,
        DraygonFrame.EyeFacingLeftIdleFirst,
        DraygonFrame.EyeFacingLeftIdleSecond,
        DraygonFrame.EyeFacingLeftIdleThird,
        DraygonFrame.EyeFacingLeftDyingLoopFifth,
        DraygonFrame.EyeFacingLeftIdleFourth,
        DraygonFrame.EyeFacingLeftDyingLoopThird,
        DraygonFrame.EyeFacingLeftIdleFifth,
        DraygonFrame.EyeFacingLeftIdleSixth,
        DraygonFrame.BodyFacingLeftReset,
        DraygonFrame.ArmsFacingLeftNearSwoopApexFirst,
        DraygonFrame.ArmsFacingLeftNearSwoopApexSecond,
        DraygonFrame.ArmsFacingLeftNearSwoopApexThird,
        DraygonFrame.ArmsFacingLeftNearSwoopApexFourth,
        DraygonFrame.TailFacingLeftIdleFirst,
        DraygonFrame.TailFacingLeftIdleSecond,
        DraygonFrame.TailFacingLeftIdleThird,
        DraygonFrame.TailFacingLeftIdleFourth,
        DraygonFrame.TailFacingLeftIdleFifth,
        DraygonFrame.TailFacingLeftIdleSixth,
        DraygonFrame.TailFacingLeftIdleSeventh,
        DraygonFrame.TailFacingLeftInitialFakeWhipSecond,
        DraygonFrame.TailFacingLeftInitialFakeWhipThird,
        DraygonFrame.TailFacingLeftInitialFakeWhipFourth,
        DraygonFrame.TailFacingLeftInitialFakeWhipFifth,
        DraygonFrame.TailFacingLeftInitialFakeWhipSixth,
        DraygonFrame.TailFacingLeftInitialFakeWhipSeventh,
        DraygonFrame.TailFacingLeftInitialFakeWhipEighth,
        DraygonFrame.ArmsFacingRightIdleFirst,
        DraygonFrame.ArmsFacingRightIdleSecond,
        DraygonFrame.ArmsFacingRightIdleThird,
        DraygonFrame.ArmsFacingRightIdleFourth,
        DraygonFrame.ArmsFacingRightIdleFifth,
        DraygonFrame.ArmsFacingRightIdleSixth,
        DraygonFrame.ArmsFacingRightDyingFirst,
        DraygonFrame.ArmsFacingRightDyingSecond,
        DraygonFrame.ArmsFacingRightDyingThird,
        DraygonFrame.ArmsFacingRightDyingFourth,
        DraygonFrame.BodyFacingRightFireGoopFirst,
        DraygonFrame.BodyFacingRightFireGoopSecond,
        DraygonFrame.BodyFacingRightFireGoopThird,
        DraygonFrame.BodyFacingRightFireGoopFourth,
        DraygonFrame.EyeFacingRightIdleFirst,
        DraygonFrame.EyeFacingRightIdleSecond,
        DraygonFrame.EyeFacingRightIdleThird,
        DraygonFrame.EyeFacingRightDyingLoopFifth,
        DraygonFrame.EyeFacingRightIdleFourth,
        DraygonFrame.EyeFacingRightDyingLoopThird,
        DraygonFrame.EyeFacingRightIdleFifth,
        DraygonFrame.EyeFacingRightIdleSixth,
        DraygonFrame.BodyFacingRightReset,
        DraygonFrame.ArmsFacingRightNearSwoopApexFirst,
        DraygonFrame.ArmsFacingRightNearSwoopApexSecond,
        DraygonFrame.ArmsFacingRightNearSwoopApexThird,
        DraygonFrame.ArmsFacingRightNearSwoopApexFourth,
        DraygonFrame.TailFacingRightIdleFirst,
        DraygonFrame.TailFacingRightIdleSecond,
        DraygonFrame.TailFacingRightIdleThird,
        DraygonFrame.TailFacingRightIdleFourth,
        DraygonFrame.TailFacingRightIdleFifth,
        DraygonFrame.TailFacingRightIdleSixth,
        DraygonFrame.TailFacingRightIdleSeventh,
        DraygonFrame.TailFacingRightFinalWhipsLoopSecond,
        DraygonFrame.TailFacingRightFinalWhipsLoopThird,
        DraygonFrame.TailFacingRightFinalWhipsLoopFourth,
        DraygonFrame.TailFacingRightFinalWhipsLoopFifth,
        DraygonFrame.TailFacingRightFinalWhipsLoopSixth,
        DraygonFrame.TailFacingRightFinalWhipsLoopSeventh,
        DraygonFrame.TailFacingRightFinalWhipsLoopEighth,
    ];
    /// <summary>How many frames the programs show.</summary>
    internal static int FrameCount => CartridgeOrder.Length;
    /// <summary>The facing-left roots, from $A5:A2DF.</summary>
    internal static readonly FacingRoots FacingLeft = new(0xa2df, 0xa40b, 0xa4a3, 0xa4ef, 0xa55b);
    /// <summary>The facing-right roots, from $A5:A607.</summary>
    internal static readonly FacingRoots FacingRight = new(0xa607, 0xa779, 0xa811, 0xa85d, 0xa8c9);
    /// <summary>The installed extended spritemap of <paramref name="frame"/>.</summary>
    internal static ushort Spritemap(DraygonFrame frame)
    {
        int position = Array.IndexOf(CartridgeOrder, frame);
        if (position < 0)
            throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Draygon frame.");
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
