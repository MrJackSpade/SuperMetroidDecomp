using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Compatibility;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// How the backend shows a fade (#2289, #2294, #2563): each channel's colour step (<see cref="FadeLevelColour"/>) moves by the
/// fade's part of the way, truncated to a whole step, so every colour a fade shows is one the game shows: a door's fades,
/// the death fade, the time-up whiteout, a picture's fades between its shadings, the opening's crossfade and a white wash.
/// </summary>
internal sealed class D3D11ColourBlend : IColourBlend
{
    /// <summary>The one blend the backend draws every frame with.</summary>
    internal static D3D11ColourBlend Instance { get; } = new();

    /// <summary>Only <see cref="Instance"/> blends.</summary>
    private D3D11ColourBlend()
    {
    }

    /// <inheritdoc />
    public RgbColor Partway(RgbColor from, RgbColor to, FadeLevel progress)
    {
        FadeLevelColour start = FadeLevelColour.Of(from);
        FadeLevelColour end = FadeLevelColour.Of(to);
        return start.Map((channel, step) => step + ((end[channel] - step) * progress.Step / progress.Steps)).ToColour();
    }

    /// <inheritdoc />
    public RgbColor AtLevel(RgbColor colour, FadeLevel level) =>
        FadeLevelColour.Of(colour).Map((_, step) => step * level.Step / level.Steps).ToColour();
}
