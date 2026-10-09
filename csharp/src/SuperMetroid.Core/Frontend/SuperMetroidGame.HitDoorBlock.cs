namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    // State $09's door function before state $0A. Every door hit installs $82:E17D; a
    // downward elevator replaces it with the $82:E19F delay.
    /// <summary>Tracks whether this door-hit dispatch is continuing the downward-elevator delay.</summary>
    private bool waitingForDownwardsElevator;

    /// <summary>Remaining countdown ticks before the downward-elevator door dispatch completes.</summary>
    private int downwardsElevatorDelayTimer;

    /// <summary>
    /// Runs this dispatch's state-$09 door function. Returns true when it sets carry, so
    /// state $0A runs in the same dispatch.
    /// </summary>
    private bool StepHitDoorBlockFunction(ushort controllerInput)
    {
        if (!waitingForDownwardsElevator)
        {
            // $82:E17D: an ordinary door finishes at once.
            if (runtime!.Enemies.ElevatorFlags == 0)
                return true;
            // Samus command 0 locks Samus; an upward elevator then finishes at once.
            runtime.Samus!.InputLocked = true;
            if ((short)runtime.Enemies.ElevatorDirection < 0)
                return true;
            downwardsElevatorDelayTimer = ElevatorDoorDefinitions.DownwardsElevatorDelayFrames;
            waitingForDownwardsElevator = true;
        }

        // $82:E19F. The elevator gate is still clear, so the ride continues behind it.
        if (--downwardsElevatorDelayTimer < 0)
        {
            waitingForDownwardsElevator = false;
            return true;
        }
        runtime!.RunDoorSoundWaitFrame(controllerInput);
        PublishGameplay(runtime);
        return false;
    }
}
