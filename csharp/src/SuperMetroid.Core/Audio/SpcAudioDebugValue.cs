namespace SuperMetroid.Core.Audio;

/// <summary>Temporary state selectors used only while proving managed/native audio parity.</summary>
public enum SpcAudioDebugValue
{
    TimerCycles,
    StartupCounter,
    MusicPointer,
    FastForward,
    MainTempoAccumulator,
    Tempo,
    BlockCount,
    KeyOn,
    KeyOff,
    CurrentChannelBit,
    ChannelOnMask,
    PatternPointer,
    NoteTicksLeft,
    NoteLength,
    InstrumentId,
    SubroutineLoops,
    SavedPatternPointer,
    PatternStartPointer,
    NoteGateOff,
    CutKey,
}
