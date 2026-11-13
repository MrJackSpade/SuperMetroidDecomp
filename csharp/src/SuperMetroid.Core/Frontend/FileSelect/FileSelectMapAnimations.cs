using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Map arrow animations and the highlight cycle, advanced by menu ticks only.</summary>
public sealed class FileSelectMapAnimations
{
    /// <summary>Independent visibility, phase, and delay state for each map-scroll arrow.</summary>
    internal readonly Arrow[] arrows = new Arrow[MapArrowDefinitions.Count];
    /// <summary>The highlight cycle, advanced separately from arrow timing.</summary>
    private readonly MapHighlightAnimation highlight = new();
    /// <summary>The installed arrow phases.</summary>
    private MapArrowPresentation? presentation;

    /// <summary>Creates independent arrow counters and highlight timing for a map menu, binding installed arrow positions and phase durations without reading the game's animation data.</summary>
    /// <param name="presentation">Required installed arrow definitions.</param>
    /// <exception cref="ArgumentNullException">The installed arrow definitions are null.</exception>
    /// <exception cref="InvalidOperationException">Installed arrow definitions are absent, including when the optional argument is omitted.</exception>
    public FileSelectMapAnimations(MapArrowPresentation? presentation = null)
    {
        BindPresentation(presentation);
    }

    /// <summary>Binds the installed arrow phases when the game is bound to its installed content, without resetting visible-arrow timing.</summary>
    internal void BindPresentation(MapArrowPresentation? content)
    {
        for (int index = 0; index < arrows.Length; index++)
        {
            Arrow? previous = arrows[index];
            Arrow replacement;
            int phaseCount;
            MapArrowPresentation installed = content ?? throw new InvalidOperationException(
                "Map arrows require installed animation definitions.");
            var direction = MapScrollControls.ArrowAt(index);
            MapArrowVisual visual = installed.Get(direction);
            replacement = new Arrow(visual.Position, MapArrowSprites.SpriteBase(direction));
            phaseCount = visual.PhaseCount;
            if (previous is not null)
                replacement.ContinueFrom(previous, phaseCount);
            arrows[index] = replacement;
        }
        presentation = content;
    }

    /// <summary>Resetting the map's animations resets the highlight cycle but does not clear the arrow counters.</summary>
    public void ResetHighlight() => highlight.Reset();
    /// <summary>Replaces the installed highlight timing while retaining the menu-owned cycle state.</summary>
    internal void BindHighlight(MapHighlightCycle? cycle) => highlight.Bind(cycle);

    /// <summary>Advances the highlight cycle; returns the room-library sound request at the cycle's loop terminator.</summary>
    /// <returns>True when the cycle wraps and the caller should queue the map's highlight-loop sound; this method does not enqueue audio itself.</returns>
    public bool StepHighlight() => highlight.Step();

    /// <summary>The shading the file-select map's highlighted sprites show at the highlight cycle's current frame (#1512).</summary>
    public PictureShading HighlightShading => highlight.Shading;

    /// <summary>Arrows advance only when their boundary test allows them to be drawn.</summary>
    /// <param name="available">Direction-specific scroll-boundary predicate; unavailable arrows become hidden and retain their existing frame and remaining timer.</param>
    /// <remarks>One call consumes one menu tick for visible arrows; phase durations are menu-tick counts, with the loop terminator wrapping to phase zero.</remarks>
    public void StepArrows(Func<MapScrollDirection, bool> available)
    {
        for (int index = 0; index < arrows.Length; index++)
        {
            Arrow arrow = arrows[index];
            if (!arrow.Admit(available(MapScrollControls.ArrowAt(index)))) continue;
            arrow.Step((presentation ?? throw new InvalidOperationException(
                "Map arrows require installed animation definitions.")).Get(MapScrollControls.ArrowAt(index)));
        }
    }

    /// <summary>Appends currently visible arrow compositions to the sprite list in the map highlight shade, without advancing counters or changing scroll availability.</summary>
    /// <param name="entries">Destination sprite buffer owned by the menu frame being composed.</param>
    /// <param name="highlight">The shading of the highlight cycle's current frame on the screen that draws the arrows (#1512).</param>
    /// <param name="sheet">Required installed map sprite sheet; no fallback artwork is used when absent.</param>
    /// <param name="verticalOffset">Screen-pixel Y translation added to each bound arrow anchor; pause-map rendering supplies its header offset here.</param>
    /// <exception cref="InvalidOperationException">Installed sprite definitions are absent, including when the optional argument is omitted.</exception>
    public void DrawArrows(ScreenSprites entries, PictureShading highlight, SpriteSheet? sheet = null, int verticalOffset = 0)
    {
        SpriteSheet installed = sheet ?? throw new InvalidOperationException("Map arrows require the installed map sprite sheet.");
        foreach (Arrow arrow in arrows)
        {
            if (!arrow.Visible) continue;
            MapSpriteSheet.Draw(installed, arrow.Base, entries, arrow.Position + new Offset16<SpritePixels>(0, verticalOffset),
                highlight);
        }
    }

    /// <summary>One map-scroll arrow's bound anchor and sprite identity with menu-tick counters.</summary>
    internal sealed class Arrow(Position16<SpritePixels> position, MapSpriteId spriteBase)
    {
        /// <summary>Installed screen anchor retained while the arrow advances its visual phase.</summary>
        public readonly Position16<SpritePixels> Position = position;
        /// <summary>Sprite composition selected for this scroll direction.</summary>
        public readonly MapSpriteId Base = spriteBase;
        /// <summary>Remaining menu ticks before the next phase transition.</summary>
        public int Timer { get; private set; }
        /// <summary>Current presentation phase, preserved modulo replacement phase count on rebinding.</summary>
        public int Frame { get; private set; }
        /// <summary>Current scroll-boundary admission result; hidden arrows retain their counters.</summary>
        public bool Visible { get; private set; }

        /// <summary>
        /// Takes over <paramref name="previous"/>'s counters on rebinding: a shortened replacement cycle retains the
        /// remaining accepted delay, then continues from the corresponding phase of its <paramref name="phaseCount"/>.
        /// </summary>
        internal void ContinueFrom(Arrow previous, int phaseCount)
        {
            Frame = previous.Frame % phaseCount;
            Timer = previous.Timer;
            Visible = previous.Visible;
        }

        /// <summary>Records this tick's scroll-boundary admission and returns it.</summary>
        internal bool Admit(bool visible) => Visible = visible;

        /// <summary>Consumes one menu tick of <paramref name="visual"/>'s cycle; past its last phase the cycle starts again from its first.</summary>
        internal void Step(MapArrowVisual visual)
        {
            if (--Timer > 0)
                return;
            Frame++;
            if (Frame == visual.PhaseCount)
                Frame = 0;
            Timer = visual.Duration(Frame);
        }
    }
}
