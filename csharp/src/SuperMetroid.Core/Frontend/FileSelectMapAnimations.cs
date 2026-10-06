using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>$82:A881 arrow animations and $82:A92B palette animation, advanced by menu ticks only.</summary>
public sealed class FileSelectMapAnimations
{
    private readonly Arrow[] arrows = new Arrow[MapArrowDefinitions.Count];
    private readonly MapPaletteAnimation palette;
    [NonSerialized] private MapArrowPresentation? presentation;

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
            replacement = new Arrow(visual.X, visual.Y, 0, MapArrowDefinitions.SpriteBase(direction));
            phaseCount = visual.PhaseCount;
            if (previous is not null)
            {
                // A shortened replacement cycle retains the remaining accepted delay,
                // then continues from the corresponding phase in the new presentation.
                replacement.Frame = previous.Frame % phaseCount;
                replacement.Timer = previous.Timer;
                replacement.Visible = previous.Visible;
                replacement.Spritemap = replacement.Base;
            }
            arrows[index] = replacement;
        }
        presentation = content;
    }

    /// <summary>ResetPauseMenuAnimations resets palette timing but does not clear the arrow counters.</summary>
    public void ResetPalette() => palette.Reset();
    internal void BindPalette(SuperMetroid.Core.Assets.MapPaletteCycle? cycle) => palette.Bind(cycle);

    /// <summary>Returns the native library-three sound request at the palette loop terminator.</summary>
    public bool StepPalette(SnesCgram cgram) => palette.Step(cgram);

    /// <summary>Arrows advance only when their native boundary test allows them to be drawn.</summary>
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
            arrow.Spritemap = arrow.Base;
        }
    }

    public void DrawArrows(OamBuffer oam, MapSpriteCatalog? sprites = null, int verticalOffset = 0)
    {
        MapSpriteCatalog installed = sprites ?? throw new InvalidOperationException(
            "Map arrows require installed sprite definitions.");
        foreach (Arrow arrow in arrows)
        {
            if (!arrow.Visible) continue;
            installed.Draw(arrow.Spritemap, oam, arrow.X, unchecked((ushort)(arrow.Y + verticalOffset)), SnesObjPalettes.Index3.PaletteBits);
        }
    }

    private sealed class Arrow(ushort x, ushort y, ushort program, ushort spriteBase)
    {
        public readonly ushort X = x, Y = y, Program = program, Base = spriteBase;
        public int Timer, Frame;
        public ushort Spritemap;
        public bool Visible;
    }
}
