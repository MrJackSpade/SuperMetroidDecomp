using SuperMetroid.Core.Frontend.Map;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Shared four-phase pulse for pause and file-select map markers.</summary>
internal static class PauseMapIndicatorAnimation
{
    /// <summary>The pulse has four phases; after the fourth it starts again from the first.</summary>
    public const int FrameCount = 4;

    /// <summary>Calculates the triangular sprite pulse, phase0..3.</summary>
    /// <remarks>Independently reviewed for #1165: start at frame 95, rise twice, then fall.
    /// The bounded triangle is2-abs(2-phase). Invalid indices throw
    /// ArgumentOutOfRangeException; timer advancement remains caller-owned.</remarks>
    public static MapSpriteId SpriteId(int frame) => frame switch
    {
        0 => MapSpriteId.IndicatorFrame0,
        1 => MapSpriteId.IndicatorFrame1,
        2 => MapSpriteId.IndicatorFrame2,
        3 => MapSpriteId.IndicatorFrame1,
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };

    /// <summary>Calculates the alternating endpoint/midpoint dwell.</summary>
    /// <remarks>Independently reviewed for #1165: phases0/2 hold an endpoint for8
    /// ticks; phases1/3 traverse the middle image for4. The game's consumers advance
    /// before loading this delay and decrement on that same tick.</remarks>
    public static int FrameDelay(int frame)
    {
        if ((uint)frame >= FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        return 8 - 4 * (frame & 1);
    }
}
