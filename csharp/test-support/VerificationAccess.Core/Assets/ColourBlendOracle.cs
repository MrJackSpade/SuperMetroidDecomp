namespace SuperMetroid.Core.Assets;

/// <summary>
/// The checks' own blending of a fade (#2563), the oracle a check reads a shading's colours through: each channel's colour
/// step moves by the fade's part of the way, truncated to a whole step, or as the cartridge's gradual colour change moves it
/// for a fade that names it (#3036), as the backend shows a fade.
/// </summary>
internal sealed class ColourBlendOracle : IColourBlend
{
    /// <summary>The one blend checks read colours through.</summary>
    internal static ColourBlendOracle Instance { get; } = new();

    /// <summary>Only <see cref="Instance"/> blends.</summary>
    private ColourBlendOracle()
    {
    }

    /// <inheritdoc />
    public RgbColor Partway(RgbColor from, RgbColor to, FadeLevel progress)
    {
        FadeLevelColour start = FadeLevelColour.Of(from);
        FadeLevelColour end = FadeLevelColour.Of(to);
        return progress.Arithmetic switch
        {
            FadeArithmetic.Proportional =>
                start.Map((channel, step) => step + ((end[channel] - step) * progress.Step / progress.Steps)).ToColour(),
            FadeArithmetic.GradualColourChange =>
                start.Map((channel, step) => SuperMetroid.Core.Compatibility.GradualColourChange.Channel(step, end[channel], progress.Step, progress.Steps)).ToColour(),
            _ => throw new ArgumentOutOfRangeException(nameof(progress), progress.Arithmetic, "Undefined fade arithmetic."),
        };
    }

    /// <inheritdoc />
    public RgbColor AtLevel(RgbColor colour, FadeLevel level) =>
        FadeLevelColour.Of(colour).Map((_, step) => step * level.Step / level.Steps).ToColour();
}
