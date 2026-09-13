using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>$82:A881 arrow animations and $82:A92B palette animation, advanced by menu ticks only.</summary>
public sealed class FileSelectMapAnimations
{
    private readonly ISnesAddressSpace bus;
    private readonly Arrow[] arrows = new Arrow[MapArrowDefinitions.Count];
    private readonly MapPaletteAnimation palette;
    [NonSerialized] private MapArrowPresentation? presentation;

    public FileSelectMapAnimations(ISnesAddressSpace bus, MapArrowPresentation? presentation = null)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
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
            if (content is not null)
            {
                var direction = (MapScrollDirection)(index + 1);
                MapArrowVisual visual = content.Get(direction);
                replacement = new Arrow(visual.X, visual.Y, 0, MapArrowDefinitions.SpriteBase(direction));
                phaseCount = visual.PhaseCount;
            }
            else
            {
                int record = FileSelectMapRomData.ScrollArrows + index * 10;
                ushort animation = Read(record + 4);
                if (animation is < 1 or > 9) throw new InvalidDataException("Invalid menu arrow animation ID.");
                ushort program = Read(MapAnimationRomData.SpritePrograms + (animation - 1) * 2);
                ushort bases = Read(MapAnimationRomData.SpriteBases + (animation - 1) * 2);
                replacement = new Arrow(Read(record), unchecked((ushort)(Read(record + 2) - 1)),
                    program, Read(FileSelectMapRomData.MenuObjectBank | bases));
                phaseCount = 0;
                while (bus.ReadByte(FileSelectMapRomData.MenuObjectBank | (program + phaseCount * 3)) != byte.MaxValue)
                    if (++phaseCount > MapArrowFormat.MaximumPhases) throw new InvalidDataException("Unterminated map arrow animation.");
                if (phaseCount == 0) throw new InvalidDataException("Map arrow animation has no frames.");
            }
            if (previous is not null)
            {
                // A shortened replacement cycle retains the remaining accepted delay,
                // then continues from the corresponding phase in the new presentation.
                replacement.Frame = previous.Frame % phaseCount;
                replacement.Timer = previous.Timer;
                replacement.Visible = previous.Visible;
                replacement.Spritemap = (ushort)(replacement.Base + (content is null ? AnimationByte(replacement, 2) : 0));
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
            MapArrowVisual? visual = presentation?.Get((MapScrollDirection)(index + 1));
            if (--arrow.Timer <= 0)
            {
                arrow.Frame++;
                byte delay = visual is null ? AnimationByte(arrow, 0)
                    : arrow.Frame == visual.PhaseCount ? byte.MaxValue : visual.Duration(arrow.Frame);
                if (delay == byte.MaxValue)
                {
                    arrow.Frame = 0;
                    delay = visual is null ? AnimationByte(arrow, 0) : visual.Duration(0);
                    if (delay == byte.MaxValue) throw new InvalidDataException("Map arrow animation has no frames.");
                }
                arrow.Timer = delay;
            }
            arrow.Spritemap = (ushort)(arrow.Base + (visual is null ? AnimationByte(arrow, 2) : 0));
        }
    }

    public void DrawArrows(OamBuffer oam)
    {
        foreach (Arrow arrow in arrows)
        {
            if (!arrow.Visible) continue;
            ushort pointer = Read(MenuPpuState.SpritemapPointerTableAddress + arrow.Spritemap * 2);
            oam.AddOnScreenSpritemap(bus, FileSelectMapRomData.MenuObjectBank | pointer,
                arrow.X, arrow.Y, SnesObjPalettes.Index3.PaletteBits);
        }
    }

    private byte AnimationByte(Arrow arrow, int offset) => bus.ReadByte(
        FileSelectMapRomData.MenuObjectBank | unchecked((ushort)(arrow.Program + arrow.Frame * 3 + offset)));
    private ushort Read(int address) => RomDataReader.ReadWordFixedBank(bus, address);

    private sealed class Arrow(ushort x, ushort y, ushort program, ushort spriteBase)
    {
        public readonly ushort X = x, Y = y, Program = program, Base = spriteBase;
        public int Timer, Frame;
        public ushort Spritemap;
        public bool Visible;
    }
}
