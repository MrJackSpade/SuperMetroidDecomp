namespace SuperMetroid.Core.Audio;

/// <summary>
/// Fixed SPC-RAM locations, protocol values, and timing constants used by Super Metroid's
/// uploaded sound driver. These are addresses in the game's own APU program, not host policy.
/// </summary>
internal static class SpcDriverData
{
    /// <summary>Defines apu ram size.</summary>
    internal const int ApuRamSize = 0x10000;
    /// <summary>Defines channel count.</summary>
    internal const int ChannelCount = 8;
    /// <summary>Defines host sample rate.</summary>
    internal const int HostSampleRate = 48_000;
    /// <summary>Defines video frames per second.</summary>
    internal const int VideoFramesPerSecond = 60;
    /// <summary>Defines host stereo frames per video frame.</summary>
    internal const int HostStereoFramesPerVideoFrame = HostSampleRate / VideoFramesPerSecond;
    /// <summary>Defines dsp cycles per driver tick.</summary>
    internal const int DspCyclesPerDriverTick = 64;
    /// <summary>Defines maximum fast forward ticks.</summary>
    internal const int MaximumFastForwardTicks = 0x10000;

    /// <summary>$1E90: the second half of the $AA/$BB ready signal, left on output port 1 by an upload.</summary>
    internal const byte UploadReadyLibraryOnePort = 0xbb;

    /// <summary>Represents ram.</summary>
    internal static class Ram
    {
        /// <summary>Defines first work region.</summary>
        internal const int FirstWorkRegion = 0x0500;
        /// <summary>Defines first work region length.</summary>
        internal const int FirstWorkRegionLength = 0x1000;
        /// <summary>Defines small work region.</summary>
        internal const int SmallWorkRegion = 0x0020;
        /// <summary>Defines small work region length.</summary>
        internal const int SmallWorkRegionLength = 0x000f;
        /// <summary>Defines sound pointer region.</summary>
        internal const int SoundPointerRegion = 0x00d0;
        /// <summary>Defines sound pointer region length.</summary>
        internal const int SoundPointerRegionLength = 0x001f;
        /// <summary>Defines sound state region.</summary>
        internal const int SoundStateRegion = 0x0391;
        /// <summary>Defines sound state region length.</summary>
        internal const int SoundStateRegionLength = 0x006f;
        /// <summary>Defines secondary sound state region.</summary>
        internal const int SecondarySoundStateRegion = 0x0440;
        /// <summary>Defines secondary sound state region length.</summary>
        internal const int SecondarySoundStateRegionLength = 0x007f;
        /// <summary>Defines default music pointer.</summary>
        internal const int DefaultMusicPointer = 0x581e;
        /// <summary>Defines instrument table.</summary>
        internal const int InstrumentTable = 0x6c00;
        /// <summary>Defines instrument record size.</summary>
        internal const int InstrumentRecordSize = 6;
        /// <summary>Defines instrument table byte length.</summary>
        internal const int InstrumentTableByteLength = 0x0100;
        /// <summary>Defines instrument count.</summary>
        internal const int InstrumentCount = InstrumentTableByteLength / InstrumentRecordSize;
    }

    /// <summary>Represents echo.</summary>
    internal static class Echo
    {
        /// <summary>Defines initial delay.</summary>
        internal const byte InitialDelay = 1;
        /// <summary>Defines write disable.</summary>
        internal const byte WriteDisable = 0x20;
        /// <summary>Defines buffer page bias.</summary>
        internal const int BufferPageBias = 0x16;
        /// <summary>Defines delay to pages.</summary>
        internal const int DelayToPages = 8;
        /// <summary>Defines fir tap count.</summary>
        internal const int FirTapCount = 8;

        /// <summary>
        /// APU address `$1E32`, the first FIR coefficient consumed by music effect `$F7`.
        /// The native driver performs byte-addressed arithmetic from this location and does
        /// not constrain the preset operand to the four conventional filters. MUL YA /
        /// MOV X,A at SPC1A94..1A97 wraps the eight-byte offset to an eight-bit index,
        /// giving32 mutable RAM groups in1E32..1F31 rather than256 unwrapped groups.
        /// </summary>
        internal const ushort FirCoefficientTableAddress = 0x1e32;
    }

    /// <summary>Represents music.</summary>
    internal static class Music
    {
        /// <summary>The seven bits of a note byte or pitch whole byte that hold the note number; bit 7 is not part of it.</summary>
        internal const byte NoteNumberMask = 0x7f;
        /// <summary>Defines initial tempo.</summary>
        internal const byte InitialTempo = 0x10;
        /// <summary>Defines default track tempo.</summary>
        internal const byte DefaultTrackTempo = 0x20;
        /// <summary>Defines default master volume.</summary>
        internal const byte DefaultMasterVolume = 0xc0;
        /// <summary>Defines default channel volume.</summary>
        internal const byte DefaultChannelVolume = byte.MaxValue;
        /// <summary>Defines center pan.</summary>
        internal const byte CenterPan = 10;
        /// <summary>SPC1E1D, panningVolumeMultipliers; indexed SBC/ADC at1C55/1C5F
        /// use this base, including bounded reads into subsequent mutable RAM.</summary>
        internal const ushort PanVolumeTableAddress = 0x1e1d;
        /// <summary>SPC1C7A..1C7F loads1400h and subtracts the current8.8 pan bias
        /// to obtain the other speaker's bias with16-bit wrap.</summary>
        internal const ushort FullyLeftPan = 0x1400;
        /// <summary>Defines first effect.</summary>
        internal const byte FirstEffect = 0xe0;
        /// <summary>Defines command marker.</summary>
        internal const byte CommandMarker = 0x80;
        /// <summary>Defines first percussion note.</summary>
        internal const byte FirstPercussionNote = 0xca;
        /// <summary>Defines percussion playback note.</summary>
        internal const byte PercussionPlaybackNote = 0xa4;
        /// <summary>Defines tie note.</summary>
        internal const byte TieNote = 0xc8;
        /// <summary>Defines rest note.</summary>
        internal const byte RestNote = 0xc9;
        /// <summary>Defines pattern pointer high byte minimum.</summary>
        internal const byte PatternPointerHighByteMinimum = 1;
        /// <summary>Defines track startup ticks.</summary>
        internal const int TrackStartupTicks = 2;
    }

    /// <summary>Represents instruments.</summary>
    internal static class Instruments
    {
        /// <summary>High bit selecting a DSP noise rate instead of a BRR source number.</summary>
        internal const byte NoiseMarker = 0x80;
    }

    /// <summary>Represents sound effects.</summary>
    internal static class SoundEffects
    {
        /// <summary>Defines library count.</summary>
        internal const int LibraryCount = 3;
        /// <summary>Defines library one channel count.</summary>
        internal const int LibraryOneChannelCount = 4;
        /// <summary>Defines other library channel count.</summary>
        internal const int OtherLibraryChannelCount = 2;
        /// <summary>Defines voice search start.</summary>
        internal const byte VoiceSearchStart = 9;
        /// <summary>Defines active.</summary>
        internal const byte Active = byte.MaxValue;
    }
}

/// <summary>
/// Music-port control bytes. Every other byte the CPU writes to the music port is a track
/// number, which the driver starts unless it is already playing.
/// </summary>
internal enum MusicPortCommand : byte
{
    /// <summary>Keys off every music channel without changing the track.</summary>
    Pause = AudioRomData.Apu.PauseMusic,
    /// <summary>Resumes the paused track.</summary>
    Resume = AudioRomData.Apu.ResumeMusic,
    /// <summary>The driver's empty-port value, written after it consumes a command.</summary>
    None = byte.MaxValue,
}

/// <summary>
/// Control words in a top-level music track stream. A word with a nonzero high byte is a
/// phrase pointer, zero ends the track, and every other low value is a repeat count.
/// </summary>
internal enum MusicTrackControl : ushort
{
    /// <summary>$0080: process phrases without waiting for note timers.</summary>
    FastForwardOn = 0x80,
    /// <summary>$0081: return to ordinary timed processing.</summary>
    FastForwardOff = 0x81,
}

/// <summary>Validated decoding of top-level music track control words.</summary>
internal static class MusicTrackControls
{
    /// <summary>Distinguishes a control word from a phrase pointer, end marker or repeat count.</summary>
    internal static bool TryDecode(ushort record, out MusicTrackControl control)
    {
        if (!Enum.IsDefined((MusicTrackControl)record))
        {
            control = default;
            return false;
        }
        control = (MusicTrackControl)record;
        return true;
    }

    /// <summary>True when <paramref name="record"/> is exactly <paramref name="control"/>.</summary>
    internal static bool Is(ushort record, MusicTrackControl control) =>
        TryDecode(record, out MusicTrackControl decoded) && decoded == control;
}
