using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Which sprite look shows over which of the cartridge's OBJ characters (#1392, #1406, #1422, #1463), as an ordered record of
/// placements: a later placement shows over an earlier one where they overlap. The import pipeline replays gameplay's shown
/// looks (<see cref="ShownSpriteArtwork"/>) through it to know what the characters hold, which the Ceres Ridley arena's
/// backgrounds read and which the pictures of enemy frames are composed over.
/// </summary>
public sealed class SpriteArtworkPlacements
{
    /// <summary>Bytes one sprite character of installed artwork holds, the unit the artwork definitions count in.</summary>
    public const int CharacterByteCount = 32;

    /// <summary>The gameplay sprite characters artwork can be placed over.</summary>
    public const int CharacterCount = 512;

    /// <summary>The bytes every sprite character holds together.</summary>
    internal const int TableByteCount = CharacterCount * CharacterByteCount;

    /// <summary>The placements in order; a later one shows over an earlier one where they overlap.</summary>
    internal readonly List<SpriteArtworkPlacement> placements = [];

    /// <summary>
    /// The placements <paramref name="looks"/> leave: each look shown, in order, where the cartridge shows it. Replaying
    /// the looks in the order last shown leaves the placements showing every look in the order each was ever shown would,
    /// as a look shown again covers its own earlier showing.
    /// </summary>
    /// <exception cref="InvalidDataException">The cartridge shows one of the looks nowhere among the sprite characters.</exception>
    public static SpriteArtworkPlacements Of(ShownSpriteArtwork looks)
    {
        var placements = new SpriteArtworkPlacements();
        foreach (SpriteArtworkSource look in Ensure.NotNull(looks).InOrder)
            placements.Show(EncounterCharacterPlacements.Placed(look));
        return placements;
    }

    /// <summary>Shows <paramref name="placement"/>'s artwork over its characters, over whatever showed there before.</summary>
    public void Show(SpriteArtworkPlacement placement)
    {
        Ensure.NotNull(placement);
        // A placement wholly covered by a later one no longer shows anywhere; dropping it keeps the record bounded.
        for (int index = placements.Count - 1; index >= 0; index--)
        {
            if (placement.Covers(placements[index].FirstCharacter, placements[index].CharacterCount))
                placements.RemoveAt(index);
        }
        placements.Add(placement);
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
}
