using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Installed colour rows kept by the identity each colours (#2565): a health band's or a frame's colours are found by the
/// band or frame, never by a row number standing for it.
/// </summary>
/// <typeparam name="T">The identity each row colours.</typeparam>
internal sealed class InstalledKeyedColourRows<T>
    where T : notnull
{
    /// <summary>Each identity's installed row.</summary>
    private readonly Dictionary<T, InstalledColourRows> rows;

    /// <summary>Keeps copies of every identity's complete installed row.</summary>
    /// <param name="rows">Each identity's installed colours.</param>
    /// <param name="identities">The identities the table must hold a row for, and only those.</param>
    /// <param name="coloursPerRow">The colours each row must hold.</param>
    /// <param name="label">What the table is, naming it in a rejection.</param>
    /// <exception cref="InvalidDataException">An identity has no row, an extra row is held, or a row has another number of colours.</exception>
    internal InstalledKeyedColourRows(IReadOnlyDictionary<T, RgbColor[]> rows, IReadOnlyCollection<T> identities, int coloursPerRow, string label)
    {
        Ensure.NotNull(rows);
        Ensure.NotNull(identities);
        if (rows.Count != identities.Count)
            throw new InvalidDataException($"{label} requires one row for each of its {identities.Count} identities.");
        this.rows = identities.ToDictionary(identity => identity, identity => new InstalledColourRows(
            rows.TryGetValue(identity, out RgbColor[]? row) ? row : throw new InvalidDataException($"{label} holds no row for {identity}."),
            coloursPerRow, $"{label} {identity}"));
    }

    /// <summary>The installed colour <paramref name="colour"/> of <paramref name="identity"/>'s row.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The table holds no row for the identity, or the colour is outside the row.</exception>
    internal RgbColor Colour(T identity, int colour) => rows.TryGetValue(identity, out InstalledColourRows? row)
        ? row.Colour(colour)
        : throw new ArgumentOutOfRangeException(nameof(identity), identity, "The installed rows hold no such identity.");
}
