using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The sprite artwork a background shows (#1392, #1422, #1463): the Ceres Ridley arena points its backgrounds at the
/// sprites' artwork, so its blocks show the looks shown there, as the installed enemy sheets resolve them when a block is
/// drawn.
/// </summary>
/// <param name="sprites">The sprites' artwork the background shows.</param>
public sealed class SpriteArtworkBackground(ISpriteObjectPage sprites) : IBgCharacterArtwork
{
    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">No enemy sheets are installed.</exception>
    public BgArtworkShown Showing(int firstCharacter, int characterCount) =>
        Ensure.NotNull(sprites).ObjectPage().Showing(firstCharacter, characterCount);
}
