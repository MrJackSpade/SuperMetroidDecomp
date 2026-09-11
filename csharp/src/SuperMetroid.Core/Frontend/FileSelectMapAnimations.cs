using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>$82:A881 arrow animations and $82:A92B palette animation, advanced by menu ticks only.</summary>
public sealed class FileSelectMapAnimations
{
    private readonly ISnesAddressSpace bus;
    private readonly Arrow[] arrows = new Arrow[4];
    private readonly MapPaletteAnimation palette;

    public FileSelectMapAnimations(ISnesAddressSpace bus)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        palette = new MapPaletteAnimation(bus);
        for (int index = 0; index < arrows.Length; index++)
        {
            int record = FileSelectMapRomData.ScrollArrows + index * 10;
            ushort animation = Read(record + 4);
            if (animation is < 1 or > 9) throw new InvalidDataException("Invalid menu arrow animation ID.");
            ushort program = Read(MapAnimationRomData.SpritePrograms + (animation - 1) * 2);
            ushort bases = Read(MapAnimationRomData.SpriteBases + (animation - 1) * 2);
            arrows[index] = new Arrow(Read(record), unchecked((ushort)(Read(record + 2) - 1)),
                program, Read(FileSelectMapRomData.MenuObjectBank | bases));
        }
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
            if (--arrow.Timer <= 0)
            {
                arrow.Frame++;
                byte delay = AnimationByte(arrow, 0);
                if (delay == byte.MaxValue)
                {
                    arrow.Frame = 0;
                    delay = AnimationByte(arrow, 0);
                    if (delay == byte.MaxValue) throw new InvalidDataException("Map arrow animation has no frames.");
                }
                arrow.Timer = delay;
            }
            arrow.Spritemap = (ushort)(arrow.Base + AnimationByte(arrow, 2));
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
