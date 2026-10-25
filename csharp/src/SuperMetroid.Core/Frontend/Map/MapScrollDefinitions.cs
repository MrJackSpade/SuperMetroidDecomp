namespace SuperMetroid.Core.Frontend.Map;

/// <summary>The pause and file-select maps' scroll step: one eight-pixel map cell per eight-update step.</summary>
public static class MapScrollDefinitions
{
    /// <summary>The update of a scroll step on which the map moves its one cell.</summary>
    public const int PulseTick = 4;

    /// <summary>Updates in one scroll step; the step completes, and the next may begin, on this update.</summary>
    public const int StepTicks = 8;

    /// <summary>Pixels one scroll step moves the map: one eight-pixel map cell.</summary>
    public const int StepPixels = 8;
}
