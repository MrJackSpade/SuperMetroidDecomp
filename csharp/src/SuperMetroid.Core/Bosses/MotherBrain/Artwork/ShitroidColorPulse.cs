using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The Shitroid's normal pulse: shares the target's symmetric dimming curve only when it matches all eight supplied pulse phases.</summary>
internal sealed class ShitroidColorPulse
{
    /// <summary>Visible target inks used to calculate the pulse when the editable phases agree.</summary>
    private readonly BabyMetroidInitialPalette target;
    /// <summary>Independent pulse phases retained when an edited phase differs from the calculated curve.</summary>
    internal readonly RgbColor[][]? supplied;

    /// <summary>Keeps the target the pulse may share and, when a phase differs from the shared curve, every supplied phase.</summary>
    /// <param name="target">The visible target inks the pulse is calculated from.</param>
    /// <param name="supplied">The supplied phases when one differs from the target-derived curve, otherwise null.</param>
    internal ShitroidColorPulse(BabyMetroidInitialPalette target, RgbColor[][]? supplied)
    {
        this.target = target;
        this.supplied = supplied;
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
