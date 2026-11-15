namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// How Crocomire's melt sags his body (#1468, #1802, #2031): from <see cref="FirstLine"/> up to <see cref="EndLine"/> each
/// screen line falls a further <see cref="Slope"/> of a row behind his body placed on room row <see cref="Top"/>; the lines
/// below show it there, and the lines above show it placed on <see cref="LevelledTop"/>, where he lay when the melt was
/// last levelled. The display draws it as a stretch of his body
/// against the frame's camera; the boss keeps no table of lines.
/// </summary>
/// <param name="LevelledTop">The room row he lay on when the melt was last levelled, which the lines above the band show.</param>
/// <param name="Top">The room row he lies on, which the band stretches from and the lines below it show.</param>
/// <param name="FirstLine">The band's first screen line.</param>
/// <param name="EndLine">The screen line after the band's last.</param>
/// <param name="Slope">The rows the band falls behind per line, in 1/65536ths of a row.</param>
public sealed record CrocomireMeltDistortion(int LevelledTop, int Top, int FirstLine, int EndLine, int Slope)
{
    /// <summary>The melt levelled with him on room row <paramref name="top"/>: every line shows his body there, and no band stretches.</summary>
    /// <param name="top">The room row he lies on.</param>
    public static CrocomireMeltDistortion Level(int top) => new(top, top, 0, 0, 0);
}
