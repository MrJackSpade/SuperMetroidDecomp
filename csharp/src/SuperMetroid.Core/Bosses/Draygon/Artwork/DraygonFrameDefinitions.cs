
namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// Draygon's presentation boundary (#641): the bank-$A5 extended spritemaps its typed programs show, in ascending
/// address order. Gameplay animates <see cref="DraygonFrame"/> indexes; only this catalog knows the spritemaps.
/// Each facing authors 27 one-component roots (arms, body, eye), eight two-component tail roots, then the tail
/// whip's roots in pairs of four, six and eight components.
/// </summary>
internal static class DraygonFrameDefinitions
{
    /// <summary>Frames per facing: 27 one-component roots, 8 tail roots and 6 whip roots.</summary>
    private const int FramesPerFacing = 41;

    /// <summary>How many frames the programs show.</summary>
    internal const int FrameCount = 2 * FramesPerFacing;

    /// <summary>The roots of one facing.</summary>
    /// <param name="OneComponent">The first of the 27 ten-byte one-component roots.</param>
    /// <param name="Tail">The first of the 8 eighteen-byte two-component tail roots.</param>
    /// <param name="WhipFour">The first of the 2 four-component whip roots.</param>
    /// <param name="WhipSix">The first of the 2 six-component whip roots.</param>
    /// <param name="WhipEight">The first of the 2 eight-component whip roots.</param>
    private readonly record struct FacingRoots(ushort OneComponent, ushort Tail, ushort WhipFour, ushort WhipSix, ushort WhipEight);

    /// <summary>The facing-left roots, from $A5:A2DF.</summary>
    private static readonly FacingRoots FacingLeft = new(0xa2df, 0xa40b, 0xa4a3, 0xa4ef, 0xa55b);

    /// <summary>The facing-right roots, from $A5:A607.</summary>
    private static readonly FacingRoots FacingRight = new(0xa607, 0xa779, 0xa811, 0xa85d, 0xa8c9);

    /// <summary>The installed extended spritemap of <paramref name="frame"/>.</summary>
    internal static ushort Spritemap(DraygonFrame frame)
    {
        if ((uint)frame.Index >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Draygon frame.");
        FacingRoots roots = frame.Index < FramesPerFacing ? FacingLeft : FacingRight;
        int index = frame.Index % FramesPerFacing;
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
    private static int Root(int components) => sizeof(ushort) + 8 * components;
}
