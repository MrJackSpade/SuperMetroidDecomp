using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The sprite artwork a background shows as its characters (#1392, #1422): the Ceres Ridley arena points its backgrounds
/// at the sprite characters, so its blocks show the artwork placed there. A placement shows under the name of its source.
/// </summary>
/// <param name="placements">The sprite characters' artwork placements.</param>
[PpuLayoutBoundary("codec: the sprite characters' artwork as background characters")]
public sealed class SpriteArtworkBackground(SpriteArtworkPlacements placements) : IBgCharacterArtwork
{
    /// <inheritdoc />
    public BgArtworkShown Showing(int firstCharacter, int characterCount)
    {
        SpriteArtworkPlacement shown = Ensure.NotNull(placements).LastOver(firstCharacter, characterCount)
            ?? throw new InvalidDataException($"Nothing is shown over sprite characters {firstCharacter}..{firstCharacter + characterCount - 1}.");
        if (!shown.Covers(firstCharacter, characterCount))
            throw new InvalidDataException($"{shown.Source} shows over only some of sprite characters {firstCharacter}..{firstCharacter + characterCount - 1}.");
        return new BgArtworkShown(new BgArtworkIdentity($"{shown.Source}"),
            (firstCharacter - shown.FirstCharacter) * SpriteArtworkPlacements.CharacterByteCount / sizeof(ushort));
    }
}
