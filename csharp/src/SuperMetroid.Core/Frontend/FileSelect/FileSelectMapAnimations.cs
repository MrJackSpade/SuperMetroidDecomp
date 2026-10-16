using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>$82:A881 arrow animations and $82:A92B palette animation, advanced by menu ticks only.</summary>
public sealed class FileSelectMapAnimations
{
    /// <summary>Independent visibility, phase, and delay state for each map-scroll arrow.</summary>
    internal readonly Arrow[] arrows = new Arrow[MapArrowDefinitions.Count];
    /// <summary>Native palette cycle state advanced separately from arrow timing.</summary>
    private readonly MapPaletteAnimation palette;
    /// <summary>Installed visual phases rebound after state restoration without serializing artwork.</summary>
    [NonSerialized] private MapArrowPresentation? presentation;

    /// <summary>Creates independent arrow counters and palette timing for a map menu, binding installed arrow positions and phase durations without reading cartridge animation data.</summary>
    /// <param name="presentation">Required installed arrow definitions, retained as host content rather than serialized with the counters.</param>
    /// <exception cref="ArgumentNullException">The address space is null.</exception>
    /// <exception cref="InvalidOperationException">Installed arrow definitions are absent, including when the optional argument is omitted.</exception>
    public FileSelectMapAnimations(MapArrowPresentation? presentation = null)
    {
        palette = new MapPaletteAnimation();
        BindPresentation(presentation);
    }

    /// <summary>Rebind current artwork after restoration without resetting visible-arrow timing.</summary>
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
            replacement = new Arrow(visual.Position, MapArrowDefinitions.SpriteBase(direction));
            phaseCount = visual.PhaseCount;
            if (previous is not null)
            {
                // A shortened replacement cycle retains the remaining accepted delay,
                // then continues from the corresponding phase in the new presentation.
                replacement.Frame = previous.Frame % phaseCount;
                replacement.Timer = previous.Timer;
                replacement.Visible = previous.Visible;
            }
            arrows[index] = replacement;
        }
        presentation = content;
    }

    /// <summary>ResetPauseMenuAnimations resets palette timing but does not clear the arrow counters.</summary>
    public void ResetPalette() => palette.Reset();
    /// <summary>Replaces the installed palette colors and durations while retaining the menu-owned cycle state.</summary>
    internal void BindPalette(SuperMetroid.Core.Frontend.Map.Artwork.MapPaletteCycle? cycle) => palette.Bind(cycle);

    /// <summary>Returns the native library-three sound request at the palette loop terminator.</summary>
    /// <param name="cgram">Palette memory receiving the bound OBJ palette-three colors when this menu tick reaches a frame boundary.</param>
    /// <returns>True when the cycle wraps and the caller should queue MapPaletteLoop; this method does not enqueue audio itself.</returns>
    public bool StepPalette(SnesCgram cgram) => palette.Step(cgram);

    /// <summary>Arrows advance only when their native boundary test allows them to be drawn.</summary>
    /// <param name="available">Direction-specific scroll-boundary predicate; unavailable arrows become hidden and retain their existing frame and remaining timer.</param>
    /// <remarks>One call consumes one menu tick for visible arrows; phase durations are menu-tick counts, with the native $FF terminator wrapping to phase zero.</remarks>
    public void StepArrows(Func<MapScrollDirection, bool> available)
    {
        for (int index = 0; index < arrows.Length; index++)
        {
            Arrow arrow = arrows[index];
            arrow.Visible = available(MapScrollControls.ArrowAt(index));
            if (!arrow.Visible) continue;
            MapArrowVisual visual = (presentation ?? throw new InvalidOperationException(
                "Map arrows require installed animation definitions.")).Get(MapScrollControls.ArrowAt(index));
            if (--arrow.Timer <= 0)
            {
                arrow.Frame++;
                byte delay = arrow.Frame == visual.PhaseCount ? byte.MaxValue : visual.Duration(arrow.Frame);
                if (delay == byte.MaxValue)
                {
                    arrow.Frame = 0;
                    delay = visual.Duration(0);
                    if (delay == byte.MaxValue) throw new InvalidDataException("Map arrow animation has no frames.");
                }
                arrow.Timer = delay;
            }
        }
    }

    /// <summary>Appends currently visible arrow compositions to OAM using OBJ palette three, without advancing counters or changing scroll availability.</summary>
    /// <param name="oam">Destination sprite buffer owned by the menu frame being composed.</param>
    /// <param name="sprites">Required installed map sprite compositions; no cartridge-art fallback is used when absent.</param>
    /// <param name="verticalOffset">Screen-pixel Y translation added to each bound arrow anchor with unsigned 16-bit wrapping; pause-map rendering supplies its header offset here.</param>
    /// <exception cref="InvalidOperationException">Installed sprite definitions are absent, including when the optional argument is omitted.</exception>
    public void DrawArrows(OamBuffer oam, MapSpriteCatalog? sprites = null, int verticalOffset = 0)
    {
        MapSpriteCatalog installed = sprites ?? throw new InvalidOperationException(
            "Map arrows require installed sprite definitions.");
        foreach (Arrow arrow in arrows)
        {
            if (!arrow.Visible) continue;
            installed.Draw(arrow.Base, oam, arrow.Position + new Offset16<OamPixels>(0, unchecked((short)verticalOffset)),
                SnesObjPalettes.Index3.PaletteBits);
        }
    }

    /// <summary>One map-scroll arrow's bound anchor and sprite identity with menu-tick counters.</summary>
    internal sealed class Arrow(Position16<OamPixels> position, MapSpriteId spriteBase)
    {
        /// <summary>Installed screen anchor retained while the arrow advances its visual phase.</summary>
        public readonly Position16<OamPixels> Position = position;
        /// <summary>Sprite composition selected for this scroll direction.</summary>
        public readonly MapSpriteId Base = spriteBase;
        /// <summary>Remaining menu ticks before the next phase transition.</summary>
        public int Timer;
        /// <summary>Current presentation phase, preserved modulo replacement phase count on rebinding.</summary>
        public int Frame;
        /// <summary>Current scroll-boundary admission result; hidden arrows retain their counters.</summary>
        public bool Visible;
    }
}
