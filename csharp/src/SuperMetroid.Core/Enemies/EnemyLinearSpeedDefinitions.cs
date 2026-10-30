namespace SuperMetroid.Core.Enemies;

/// <summary>The size of the shared linear enemy speed table.</summary>
/// <remarks>
/// Row <c>n</c> moves an enemy <c>n</c> sixteenths of a pixel per frame, in either direction. Reviewed for #1165
/// against NTSC J/U v1.0; PAL authors a different table.
/// </remarks>
public static class EnemyLinearSpeedDefinitions
{
    /// <summary>The number of rows, zero through 64: from standing still to four pixels per frame.</summary>
    public const int RowCount = 65;

    /// <summary>The row that moves one pixel per frame.</summary>
    internal const int OnePixelPerFrameRow = 16;
}
