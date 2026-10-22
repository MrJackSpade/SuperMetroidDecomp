using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The pause screen <c>GameState_13</c> sets up (#1358): the installed artwork it draws, its own tilemaps and palette,
/// and its sprite entries. It publishes these; the capture composes them, so pause owns no PPU memory.
/// </summary>
internal sealed class PauseScreen
{
    /// <summary>Installs the map, pause interface, HUD and menu sprite artwork.</summary>
    public PauseScreen(AreaMapPresentationCatalog artwork)
    {
        Artwork = Ensure.NotNull(artwork);
        Objects = new ScreenSprites();
    }

    /// <summary>The installed artwork the screen draws.</summary>
    [field: NonSerialized]
    [AllowNull]
    public AreaMapPresentationCatalog Artwork
    {
        get => field ?? throw new InvalidOperationException("The pause screen's artwork is rebound after debugger restoration.");
        private set;
    }

    /// <summary>The pause tilemaps.</summary>
    public PauseTilemaps Tilemaps { get; } = new();

    /// <summary>The pause palette, with its live highlight and reserve-arrow colours.</summary>
    public ScreenPalette Palette { get; } = new();

    /// <summary>The pause sprite entries over the installed menu OBJ page.</summary>
    public ScreenSprites Objects { get; }

    /// <summary>Rebinds the installed artwork, keeping tilemaps, palette and sprite entries.</summary>
    public void BindArtwork(AreaMapPresentationCatalog artwork)
    {
        Artwork = Ensure.NotNull(artwork);
        characters = null;
    }

    /// <summary>The composed character image of the bound artwork, until a rebind replaces it.</summary>
    [NonSerialized]
    private byte[]? characters;

    /// <summary>The bound artwork's composed character image.</summary>
    internal ReadOnlySpan<byte> Characters => characters ??= PauseScreenImage.Characters(Artwork);
}
