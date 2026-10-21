namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>The item selector's animation frame and timer.</summary>
/// <param name="Frame">The frame.</param>
/// <param name="Timer">The timer.</param>
internal readonly record struct SelectorAnimation(int Frame, int Timer);
