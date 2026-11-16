using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Plays one looping block animation (#1406, #1988): the one copy of the loop every block animation plays, a room
/// effect's overlay animation, the spikes, the Maridia sand and the Wrecked Ship treadmills alike. A loop that starts once
/// the area's boss is defeated waits for that first (<see cref="BlockAnimationStart"/>); then each pass counts down the
/// frame shown, and when its time is up shows the loop's next frame for that frame's duration, returning from the last
/// frame to the first.
/// </summary>
/// <remarks>
/// Lava, acid, rain, and spores do not show pictures of the room tileset's. Their effect starts one looping animation,
/// each with its own named frames and durations. Playing the whole loop matters: showing one convenient frame would make
/// the first still image look plausible while discarding the game's cadence and preventing replacement graphics from
/// following the same frames.
/// </remarks>
internal sealed class BlockAnimationPlayer
{
    /// <summary>
    /// The frame shown when the frame's time is next up, an index into the loop's <see cref="BlockAnimationLoop.Frames"/>:
    /// it only ever moves to the next frame, and from the last frame back to the first in the same pass.
    /// </summary>
    private int nextFrame;
    /// <summary>Remaining passes before the current frame gives way to the next; a loop starts at one, due on its first pass.</summary>
    private int frameHold;
    /// <summary>Whether the loop still waits for the area's boss to be defeated before its first frame.</summary>
    private bool awaitsAreaBoss;
    /// <summary>The loop played, or null while none is.</summary>
    private BlockAnimationLoop? loop;

    /// <summary>The loop played; only a playing player has one.</summary>
    /// <exception cref="InvalidOperationException">No loop is playing.</exception>
    public BlockAnimationLoop Loop => loop ?? throw new InvalidOperationException("No block animation is playing.");

    /// <summary>
    /// Starts playing <paramref name="played"/>, due on the next pass, or nothing when it is null (a room effect whose type
    /// shows no overlay animation).
    /// </summary>
    /// <exception cref="InvalidOperationException">This player already plays a loop: each room builds its own players.</exception>
    public void Play(BlockAnimationLoop? played)
    {
        if (loop is not null)
            throw new InvalidOperationException($"Block animation player already plays {loop.Animation}.");
        if (played is null)
            return;
        loop = played;
        nextFrame = 0;
        frameHold = 1;
        awaitsAreaBoss = played.Start == BlockAnimationStart.OnceAreaBossIsDefeated;
    }

    /// <summary>
    /// Runs one pass of a loop that starts immediately; a player with no loop does nothing.
    /// </summary>
    /// <exception cref="InvalidOperationException">The loop waits for the area's boss, which this pass does not say.</exception>
    public void Step(ArtworkFrameSelections frames)
    {
        if (awaitsAreaBoss)
            throw new InvalidOperationException($"Block animation {Loop.Animation} waits for the area's boss; its pass needs the boss's state.");
        PlayPass(frames);
    }

    /// <summary>
    /// Runs one pass: a loop still waiting for the area's boss starts on the first pass <paramref name="areaBossDefeated"/>
    /// holds, and shows its first frame on that pass, never testing the boss again; a player with no loop does nothing.
    /// </summary>
    public void Step(bool areaBossDefeated, ArtworkFrameSelections frames)
    {
        if (awaitsAreaBoss)
        {
            Ensure.NotNull(frames);
            if (!areaBossDefeated)
                return;
            awaitsAreaBoss = false;
        }
        PlayPass(frames);
    }

    /// <summary>
    /// Counts the frame shown down by one pass. When its time is up, the next frame is shown for its duration, selected in
    /// <paramref name="frames"/> at this update, as this update's frame boundary shows it; the loop returns from the last
    /// frame to the first.
    /// </summary>
    private void PlayPass(ArtworkFrameSelections frames)
    {
        Ensure.NotNull(frames);
        if (loop is null)
            return;

        if (--frameHold != 0)
            return;

        IReadOnlyList<TimedArtworkFrame> loopFrames = loop.Frames;
        TimedArtworkFrame shown = loopFrames[nextFrame];
        frameHold = shown.Duration;
        // After the last frame the loop returns to the first.
        nextFrame = nextFrame + 1 < loopFrames.Count ? nextFrame + 1 : 0;
        frames.Select(shown.Frame);
    }
}
