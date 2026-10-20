using SuperMetroid.Core.Runtime.Attract;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Where the attract demos are: the set and the scene the title plays next, the loading and playback countdowns of the
/// scene running, and how it ended.
/// </summary>
internal sealed class AttractDemoSchedule
{
    /// <summary>The demo set the title plays.</summary>
    internal int Set { get; private set; }

    /// <summary>The next scene within <see cref="Set"/>.</summary>
    internal int Scene { get; set; }

    /// <summary>The scripted input frames left in the scene playing.</summary>
    internal int FramesRemaining { get; set; }

    /// <summary>The still-frame hold left after the scene's scripted input.</summary>
    internal int HoldFramesRemaining { get; private set; }

    /// <summary>The blank enemy-transfer waits left before the scene becomes visible; negative before its runtime exists.</summary>
    private int loadFramesRemaining = -1;

    /// <summary>Whether live controller input cancelled the scene.</summary>
    internal bool Cancelled { get; private set; }

    /// <summary>Whether another scene follows before returning to the title.</summary>
    internal bool HasNextScene { get; private set; }

    /// <summary>Whether the next scene's runtime has yet to be created.</summary>
    internal bool AwaitsScene => loadFramesRemaining < 0;

    /// <summary>Whether the scene's scripted input has ended and its final image is holding.</summary>
    internal bool Holding => HoldFramesRemaining != 0;

    /// <summary>A soft reset returns to the first scene of the first set.</summary>
    internal void Reset()
    {
        Set = 0;
        Scene = 0;
        loadFramesRemaining = -1;
    }

    /// <summary>The title requests a demo: its set starts again from its first scene.</summary>
    internal void RequestFromTitle()
    {
        Scene = 0;
        loadFramesRemaining = -1;
    }

    /// <summary>The scene the next demo plays, or null at the end of the set.</summary>
    internal AttractDemoScene? NextScene => StockAttractDemoScenes.Get(Set, Scene);

    /// <summary>Starts <paramref name="scene"/>, the next scene, with its scripted input and enemy-transfer waits ahead.</summary>
    internal void Begin(AttractDemoScene scene)
    {
        Ensure.NotNull(scene);
        FramesRemaining = scene.Duration;
        Scene++;
        HoldFramesRemaining = 0;
        Cancelled = false;
        loadFramesRemaining = AttractDemoRomData.EnemyTransferFrames;
    }

    /// <summary>Counts one enemy-transfer wait; returns whether it was the last.</summary>
    internal bool CountLoadFrame() => --loadFramesRemaining == 0;

    /// <summary>Counts one held frame; returns whether the hold ended.</summary>
    internal bool CountHoldFrame() => --HoldFramesRemaining == 0;

    /// <summary>Counts one scripted frame; once the script runs out, the final image starts holding.</summary>
    internal void CountPlaybackFrame()
    {
        if (--FramesRemaining <= 0)
            HoldFramesRemaining = AttractDemoRomData.FinalImageHoldFrames;
    }

    /// <summary>Records how the scene ended.</summary>
    internal void Finish(bool cancelled) => Cancelled = cancelled;

    /// <summary>
    /// After a scene: an uncancelled demo continues with the set's next scene, and once the set is exhausted the next of
    /// <paramref name="setCount"/> sets starts from its first scene.
    /// </summary>
    internal void AdvanceAfterScene(int setCount)
    {
        HasNextScene = false;
        if (Cancelled)
            return;
        HasNextScene = NextScene is not null;
        if (HasNextScene)
            return;
        Set = (Set + 1) % setCount;
        Scene = 0;
    }

    /// <summary>The next scene of the set loads afresh.</summary>
    internal void LoadNextScene() => loadFramesRemaining = -1;
}
