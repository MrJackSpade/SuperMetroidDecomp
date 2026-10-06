namespace SuperMetroid.Core.Frontend;

/// <summary>Constants of the state-$09 door functions for elevator pseudo-doors.</summary>
public static class ElevatorDoorDefinitions
{
    /// <summary>
    /// <c>$82:E18E</c>: frames <c>DoorTransitionFunction_Wait48FramesForDownElevator</c>
    /// ($82:E19F) counts before state $0A (<c>$0030*!FPS</c>, NTSC).
    /// </summary>
    public const int DownwardsElevatorDelayFrames = 0x30;
}
