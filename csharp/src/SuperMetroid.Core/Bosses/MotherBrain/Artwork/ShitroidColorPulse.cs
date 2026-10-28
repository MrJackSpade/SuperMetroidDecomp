using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The Shitroid's normal pulse: shares the target's symmetric dimming curve only when it matches all eight supplied pulse phases.</summary>
internal sealed class ShitroidColorPulse
{
    /// <summary>Visible target inks used to calculate the native pulse when the editable rows agree.</summary>
    private readonly BabyMetroidInitialPalette target;
    /// <summary>Independent pulse rows retained when an edited frame differs from the calculated curve.</summary>
    internal readonly RgbColor[][]? supplied;

    /// <summary>Compares each supplied ink with the target-derived pulse before deciding whether rows can be shared.</summary>
    public ShitroidColorPulse(RgbColor[][] frames, BabyMetroidInitialPalette target)
    {
        this.target = target;
        // Sharing the target's innard colors is valid only when every supplied
        // pulse sample agrees. Independently edited targets or frames stay exact.
        for (int frame = 0; frame < frames.Length; frame++)
        for (int color = 0; color < ShitroidColorDefinitions.NormalColorsPerFrame; color++)
            if (Resolve(frame, color) != frames[frame][color])
            { supplied = frames; return; }
    }

    /// <summary>Uses the supplied ink or mirrors the eight-phase dimming curve around its two darkest phases, retaining the organ's red floor.</summary>
    public RgbColor Resolve(int frame, int color)
    {
        if (supplied is not null) return supplied[frame][color];
        int phase = Math.Min(frame, ShitroidColorDefinitions.NormalFrameCount - 1 - frame);
        RgbColor paint = target.Resolve(ShitroidColorDefinitions.NormalFirstInitialColor + color);
        return paint.Map((channel, origin) =>
        {
            int floor = 0;
            if (channel == ColorChannel.Red)
            {
                floor = ShitroidColorDefinitions.NormalOrganRedFloor;
                if (origin == 31) origin += ShitroidColorDefinitions.NormalOrganRedHeadroom;
            }
            return Math.Clamp(origin - ShitroidColorDefinitions.NormalOrganDimmingStep * phase, floor, 31);
        });
    }
}
