namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// Walks a horizontal strip of cinematic part cells through their sixteen-column character atlas: crossing sixteen
/// columns skips the atlas row occupied by the bottom halves of large parts.
/// </summary>
internal static class CinematicAtlasStrip
{
    /// <summary>At a sixteen-column boundary, skip the row of lower tile halves.</summary>
    internal static int TwoRowStrip(int start, int column)
    {
        int atlasColumn = (start & 15) + column;
        return (start & ~15) + 32 * (atlasColumn / 16) + atlasColumn % 16;
    }
}
