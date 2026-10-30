using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>
/// Lays out what a message box shows as tile words (#1466): the words the installed messages give the form the box shows,
/// so verification compares the form the game names with the words the cartridge's message routines write.
/// </summary>
internal static class GameplayMessageBoxStateTiles
{
    /// <summary>The words of what the box shows.</summary>
    extension(GameplayMessageBoxState self)
    {
        /// <summary>The tile words <paramref name="messages"/> lays out for what the box shows, 32 per row.</summary>
        /// <exception cref="InvalidOperationException">The box shows no message.</exception>
        internal BackgroundTile[] Tilemap(GameplayMessageArtwork messages) => Ensure.NotNull(messages).Tiles(
            self.Shown ?? throw new InvalidOperationException("The message box shows no message."));

        /// <summary>Number of consecutive 32-tile rows <paramref name="messages"/> lays out for what the box shows.</summary>
        internal int TilemapRowCount(GameplayMessageArtwork messages) =>
            self.Tilemap(messages).Length / GameplayMessageBoxDefinitions.Layout.TilemapWidth;
    }
}
