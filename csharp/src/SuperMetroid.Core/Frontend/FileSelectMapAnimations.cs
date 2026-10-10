using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>$82:A881 arrow animations and $82:A92B palette animation, advanced by menu ticks only.</summary>
public sealed class FileSelectMapAnimations
{
    private readonly Arrow[] arrows = new Arrow[MapArrowDefinitions.Count];
    private readonly MapPaletteAnimation palette;
    [NonSerialized] private MapArrowPresentation? presentation;

    /// <summary>Creates independent arrow counters and palette timing for a map menu, binding installed arrow positions and phase durations without reading cartridge animation data.</summary>
    /// <param name="bus">Required address-space dependency passed to palette initialization; construction validates it but does not read animation bytes from it.</param>
    /// <param name="presentation">Required installed arrow definitions, retained as host content rather than serialized with the counters.</param>
    /// <exception cref="ArgumentNullException">The address space is null.</exception>
    /// <exception cref="InvalidOperationException">Installed arrow definitions are absent, including when the optional argument is omitted.</exception>
    public FileSelectMapAnimations(ISnesAddressSpace bus, MapArrowPresentation? presentation = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        palette = new MapPaletteAnimation(bus);
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
            var direction = (MapScrollDirection)(index + 1);
            MapArrowVisual visual = installed.Get(direction);
            replacement = new Arrow(visual.X, visual.Y, MapArrowDefinitions.SpriteBase(direction));
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
    internal void BindPalette(SuperMetroid.Core.Assets.MapPaletteCycle? cycle) => palette.Bind(cycle);

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
            arrow.Visible = available((MapScrollDirection)(index + 1));
            if (!arrow.Visible) continue;
            MapArrowVisual visual = (presentation ?? throw new InvalidOperationException(
                "Map arrows require installed animation definitions.")).Get((MapScrollDirection)(index + 1));
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
            installed.Draw(arrow.Base, oam, arrow.X, unchecked((ushort)(arrow.Y + verticalOffset)), SnesObjPalettes.Index3.PaletteBits);
        }
    }

    private sealed class Arrow(ushort x, ushort y, MapSpriteId spriteBase)
    {
        public readonly ushort X = x, Y = y;
        public readonly MapSpriteId Base = spriteBase;
        public int Timer, Frame;
        public bool Visible;
    }
}
