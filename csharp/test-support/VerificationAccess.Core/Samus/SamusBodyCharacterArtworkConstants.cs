using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Cartridge values and helpers of <see cref="SamusBodyCharacterArtwork"/> that only verification reads.</summary>
internal static class SamusBodyCharacterArtworkConstants
{
    /// <summary>SHA-256 of <paramref name="artwork"/>'s numbered half pictures' transfer sizes and characters, in record order (#1636: import data).</summary>
    public static string CharactersContentIdentityOf(SamusBodyCharacterArtwork artwork)
    {
        Ensure.NotNull(artwork);
        return ContentIdentityOf(artwork.Body.TopSetStarts.ToArray(), artwork.Body.BottomSetStarts.ToArray(), artwork.top, artwork.bottom);
    }

    /// <summary>SHA-256 of each numbered half picture's transfer sizes and characters, in record order.</summary>
    /// <param name="topStarts">Each upper-body definition group's first record.</param>
    /// <param name="bottomStarts">Each lower-body definition group's first record.</param>
    /// <param name="top">Upper-body tile definitions grouped by pointer table entry.</param>
    /// <param name="bottom">Lower-body tile definitions grouped by pointer table entry.</param>
    public static string ContentIdentityOf(int[] topStarts, int[] bottomStarts, SamusBodyTileTransfer[][] top, SamusBodyTileTransfer[][] bottom)
    {
        Ensure.NotNull(topStarts);
        Ensure.NotNull(bottomStarts);
        Ensure.NotNull(top);
        Ensure.NotNull(bottom);
        var records = new SortedDictionary<int, SamusBodyTileTransfer>();
        Add(topStarts, top);
        Add(bottomStarts, bottom);
        return SelectedPresentationHash.Create("samus-body-characters-v1", content =>
        {
            foreach ((int record, SamusBodyTileTransfer definition) in records)
            {
                content.Append("definition record", record);
                content.Append("first transfer size", definition.FirstSize);
                content.Append("second transfer size", definition.SecondSize);
                content.Append("characters", definition.Planar.Span);
            }
        });

        void Add(int[] starts, SamusBodyTileTransfer[][] groups)
        {
            if (starts.Length != groups.Length)
                throw new InvalidDataException("Samus body selector tables have an invalid length.");
            for (int set = 0; set < groups.Length; set++)
            {
                SamusBodyTileTransfer[] group = groups[set] ?? throw new InvalidDataException($"Samus body definition set {set} is missing.");
                for (int position = 0; position < group.Length; position++)
                    if (!records.TryAdd(starts[set] + position, group[position]))
                        throw new InvalidDataException($"Samus body definition record {starts[set] + position} is duplicated.");
            }
        }
    }
}
