namespace SuperMetroid.Core.Hardware;

/// <summary>
/// What shows over the gameplay sprite characters (#1392, #1406), as an ordered record of placements: which owned artwork
/// shows over which characters, in the order shown. Enemy frames select their pictures from it, and the Ceres Ridley
/// arena's backgrounds read it. Installed artwork is named, never copied, so saved state carries only the identities.
/// </summary>
public sealed class SpriteCharacters
{
    /// <summary>Bytes one four-bit sprite character holds.</summary>
    public const int CharacterByteCount = 32;

    /// <summary>The sprite characters: 512 four-bit characters.</summary>
    public const int CharacterCount = 512;

    /// <summary>The placements in order; a later one shows over an earlier one where they overlap.</summary>
    internal readonly List<SpriteCharacterPlacement> placements = [];

    /// <summary>Each placement's place among all placements ever shown, alongside <see cref="placements"/>.</summary>
    private readonly List<long> _stamps = [];

    /// <summary>The place of the latest placement among all ever shown; zero before the first.</summary>
    public long Recorded { get; private set; }

    /// <summary>Shows <paramref name="placement"/>'s artwork over its characters, over whatever showed there before.</summary>
    public void Show(SpriteCharacterPlacement placement)
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
    public SpriteCharacterPlacement? LastOver(int firstCharacter, int characterCount)
    {
        for (int index = placements.Count - 1; index >= 0; index--)
        {
            if (placements[index].Overlaps(firstCharacter, characterCount))
                return placements[index];
        }
        return null;
    }

    /// <summary>The sources shown, newest first, each with its place among all placements ever shown.</summary>
    public IEnumerable<(ObjCharacterSource Source, long Stamp)> NewestFirst
    {
        get
        {
            for (int index = placements.Count - 1; index >= 0; index--)
                yield return (placements[index].Source, _stamps[index]);
        }
    }
}
