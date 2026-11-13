using SuperMetroid.Core.Enemies.Elevator;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The door transition's phase before destination loading: every door hit installs an immediate finish, and a downward elevator replaces it
/// with a delay before destination loading resumes.
/// </summary>
internal sealed class DownwardsElevatorWait
{
    /// <summary>The frames left before destination loading resumes.</summary>
    private int delay;

    /// <summary>Whether room loading is paused for the downward elevator.</summary>
    internal bool Waiting { get; private set; }

    /// <summary>Starts the elevator's delay.</summary>
    internal void Begin()
    {
        delay = ElevatorDoorDefinitions.DownwardsElevatorDelayFrames;
        Waiting = true;
    }

    /// <summary>Counts one frame of the delay; returns whether loading resumes.</summary>
    internal bool CountFrame()
    {
        if (--delay >= 0)
            return false;
        Waiting = false;
        return true;
    }
}
