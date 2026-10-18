using SuperMetroid.Core.Enemies.Artwork;


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

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(DraygonFrame frame) => frame switch
    {
        { Index: 0 } => EnemyFrameId.DraygonOamA2DF,
        { Index: 1 } => EnemyFrameId.DraygonOamA2E9,
        { Index: 10 } => EnemyFrameId.DraygonBg2A343,
        { Index: 11 } => EnemyFrameId.DraygonBg2A34D,
        { Index: 12 } => EnemyFrameId.DraygonBg2A357,
        { Index: 13 } => EnemyFrameId.DraygonBg2A361,
        { Index: 14 } => EnemyFrameId.DraygonBg2A36B,
        { Index: 15 } => EnemyFrameId.DraygonBg2A375,
        { Index: 16 } => EnemyFrameId.DraygonBg2A37F,
        { Index: 17 } => EnemyFrameId.DraygonBg2A389,
        { Index: 18 } => EnemyFrameId.DraygonBg2A393,
        { Index: 19 } => EnemyFrameId.DraygonBg2A39D,
        { Index: 2 } => EnemyFrameId.DraygonOamA2F3,
        { Index: 20 } => EnemyFrameId.DraygonBg2A3A7,
        { Index: 21 } => EnemyFrameId.DraygonBg2A3B1,
        { Index: 22 } => EnemyFrameId.DraygonBg2A3BB,
        { Index: 23 } => EnemyFrameId.DraygonOamA3C5,
        { Index: 24 } => EnemyFrameId.DraygonOamA3CF,
        { Index: 25 } => EnemyFrameId.DraygonOamA3D9,
        { Index: 26 } => EnemyFrameId.DraygonOamA3E3,
        { Index: 27 } => EnemyFrameId.DraygonOamA40B,
        { Index: 28 } => EnemyFrameId.DraygonOamA41D,
        { Index: 29 } => EnemyFrameId.DraygonOamA42F,
        { Index: 3 } => EnemyFrameId.DraygonOamA2FD,
        { Index: 30 } => EnemyFrameId.DraygonOamA441,
        { Index: 31 } => EnemyFrameId.DraygonOamA453,
        { Index: 32 } => EnemyFrameId.DraygonOamA465,
        { Index: 33 } => EnemyFrameId.DraygonOamA477,
        { Index: 34 } => EnemyFrameId.DraygonOamA489,
        { Index: 35 } => EnemyFrameId.DraygonOamA4A3,
        { Index: 36 } => EnemyFrameId.DraygonOamA4C5,
        { Index: 37 } => EnemyFrameId.DraygonOamA4EF,
        { Index: 38 } => EnemyFrameId.DraygonOamA521,
        { Index: 39 } => EnemyFrameId.DraygonOamA55B,
        { Index: 4 } => EnemyFrameId.DraygonOamA307,
        { Index: 40 } => EnemyFrameId.DraygonOamA59D,
        { Index: 41 } => EnemyFrameId.DraygonOamA607,
        { Index: 42 } => EnemyFrameId.DraygonOamA611,
        { Index: 43 } => EnemyFrameId.DraygonOamA61B,
        { Index: 44 } => EnemyFrameId.DraygonOamA625,
        { Index: 45 } => EnemyFrameId.DraygonOamA62F,
        { Index: 46 } => EnemyFrameId.DraygonOamA639,
        { Index: 47 } => EnemyFrameId.DraygonBg2A643,
        { Index: 48 } => EnemyFrameId.DraygonBg2A64D,
        { Index: 49 } => EnemyFrameId.DraygonBg2A657,
        { Index: 5 } => EnemyFrameId.DraygonOamA311,
        { Index: 50 } => EnemyFrameId.DraygonBg2A661,
        { Index: 51 } => EnemyFrameId.DraygonBg2A66B,
        { Index: 52 } => EnemyFrameId.DraygonBg2A675,
        { Index: 53 } => EnemyFrameId.DraygonBg2A67F,
        { Index: 54 } => EnemyFrameId.DraygonBg2A689,
        { Index: 55 } => EnemyFrameId.DraygonBg2A693,
        { Index: 56 } => EnemyFrameId.DraygonBg2A69D,
        { Index: 57 } => EnemyFrameId.DraygonBg2A6A7,
        { Index: 58 } => EnemyFrameId.DraygonBg2A6B1,
        { Index: 59 } => EnemyFrameId.DraygonBg2A6BB,
        { Index: 6 } => EnemyFrameId.DraygonBg2A31B,
        { Index: 60 } => EnemyFrameId.DraygonBg2A6C5,
        { Index: 61 } => EnemyFrameId.DraygonBg2A6CF,
        { Index: 62 } => EnemyFrameId.DraygonBg2A6D9,
        { Index: 63 } => EnemyFrameId.DraygonBg2A6E3,
        { Index: 64 } => EnemyFrameId.DraygonOamA6ED,
        { Index: 65 } => EnemyFrameId.DraygonOamA6F7,
        { Index: 66 } => EnemyFrameId.DraygonOamA701,
        { Index: 67 } => EnemyFrameId.DraygonOamA70B,
        { Index: 68 } => EnemyFrameId.DraygonOamA779,
        { Index: 69 } => EnemyFrameId.DraygonOamA78B,
        { Index: 7 } => EnemyFrameId.DraygonBg2A325,
        { Index: 70 } => EnemyFrameId.DraygonOamA79D,
        { Index: 71 } => EnemyFrameId.DraygonOamA7AF,
        { Index: 72 } => EnemyFrameId.DraygonOamA7C1,
        { Index: 73 } => EnemyFrameId.DraygonOamA7D3,
        { Index: 74 } => EnemyFrameId.DraygonOamA7E5,
        { Index: 75 } => EnemyFrameId.DraygonOamA7F7,
        { Index: 76 } => EnemyFrameId.DraygonOamA811,
        { Index: 77 } => EnemyFrameId.DraygonOamA833,
        { Index: 78 } => EnemyFrameId.DraygonOamA85D,
        { Index: 79 } => EnemyFrameId.DraygonOamA88F,
        { Index: 8 } => EnemyFrameId.DraygonBg2A32F,
        { Index: 80 } => EnemyFrameId.DraygonOamA8C9,
        { Index: 81 } => EnemyFrameId.DraygonOamA90B,
        { Index: 9 } => EnemyFrameId.DraygonBg2A339,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the DraygonFrame programs show."),
    };
}
