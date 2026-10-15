using SuperMetroid.Core.Frontend.Map;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Shared four-phase pulse for pause and file-select map markers at $82:B9FC.</summary>
internal static class PauseMapIndicatorAnimation
{
    /// <summary>$82:BA06 wraps at byte offset8: four word-indexed phases.</summary>
    public const int FrameCount = 4;

    /// <summary>Calculates the triangular sprite pulse at $82:BA2D, phase0..3.</summary>
    /// <remarks>Independently reviewed for #1165: start at5F, rise twice, then fall.
    /// The bounded triangle is2-abs(2-phase). Invalid indices preserve the former
    /// array's IndexOutOfRangeException; timer advancement remains caller-owned.</remarks>
    public static MapSpriteId SpritemapId(int frame) => frame switch
    {
        0 => MapSpriteId.IndicatorFrame0,
        1 => MapSpriteId.IndicatorFrame1,
        2 => MapSpriteId.IndicatorFrame2,
        3 => MapSpriteId.IndicatorFrame1,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>Calculates the alternating endpoint/midpoint dwell at $82:BA25.</summary>
    /// <remarks>Independently reviewed for #1165: phases0/2 hold an endpoint for8
    /// ticks; phases1/3 traverse the middle image for4. Native consumers advance
    /// before loading this delay and decrement on that same tick.</remarks>
    public static int FrameDelay(int frame)
    {
        if ((uint)frame >= FrameCount) throw new IndexOutOfRangeException();
        return 8 - 4 * (frame & 1);
    }
}
