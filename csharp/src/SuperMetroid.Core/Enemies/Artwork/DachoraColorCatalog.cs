using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Animals;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Editable default, speed, and shine RGB5 images for Dachora. Native speed/shine
/// timers and their frame-index arithmetic remain compiled enemy behavior.
/// </summary>
public sealed class DachoraColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("DachoraColorCatalog-v1", content =>
        {
            content.AppendColors("normal", normal);
            content.AppendColorFrames("speed", Frames(DachoraPalettePhase.Speed));
            content.AppendColorFrames("shine", Frames(DachoraPalettePhase.Shine));
        });

    /// <summary>The sixteen supplied normal colors used as the basis of both animated phases.</summary>
    private readonly Bgr555[] normal;
    /// <summary>Speed-frame colors that differ from the normal palette's calculated blue ramp.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> speedEdits = new();
    /// <summary>Shine-frame colors that differ from the calculated fade toward white.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> shineEdits = new();

    /// <summary>Retains the normal palette and only deviations from calculated speed and shine frames.</summary>
    internal DachoraColorCatalog(Bgr555[] normal, Bgr555[][] speed, Bgr555[][] shine)
    {
        this.normal = normal;
        for (int frame = 0; frame < DachoraColorRomData.AnimatedFrameCount; frame++)
        for (int color = 0; color < DachoraColorRomData.ColorsPerFrame; color++)
        {
            var key = new ColourCell(frame, color);
            if (speed[frame][color] != SpeedColor(frame, color)) speedEdits.Add(key, speed[frame][color]);
            if (shine[frame][color] != ShineColor(frame, color)) shineEdits.Add(key, shine[frame][color]);
        }
    }

    /// <summary>$A7:F245 speed-boost sequence's flash backdrop in its first frame, authored paint.</summary>
    private static readonly Bgr555 SpeedFlashBackdrop = new(0, 12, 5);

    /// <summary>
    /// $A7:F245..F2C4: frame k raises each color's blue channel toward min(blue + 16, 31),
    /// rounding to nearest over three steps; red and green keep the normal color. The first
    /// frame's transparent slot is the authored flash backdrop, later frames keep the normal one.
    /// </summary>
    private Bgr555 SpeedColor(int frame, int color)
    {
        Bgr555 basis = normal[color];
        if (color == 0) return frame == 0 ? SpeedFlashBackdrop : basis;
        int blue = basis.Blue;
        int target = Math.Min(blue + 16, 31);
        int steps = DachoraColorRomData.AnimatedFrameCount - 1;
        int shifted = blue + ((target - blue) * frame + steps / 2) / steps;
        return basis.WithBlue(shifted);
    }

    /// <summary>
    /// $A7:F2C5..F344: frame k fades every channel toward white by k/5, as
    /// channel + floor(k * (31 - channel) / 5); the transparent slot keeps the normal color.
    /// </summary>
    private Bgr555 ShineColor(int frame, int color)
    {
        Bgr555 basis = normal[color];
        if (color == 0) return basis;
        return basis.Map((_, channel) => channel + frame * (31 - channel) / 5);
    }

    /// <summary>Materializes four selected animated frames for stable content hashing.</summary>
    private Bgr555[][] Frames(DachoraPalettePhase phase) =>
        Enumerable.Range(0, DachoraColorRomData.AnimatedFrameCount)
            .Select(frame => Enumerable.Range(0, DachoraColorRomData.ColorsPerFrame)
                .Select(color => Resolve(phase, frame, color)).ToArray()).ToArray();

    /// <summary>Resolves one selected Dachora color, using the normal palette as a calculated animated basis while preserving every supplied speed/shine deviation.</summary>
    /// <param name="phase">Exclusive default, speed-boost, or stored-shine palette phase selected by enemy behavior.</param>
    /// <param name="frame">Zero for the default phase, or zero-based animated frame 0 through 3 for speed and shine.</param>
    /// <param name="color">Palette color index 0 through 15, including the transparent slot at zero.</param>
    /// <returns>SNES RGB555 word with red in bits 0..4, green in bits 5..9, and blue in bits 10..14; the caller chooses the actor's OBJ palette destination.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The phase/frame combination is unsupported or the color index is outside the palette.</exception>
    public Bgr555 Resolve(DachoraPalettePhase phase, int frame, int color)
    {
        bool animated = phase is DachoraPalettePhase.Speed or DachoraPalettePhase.Shine;
        if (!(phase == DachoraPalettePhase.Default && frame == 0) &&
            !(animated && (uint)frame < DachoraColorRomData.AnimatedFrameCount))
            throw new ArgumentOutOfRangeException(nameof(frame), $"Dachora phase {phase} has no frame {frame}.");
        if ((uint)color >= DachoraColorRomData.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        var key = new ColourCell(frame, color);
        return phase switch
        {
            DachoraPalettePhase.Speed => speedEdits.TryGetValue(key, out Bgr555 speed) ? speed : SpeedColor(frame, color),
            DachoraPalettePhase.Shine => shineEdits.TryGetValue(key, out Bgr555 shine) ? shine : ShineColor(frame, color),
            DachoraPalettePhase.Default => normal[color],
            _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "Undefined Dachora palette phase."),
        };
    }
}
