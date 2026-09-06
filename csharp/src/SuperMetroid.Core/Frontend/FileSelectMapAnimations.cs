using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>$82:A881 arrow animations and $82:A92B palette animation, advanced by menu ticks only.</summary>
public sealed class FileSelectMapAnimations
{
    private readonly ISnesAddressSpace bus;
    private readonly Arrow[] arrows = new Arrow[4];
    private byte paletteTimer = 1;
    private byte paletteFrame;

    public FileSelectMapAnimations(ISnesAddressSpace bus)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
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
    public void ResetPalette() { paletteTimer = 1; paletteFrame = 0; }

    /// <summary>Returns the native library-three sound request at the palette loop terminator.</summary>
    public bool StepPalette(SnesCgram cgram)
    {
        if (paletteTimer == 0 || --paletteTimer != 0) return false;
        paletteFrame++;
        byte delay = bus.ReadByte(MapAnimationRomData.PaletteTiming + paletteFrame * 3);
        bool looped = delay == byte.MaxValue;
        if (looped)
        {
            paletteFrame = 0;
            delay = bus.ReadByte(MapAnimationRomData.PaletteTiming);
            if (delay == byte.MaxValue) throw new InvalidDataException("Map palette animation has no frames.");
        }
        paletteTimer = delay;
        for (int color = 0; color < 16; color++)
            cgram.SetColor(MapAnimationRomData.PaletteDestination + color,
                Read(MapAnimationRomData.PaletteColors + paletteFrame * 32 + color * 2));
        return looped;
    }

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
                arrow.X, arrow.Y, FileSelectMapRomData.StationMarkerPalette);
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
