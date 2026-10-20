using SuperMetroid.Core.Enemies.Elevator;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// State $09's door function before state $0A: every door hit installs <c>$82:E17D</c>, and a downward elevator replaces it
/// with the <c>$82:E19F</c> delay before destination loading resumes.
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
