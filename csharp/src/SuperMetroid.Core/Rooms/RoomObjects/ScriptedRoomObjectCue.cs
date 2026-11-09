namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One scripted room PLM cue.</summary>
internal abstract record ScriptedRoomObjectCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private ScriptedRoomObjectCue()
    {
    }

    /// <summary>Installs a pre-instruction the slot runs before its timer on later passes.</summary>
    /// <param name="FrameBehavior">The pre-instruction an install cue installs.</param>
    internal sealed record InstallFrameBehavior(ScriptedRoomObjectFrameBehavior? FrameBehavior) : ScriptedRoomObjectCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(ScriptedRoomObjectCueKind Kind) : ScriptedRoomObjectCue;
}
