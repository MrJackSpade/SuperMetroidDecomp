namespace SuperMetroid.Core.Assets;

/// <summary>
/// Which sprite artwork shows over which of the gameplay sprite characters (#1392, #1406, #1422), as an ordered record of
/// placements: a room's load, its setup and its encounters place their artwork over runs of characters, and a later
/// placement shows over an earlier one where they overlap. Enemy frames select their pre-coloured pictures from it, and
/// the Ceres Ridley arena's backgrounds read it. The record names artwork only; saved state carries the identities.
/// </summary>
public sealed class SpriteArtworkPlacements
{
    /// <summary>Bytes one sprite character of installed artwork holds, the unit the artwork definitions count in.</summary>
    public const int CharacterByteCount = 32;

    /// <summary>The gameplay sprite characters artwork can be placed over.</summary>
    public const int CharacterCount = 512;

    /// <summary>The placements in order; a later one shows over an earlier one where they overlap.</summary>
    internal readonly List<SpriteArtworkPlacement> placements = [];

    /// <summary>Each placement's place among all placements ever made, alongside <see cref="placements"/>.</summary>
    private readonly List<long> _stamps = [];

    /// <summary>The place of the latest placement among all ever made; zero before the first.</summary>
    public long Recorded { get; private set; }

    /// <summary>Shows <paramref name="placement"/>'s artwork over its characters, over whatever showed there before.</summary>
    public void Show(SpriteArtworkPlacement placement)
    {
        Ensure.NotNull(placement);
        // A placement wholly covered by a later one no longer shows anywhere; dropping it keeps the record bounded.
        for (int index = placements.Count - 1; index >= 0; index--)
        {
            if (placement.Covers(placements[index].FirstCharacter, placements[index].CharacterCount))
            {
                placements.RemoveAt(index);
                _stamps.RemoveAt(index);
            }
        }
        Recorded++;
        placements.Add(placement);
        _stamps.Add(Recorded);
    }

    /// <summary>The placement shown last over any of <paramref name="characterCount"/> characters from <paramref name="firstCharacter"/>, if any has been.</summary>
    public SpriteArtworkPlacement? LastOver(int firstCharacter, int characterCount)
    {
        for (int index = placements.Count - 1; index >= 0; index--)
        {
            if (placements[index].Overlaps(firstCharacter, characterCount))
                return placements[index];
        }
        return null;
    }

    /// <summary>The artwork shown, newest placement first.</summary>
    public IEnumerable<SpriteArtworkSource> NewestFirst
    {
        get
        {
            for (int index = placements.Count - 1; index >= 0; index--)
                yield return placements[index].Source;
        }
    }

    /// <summary>The artwork placed after the placement whose place is <paramref name="recorded"/>, newest first.</summary>
    /// <param name="recorded">A value <see cref="Recorded"/> held.</param>
    public IEnumerable<SpriteArtworkSource> ShownAfter(long recorded)
    {
        for (int index = placements.Count - 1; index >= 0 && _stamps[index] > recorded; index--)
            yield return placements[index].Source;
    }
}
