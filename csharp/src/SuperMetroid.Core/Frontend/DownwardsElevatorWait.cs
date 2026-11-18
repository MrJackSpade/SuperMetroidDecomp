using SuperMetroid.Core.Enemies.Elevator;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The door transition's wait before destination loading: an ordinary door or an upward elevator goes straight on, and a
/// downward elevator waits out a delay before destination loading resumes.
/// </summary>
internal sealed class DownwardsElevatorWait
{
    /// <summary>
    /// The frames loading still stays paused, counted down to none: a wait of <c>n</c> pauses <c>n</c> frames and resumes
    /// loading on the frame after them (#2488).
    /// </summary>
    private int framesLeft;

    /// <summary>Whether room loading is paused for the downward elevator.</summary>
    internal bool Waiting { get; private set; }

    /// <summary>Starts the elevator's delay.</summary>
    internal void Begin()
    {
        framesLeft = ElevatorDoorDefinitions.DownwardsElevatorDelayFrames;
        Waiting = true;
    }

    /// <summary>Counts one frame of the delay; returns whether loading resumes.</summary>
    internal bool CountFrame()
    {
        if (framesLeft > 0)
        {
            framesLeft--;
            return false;
        }
        Waiting = false;
        return true;
    }
}
