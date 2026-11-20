using SuperMetroid.Core.Enemies.Elevator;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The door transition's wait before destination loading: an ordinary door or an upward elevator goes straight on, and a
/// downward elevator waits out a delay before destination loading resumes.
/// </summary>
internal sealed class DownwardsElevatorWait
{
    /// <summary>
    /// The delay loading stays paused for, over once its frames have passed: a delay of <c>n</c> pauses <c>n</c> frames and
    /// resumes loading on the frame after them (#2488).
    /// </summary>
    private UpdateCountdown delay = UpdateCountdown.Ended();

    /// <summary>Whether room loading is paused for the downward elevator: from its delay's start until loading resumes.</summary>
    internal bool Waiting { get; private set; }

    /// <summary>Starts the elevator's delay.</summary>
    internal void Begin()
    {
        delay = UpdateCountdown.For(ElevatorDoorDefinitions.DownwardsElevatorDelayFrames);
        Waiting = true;
    }

    /// <summary>Counts one frame of the delay the door's wait began; returns whether loading resumes.</summary>
    internal bool CountFrame()
    {
        if (delay.IsRunning)
        {
            delay.Count();
            return false;
        }
        Waiting = false;
        return true;
    }
}
