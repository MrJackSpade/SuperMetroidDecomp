namespace SuperMetroid.Core.Hardware;

/// <summary>
/// What shows over the OBJ character table's characters as background characters (#1392): the Ceres Ridley arena points
/// BG1 and BG2 at the table, so its blocks show the uploads it records. An upload shows under the name of its source.
/// </summary>
/// <param name="uploads">The table's uploads.</param>
[PpuLayoutBoundary("codec: the OBJ character table's characters as background characters")]
public sealed class ObjPageCharacterArtwork(SpriteCharacters uploads) : IBgCharacterArtwork
{
    /// <inheritdoc />
    public BgArtworkShown Showing(int firstCharacter, int characterCount)
    {
        SpriteCharacterPlacement shown = Ensure.NotNull(uploads).LastOver(firstCharacter, characterCount)
            ?? throw new InvalidDataException($"Nothing is shown over sprite characters {firstCharacter}..{firstCharacter + characterCount - 1}.");
        if (!shown.Covers(firstCharacter, characterCount))
            throw new InvalidDataException($"{shown.Source} shows over only some of sprite characters {firstCharacter}..{firstCharacter + characterCount - 1}.");
        return new BgArtworkShown(new BgArtworkIdentity($"{shown.Source}"),
            (firstCharacter - shown.FirstCharacter) * SpriteCharacters.CharacterByteCount / sizeof(ushort));
    }
}
