using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// How long a block animation's loop shows each of its frames (#2604), measured by playing the loop as the game plays it
/// (<see cref="BlockAnimationLoops.Start"/>): each pass of the loop counts the frame shown down, and the pass that shows
/// the next frame ends it.
/// </summary>
internal static class BlockAnimationLoopPasses
{
    /// <summary>
    /// The passes each frame of <paramref name="animation"/>'s loop is shown for, in the order the loop shows them
    /// (<see cref="BlockAnimationLoops.Frames"/>), over one whole turn of the loop from its start.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="animation"/> is undefined or has no loop.</exception>
    internal static IReadOnlyList<int> Of(BlockAnimation animation)
    {
        int frames = BlockAnimationLoops.Frames(animation).Count;
        var loop = BlockAnimationLoops.Start(animation);
        var passes = new List<int>(frames);
        // A loop starts due, so its first pass shows its first frame; each later frame's showing ends the one before it.
        if (loop.StepToNewFrame() is null)
            throw new InvalidOperationException($"The {animation} loop shows nothing on its first pass.");
        int shown = 1;
        while (passes.Count < frames)
        {
            if (loop.StepToNewFrame() is null)
            {
                shown++;
                continue;
            }
            passes.Add(shown);
            shown = 1;
        }
        return passes;
    }
}
