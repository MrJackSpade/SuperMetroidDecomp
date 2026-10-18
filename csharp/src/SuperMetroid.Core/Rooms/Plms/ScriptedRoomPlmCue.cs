namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One scripted room PLM cue.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="SoundId">The library-2 sound a queue cue plays.</param>
/// <param name="PreInstruction">The pre-instruction an install cue installs.</param>
internal readonly record struct ScriptedRoomPlmCue(ScriptedRoomPlmCueKind Kind, int SoundId = 0, ScriptedRoomPlmPreInstruction? PreInstruction = null);
