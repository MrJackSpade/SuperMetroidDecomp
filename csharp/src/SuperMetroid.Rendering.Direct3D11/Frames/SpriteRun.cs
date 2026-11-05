namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>A run of the frame's packed sprite draws (#1514): the draws one sprite layer shows, or all of the frame's.</summary>
/// <param name="First">The run's first draw, counted frontmost first among the packed draws.</param>
/// <param name="Count">How many draws the run holds.</param>
internal readonly record struct SpriteRun(int First, int Count);
