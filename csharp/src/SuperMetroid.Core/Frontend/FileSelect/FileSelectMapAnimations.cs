using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// The map's scroll arrows, each shown while the map can scroll its way, and the highlight cycle, advanced by menu updates
/// only.
/// </summary>
public sealed partial class FileSelectMapAnimations
{
    /// <summary>Each map-scroll arrow's anchor, picture and visibility, by the direction it scrolls.</summary>
    internal readonly Dictionary<MapScrollDirection, Arrow> arrows = [];
    /// <summary>The highlight cycle, advanced separately from arrow timing.</summary>
    private readonly MapHighlightAnimation highlight;
    /// <summary>Creates the map menu's arrows at their installed anchors and its highlight cycle over its installed timing.</summary>
    /// <param name="presentation">The installed arrow anchors.</param>
    /// <param name="highlightCycle">The installed highlight cycle's timing.</param>
    public FileSelectMapAnimations(MapArrowPresentation presentation, MapHighlightCycle highlightCycle)
    {
        highlight = new MapHighlightAnimation(highlightCycle);
        BindPresentation(presentation);
    }

    /// <summary>Binds the installed arrow anchors when the game is bound to its installed content, keeping which arrows show.</summary>
    internal void BindPresentation(MapArrowPresentation content)
    {
        Ensure.NotNull(content);
        foreach (MapScrollDirection direction in MapScrollControls.Directions)
        {
            Arrow replacement = new(content.Get(direction).Position, Ensure.Listed(MapArrowDefinitions.ArrowPictures, direction));
            if (arrows.GetValueOrDefault(direction) is { } previous)
                replacement.Admit(previous.Visible);
            arrows[direction] = replacement;
        }
    }

    /// <summary>Restarts the highlight cycle and leaves the arrows as they are.</summary>
    public void ResetHighlight() => highlight.Reset();
    /// <summary>Replaces the installed highlight timing while retaining the menu-owned cycle state.</summary>
    internal void BindHighlight(MapHighlightCycle cycle) => highlight.Bind(cycle);

    /// <summary>Advances the highlight and returns whether its cycle starts again.</summary>
    /// <returns>True when the cycle wraps and the map's highlight-loop sound should be queued; this method does not enqueue audio itself.</returns>
    public bool StepHighlight() => highlight.Step();

    /// <summary>The shading the file-select map's highlighted sprites show at the highlight cycle's current frame (#1512).</summary>
    public PictureShading HighlightShading => highlight.Shading;

    /// <summary>Shows each arrow this update only while the map can scroll its way.</summary>
    /// <param name="available">Whether the map can scroll each way; an arrow the map cannot scroll is hidden.</param>
    public void StepArrows(Func<MapScrollDirection, bool> available)
    {
        Ensure.NotNull(available);
        foreach (MapScrollDirection direction in MapScrollControls.Directions)
            arrows[direction].Admit(available(direction));
    }

    /// <summary>Appends the shown arrows' pictures to the sprite list in the map highlight shade, without changing which show.</summary>
    /// <param name="entries">The screen's sprites, among which the arrows are drawn.</param>
    /// <param name="highlight">The shading of the highlight cycle's current frame on the screen that draws the arrows (#1512).</param>
    /// <param name="sheet">The installed map sprite sheet.</param>
    /// <param name="verticalOffset">Screen-pixel Y translation added to each bound arrow anchor; pause-map rendering supplies its banner offset here.</param>
    public void DrawArrows(ScreenSprites entries, PictureShading highlight, SpriteSheet sheet, int verticalOffset = 0)
    {
        SpriteSheet installed = Ensure.NotNull(sheet);
        foreach (MapScrollDirection direction in MapScrollControls.Directions)
        {
            Arrow arrow = arrows[direction];
            if (!arrow.Visible) continue;
            MapSpriteSheet.Draw(installed, arrow.Picture, entries, arrow.Position + new Offset16<ScreenPixels>(0, verticalOffset),
                highlight);
        }
    }

    /// <summary>One map-scroll arrow: its installed anchor, its picture and whether it shows.</summary>
    internal sealed partial class Arrow(Offset16<ScreenPixels> position, MapSpriteId picture)
    {
        /// <summary>Installed screen anchor.</summary>
        public readonly Offset16<ScreenPixels> Position = position;
        /// <summary>The arrow's picture for its scroll direction.</summary>
        public readonly MapSpriteId Picture = picture;
        /// <summary>Whether the arrow shows: whether the map could scroll its way at the last update.</summary>
        public bool Visible { get; private set; }

        /// <summary>Records whether the arrow shows this update.</summary>
        internal void Admit(bool visible) => Visible = visible;
    }
}
