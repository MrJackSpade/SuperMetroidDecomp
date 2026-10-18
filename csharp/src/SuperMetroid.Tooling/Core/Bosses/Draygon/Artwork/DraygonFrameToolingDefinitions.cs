using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>Development-tool members of <see cref="DraygonFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class DraygonFrameToolingDefinitions
{
    /// <summary>Frames per facing: 27 one-component roots, 8 tail roots and 6 whip roots.</summary>
    internal const int FramesPerFacing = 41;
    /// <summary>How many frames the programs show.</summary>
    internal const int FrameCount = 2 * FramesPerFacing;
    /// <summary>The facing-left roots, from $A5:A2DF.</summary>
    internal static readonly DraygonFrameDefinitions.FacingRoots FacingLeft = new(0xa2df, 0xa40b, 0xa4a3, 0xa4ef, 0xa55b);
    /// <summary>The facing-right roots, from $A5:A607.</summary>
    internal static readonly DraygonFrameDefinitions.FacingRoots FacingRight = new(0xa607, 0xa779, 0xa811, 0xa85d, 0xa8c9);
    /// <summary>The installed extended spritemap of <paramref name="frame"/>.</summary>
    internal static ushort Spritemap(DraygonFrame frame)
    {
        if ((uint)frame.Index >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Draygon frame.");
        DraygonFrameDefinitions.FacingRoots roots = frame.Index < FramesPerFacing ? FacingLeft : FacingRight;
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
    internal static int Root(int components) => sizeof(ushort) + 8 * components;
}
