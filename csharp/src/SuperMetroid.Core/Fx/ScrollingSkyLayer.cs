using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The scrolling sky as a frame shows it (#1347, #1687): the room row at the camera and the sky's bands from the top
/// down, each with the rows it covers and how far it has drifted. The display describes the screen's bands from it
/// (#1468). Rows outside the bands, and every row of a frozen frame, show unshifted.
/// </summary>
/// <param name="CameraRow">The room row at the camera; a camera above the room's top is at a negative row.</param>
/// <param name="Bands">The sky's bands from the top of the room down, each beginning at or below the end of the one before.</param>
/// <param name="ShowsDrift">False on a frozen frame, which shows every band unshifted.</param>
public sealed record ScrollingSkyLayer(int CameraRow, ImmutableArray<ShownSkyBand> Bands, bool ShowsDrift)
{
    /// <summary>The sky's bands from the top of the room down.</summary>
    /// <exception cref="ArgumentException">A band begins above the end of the one before it.</exception>
    public ImmutableArray<ShownSkyBand> Bands
    {
        get;
        init => field = RequireTopDown(value);
    } = RequireTopDown(Bands);

    /// <summary>The band covering room row <paramref name="row"/>, or null outside the sky's bands.</summary>
    /// <param name="row">A room row; rows above the room are negative.</param>
    public ShownSkyBand? BandAt(int row)
    {
        foreach (ShownSkyBand band in Bands)
            if (band.Covers(row))
                return band;
        return null;
    }

    /// <summary>The first row of the first band below room row <paramref name="row"/>, or null when no band lies below it.</summary>
    /// <param name="row">A room row outside the sky's bands.</param>
    public int? NextBandTop(int row)
    {
        foreach (ShownSkyBand band in Bands)
            if (band.FirstRow > row)
                return band.FirstRow;
        return null;
    }

    /// <summary>How far room row <paramref name="row"/> shows drifted: its band's offset, or none outside the bands or on a frozen frame.</summary>
    /// <param name="row">A room row; rows above the room are negative.</param>
    public int OffsetAt(int row) => ShowsDrift && BandAt(row) is { } band ? band.Offset : 0;

    /// <summary>Requires bands each beginning at or below the end of the one before.</summary>
    /// <exception cref="ArgumentException">The bands overlap or are out of order.</exception>
    private static ImmutableArray<ShownSkyBand> RequireTopDown(ImmutableArray<ShownSkyBand> bands)
    {
        if (bands.IsDefault) return [];
        for (int index = 1; index < bands.Length; index++)
            if (bands[index].FirstRow < bands[index - 1].EndRow)
                throw new ArgumentException("Each sky band begins at or below the end of the one before it.", nameof(bands));
        return bands;
    }
}
