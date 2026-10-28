using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>
/// The nine $A7:B3D3/$B513 bands comprise a white flash and eight RGB5 health steps.
/// The flash, both endpoint bands and the two authored color-six steps come from
/// <see cref="KraidPaint"/> unless supplied differently; interior bands
/// interpolate the current endpoints, and their transparent slot is the final band's.
/// Only supplied deviations are stored, so edited endpoints still drive every step.
/// </summary>
internal sealed class KraidHealthBands : IReadOnlyList<RgbColor>
{
    /// <summary>Edited authored endpoints or interior samples that differ from calculated interpolation.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> deviations = new();

    /// <summary>Records endpoint edits before comparing interior colors against those selected endpoints.</summary>
    internal KraidHealthBands(RgbColor[] supplied)
    {
        // Authored cells first: interior interpolation reads the (possibly edited) endpoints.
        for (int index = 0; index < supplied.Length; index++)
            if (KraidPaint.HealthAuthored(index) is RgbColor stock && stock != supplied[index])
                deviations.Add(new ColourCell(0, index), supplied[index]);
        for (int index = 0; index < supplied.Length; index++)
            if (KraidPaint.HealthAuthored(index) is null && Calculate(index) != supplied[index])
                deviations.Add(new ColourCell(0, index), supplied[index]);
    }

    /// <summary>The secondary bands: the health bands themselves when the supplied colors match them exactly.</summary>
    internal static IReadOnlyList<RgbColor> Secondary(RgbColor[] supplied, KraidHealthBands health) =>
        supplied.SequenceEqual(health) ? health : new KraidHealthBands(supplied);

    /// <summary>
    /// Interpolates one channelwise health color for <paramref name="band"/> between the first
    /// normal band and the final band, to nearest integer over the seven intervals.
    /// </summary>
    internal static RgbColor Interpolate(RgbColor first, RgbColor last, int band)
    {
        int intervals = KraidPalettes.HealthBandCount - 2;
        return first.Zip(last, (_, start, end) =>
        {
            int delta = end - start;
            return start + Math.Sign(delta) * ((Math.Abs(delta) * (band - 1) + intervals / 2) / intervals);
        });
    }

    /// <summary>White flash plus eight native sixteen-color health bands.</summary>
    public int Count => KraidPalettes.HealthBandCount * KraidPalettes.BandColors;
    /// <summary>Selected health color, preserving independent edits over calculated bands.</summary>
    public RgbColor this[int index]
    {
        get
        {
            if ((uint)index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return deviations.TryGetValue(new ColourCell(0, index), out RgbColor supplied) ? supplied : Calculate(index);
        }
    }

    /// <summary>Uses authored flash/endpoints or interpolates an interior band from the current selected endpoints.</summary>
    private RgbColor Calculate(int index)
    {
        if (KraidPaint.HealthAuthored(index) is RgbColor authored)
            return authored;
        int band = index / KraidPalettes.BandColors;
        int color = index % KraidPalettes.BandColors;
        int finalBand = (KraidPalettes.HealthBandCount - 1) * KraidPalettes.BandColors;
        if (color == 0)
            return this[finalBand];
        return Interpolate(this[KraidPalettes.BandColors + color], this[finalBand + color], band);
    }

    /// <summary>Visits flash and health bands in their native contiguous order.</summary>
    public IEnumerator<RgbColor> GetEnumerator()
    {
        for (int index = 0; index < Count; index++)
            yield return this[index];
    }
    /// <summary>Exposes the same health-band order to non-generic enumeration.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
