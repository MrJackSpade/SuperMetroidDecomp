namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>The selection missile's four-frame cycle, shared by file select, options and game over.</summary>
internal sealed class MenuMissileAnimation
{
    /// <summary>Updates remaining before the missile advances a frame.</summary>
    internal int Timer { get; private set; } = 1;

    /// <summary>The current frame of the four-frame cycle.</summary>
    internal int Frame { get; private set; }

    /// <summary>
    /// Counts one update down and, when the frame's duration expires, advances to the next frame for
    /// <paramref name="frameDuration"/> updates, or the native duration when no presentation supplies one.
    /// </summary>
    internal void Step(int? frameDuration)
    {
        if (--Timer != 0)
            return;
        Frame = (Frame + 1) % MenuMissileAnimationDefinitions.FrameCount;
        Timer = frameDuration ?? MenuMissileAnimationDefinitions.FrameDuration;
    }
}
