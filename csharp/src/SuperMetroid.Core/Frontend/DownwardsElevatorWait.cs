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
    /// resumes loading on the frame after them (#2488); none while loading is not paused.
    /// </summary>
    private UpdateCountdown? delay;

    /// <summary>Whether room loading is paused for the downward elevator.</summary>
    internal bool Waiting => delay is not null;

    /// <summary>Starts the elevator's delay.</summary>
    internal void Begin() => delay = UpdateCountdown.For(ElevatorDoorDefinitions.DownwardsElevatorDelayFrames);

    /// <summary>Counts one frame of the delay; returns whether loading resumes.</summary>
    /// <exception cref="InvalidOperationException">Loading is not paused.</exception>
    internal bool CountFrame()
    {
        UpdateCountdown paused = delay ?? throw new InvalidOperationException("The downward elevator's delay is counted while loading is not paused.");
        if (paused.IsRunning)
        {
            paused.Count();
            return false;
        }
        delay = null;
        return true;
    }
}
