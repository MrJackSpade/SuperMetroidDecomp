namespace SuperMetroid.Core.Game;

/// <summary>Bank-$A7 rage-wave producer operands, independent of the bank-$86 motion program.</summary>
internal static class PhantoonRageRomData
{
    /// <summary>$A7:D8BB: first direction of the seven-flame even wave; descends through zero.</summary>
    internal const int EvenWaveFirstDirection = 6;
    /// <summary>$A7:D8D0: first direction of the eight-flame odd wave.</summary>
    internal const int OddWaveFirstDirection = 15;
    /// <summary>$A7:D8E1: inclusive final odd-wave direction after DEY/CPY/BPL.</summary>
    internal const int OddWaveLastDirection = 8;
    /// <summary>$A7:D8C0/$D8D5: parameter high byte selects the enraged flame initializer.</summary>
    internal const ushort FlameParameter = 0x0200;
    /// <summary>$A7:D8F4: round count before closing the eye and fading out.</summary>
    internal const int WaveCount = 8;
    /// <summary>$A7:D8F9: AI calls separating successive waves.</summary>
    internal const ushort WaveInterval = 128;
    /// <summary>$A7:D8E6: library-three sound emitted once after each complete wave.</summary>
    internal static SoundEffectId WaveSound => SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x29);
    /// <summary>$A7:D8E9: QueueSound_Lib3_Max6 handoff capacity.</summary>
    internal const byte WaveSoundQueueCapacity = 6;
}
