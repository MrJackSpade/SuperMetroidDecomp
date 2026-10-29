using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The sprite artwork a background shows as its characters (#1392, #1422, #1463): the Ceres Ridley arena points its
/// backgrounds at the sprite characters, so its blocks show what gameplay's shown looks leave placed there, each under the
/// name of its look. The placements are replayed from the looks whenever another look has been shown since.
/// </summary>
/// <param name="looks">Gameplay's shown sprite looks.</param>
internal sealed class ObjectPageBackground(ShownSpriteArtwork looks) : IBgCharacterArtwork
{
    /// <summary>The placements the looks left when <see cref="placedAt"/> was their latest showing.</summary>
    private SpriteArtworkPlacements? placed;

    /// <summary>The latest showing <see cref="placed"/> was replayed through.</summary>
    private long placedAt;

    /// <inheritdoc />
    public BgArtworkShown Showing(int firstCharacter, int characterCount)
    {
        if (placed is null || placedAt != looks.Recorded)
        {
            placed = SpriteArtworkPlacements.Of(looks);
            placedAt = looks.Recorded;
        }
        SpriteArtworkPlacement shown = placed.LastOver(firstCharacter, characterCount)
            ?? throw new InvalidDataException($"Nothing is shown over sprite characters {firstCharacter}..{firstCharacter + characterCount - 1}.");
        if (!shown.Covers(firstCharacter, characterCount))
            throw new InvalidDataException($"{shown.Source} shows over only some of sprite characters {firstCharacter}..{firstCharacter + characterCount - 1}.");
        return new BgArtworkShown(new BgArtworkIdentity($"{shown.Source}"),
            (firstCharacter - shown.FirstCharacter) * SpriteArtworkPlacements.CharacterByteCount / sizeof(ushort));
    }
}
