namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One scripted room PLM cue.</summary>
internal abstract record ScriptedRoomPlmCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private ScriptedRoomPlmCue()
    {
    }

    /// <summary>Installs a pre-instruction the slot runs before its timer on later passes.</summary>
    /// <param name="PreInstruction">The pre-instruction an install cue installs.</param>
    internal sealed record InstallPreInstruction(ScriptedRoomPlmPreInstruction? PreInstruction) : ScriptedRoomPlmCue;

    /// <summary>Queues a library-2 sound, at most three pending.</summary>
    /// <param name="SoundId">The library-2 sound a queue cue plays.</param>
    internal sealed record QueueSoundLibrary2Maximum3(int SoundId) : ScriptedRoomPlmCue;

    /// <summary>Queues a library-2 sound, at most six pending.</summary>
    /// <param name="SoundId">The library-2 sound a queue cue plays.</param>
    internal sealed record QueueSoundLibrary2Maximum6(int SoundId) : ScriptedRoomPlmCue;

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(ScriptedRoomPlmCueKind Kind) : ScriptedRoomPlmCue;
}
