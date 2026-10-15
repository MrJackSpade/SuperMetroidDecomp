namespace SuperMetroid.Core.Audio;

/// <summary>
/// Fixed SPC-RAM locations, protocol values, and timing constants used by Super Metroid's
/// uploaded sound driver. These are addresses in the game's own APU program, not host policy.
/// </summary>
internal static class SpcDriverDefinitions
{
    /// <summary>Complete sixteen-bit APU address range available to the uploaded driver.</summary>
    internal const int ApuRamSize = 0x10000;
    /// <summary>Eight S-DSP voices, shared by music and the three sound-effect libraries.</summary>
    internal const int ChannelCount = 8;
    /// <summary>Host output rate used when resampling the native DSP stream.</summary>
    internal const int HostSampleRate = 48_000;
    /// <summary>NTSC game-frame rate used to size one host audio packet.</summary>
    internal const int VideoFramesPerSecond = 60;
    /// <summary>Stereo sample frames generated for each 60 Hz game frame.</summary>
    internal const int HostStereoFramesPerVideoFrame = HostSampleRate / VideoFramesPerSecond;
    /// <summary>DSP cycle advance associated with one uploaded-driver processing tick.</summary>
    internal const int DspCyclesPerDriverTick = 64;

    /// <summary>
    /// Added to the driver's eight-bit sound-effect timer on every tick; its carry polls the
    /// three sound-effect ports, so they are serviced every eighth tick.
    /// </summary>
    internal const byte SoundEffectTimerStep = 0x20; // magic-number-audit: allow(AudioId) - SPC timer scale, not a sequence ID
    /// <summary>Bound on immediate track processing when the driver follows fast-forward control words.</summary>
    internal const int MaximumFastForwardTicks = 0x10000;

    /// <summary>$1E90: the second half of the $AA/$BB ready signal, left on output port 1 by an upload.</summary>
    internal const byte UploadReadyLibraryOnePort = 0xbb;

    /// <summary>Native SPC-RAM regions used for driver work state, sound pointers, and instrument records.</summary>
    internal static class Ram
    {
        /// <summary>Start of the first uploaded-driver work area in SPC RAM.</summary>
        internal const int FirstWorkRegion = 0x0500;
        /// <summary>Byte extent reserved for the first driver work area.</summary>
        internal const int FirstWorkRegionLength = 0x1000;
        /// <summary>Small low-RAM work area preceding the sound pointer region.</summary>
        internal const int SmallWorkRegion = 0x0020;
        /// <summary>Byte extent of that low-RAM scratch area.</summary>
        internal const int SmallWorkRegionLength = 0x000f;
        /// <summary>Low-RAM base of the sound pointer state.</summary>
        internal const int SoundPointerRegion = 0x00d0;
        /// <summary>Byte extent copied or initialized with the sound pointer state.</summary>
        internal const int SoundPointerRegionLength = 0x001f;
        /// <summary>First sound effect state block in SPC RAM.</summary>
        internal const int SoundStateRegion = 0x0391;
        /// <summary>Byte extent of the first sound effect state block.</summary>
        internal const int SoundStateRegionLength = 0x006f;
        /// <summary>Second sound effect state block in SPC RAM.</summary>
        internal const int SecondarySoundStateRegion = 0x0440;
        /// <summary>Byte extent of the second sound effect state block.</summary>
        internal const int SecondarySoundStateRegionLength = 0x007f;
        /// <summary>Track pointer loaded when the uploaded driver begins default music playback.</summary>
        internal const int DefaultMusicPointer = 0x581e;
        /// <summary>Native six-byte instrument record table in uploaded APU memory.</summary>
        internal const int InstrumentTable = 0x6c00;
        /// <summary>Byte stride between successive instrument source, envelope, and gain records.</summary>
        internal const int InstrumentRecordSize = 6;
        /// <summary>Bounded APU memory span reserved for the instrument records.</summary>
        internal const int InstrumentTableByteLength = 0x0100;
        /// <summary>Number of whole six-byte instrument records fitting the reserved table span.</summary>
        internal const int InstrumentCount = InstrumentTableByteLength / InstrumentRecordSize;
    }

    /// <summary>Native echo buffer sizing, write control, and FIR filter layout.</summary>
    internal static class Echo
    {
        /// <summary>Initial echo delay register value before music effects reconfigure the buffer.</summary>
        internal const byte InitialDelay = 1;
        /// <summary>FLG bit that suppresses writes into the echo buffer.</summary>
        internal const byte WriteDisable = 0x20;
        /// <summary>Native page bias used when placing the echo buffer after driver RAM.</summary>
        internal const int BufferPageBias = 0x16;
        /// <summary>Converts one EDL delay step into eight 256-byte APU pages.</summary>
        internal const int DelayToPages = 8;
        /// <summary>Eight signed coefficients in the S-DSP echo FIR filter.</summary>
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

    /// <summary>Track stream markers, startup timing, and default mixer values.</summary>
    internal static class Music
    {
        /// <summary>The seven bits of a note byte or pitch whole byte that hold the note number; bit 7 is not part of it.</summary>
        internal const byte NoteNumberMask = 0x7f;
        /// <summary>Driver tempo word initialized before a track's own tempo effect runs.</summary>
        internal const byte InitialTempo = 0x10;
        /// <summary>Per-track tempo value when a new track has no explicit override yet.</summary>
        internal const byte DefaultTrackTempo = 0x20;
        /// <summary>Initial master volume before track effects change it.</summary>
        internal const byte DefaultMasterVolume = 0xc0;
        /// <summary>Full default level for one music channel.</summary>
        internal const byte DefaultChannelVolume = byte.MaxValue;
        /// <summary>Native pan midpoint in the driver's indexed volume multiplier table.</summary>
        internal const byte CenterPan = 10;
        /// <summary>SPC1E1D, panningVolumeMultipliers; indexed SBC/ADC at1C55/1C5F
        /// use this base, including bounded reads into subsequent mutable RAM.</summary>
        internal const ushort PanVolumeTableAddress = 0x1e1d;
        /// <summary>SPC1C7A..1C7F loads1400h and subtracts the current8.8 pan bias
        /// to obtain the other speaker's bias with16-bit wrap.</summary>
        internal const ushort FullyLeftPan = 0x1400;
        /// <summary>First note-stream byte reserved for a music effect command.</summary>
        internal const byte FirstEffect = 0xe0;
        /// <summary>High bit distinguishing note/control byte classes in the music stream.</summary>
        internal const byte CommandMarker = 0x80;
        /// <summary>First special note byte selecting a percussion source.</summary>
        internal const byte FirstPercussionNote = 0xca;
        /// <summary>Native note number substituted when playing a percussion instrument.</summary>
        internal const byte PercussionPlaybackNote = 0xa4;
        /// <summary>Extends the current note without retriggering its sample.</summary>
        internal const byte TieNote = 0xc8;
        /// <summary>Consumes note time while leaving the channel silent.</summary>
        internal const byte RestNote = 0xc9;
        /// <summary>Distinguishes a phrase pointer from low-valued repeat/control words.</summary>
        internal const byte PatternPointerHighByteMinimum = 1;
        /// <summary>Driver ticks spent initializing a selected track before its pattern runs.</summary>
        internal const int TrackStartupTicks = 2;
        /// <summary>
        /// The global top-level repeat counter reloads its record's count on the decrement after any
        /// value above this, and after zero; $01-$80 count down instead. Values in the reloading
        /// class therefore behave identically.
        /// </summary>
        internal const byte RepeatReloadClassMinimum = 0x80;
    }

    /// <summary>Instrument source-byte encoding shared by sampled and noise voices.</summary>
    internal static class Instruments
    {
        /// <summary>High bit selecting a DSP noise rate instead of a BRR source number.</summary>
        internal const byte NoiseMarker = 0x80;
        /// <summary>An instrument byte with this bit selects a percussion instrument, offset by the native bias and the track's percussion base.</summary>
        internal const byte PercussionMarker = 0x80;
        /// <summary>Added with the track's percussion base to a percussion instrument byte to index the instrument table.</summary>
        internal const int PercussionBias = 54;
    }

    /// <summary>Three native sound-effect libraries and their voice allocation limits.</summary>
    internal static class SoundEffects
    {
        /// <summary>Library one plus the two smaller effect libraries.</summary>
        internal const int LibraryCount = 3;
        /// <summary>Four simultaneous effect channels allocated to library one.</summary>
        internal const int LibraryOneChannelCount = 4;
        /// <summary>Two simultaneous channels in each remaining effect library.</summary>
        internal const int OtherLibraryChannelCount = 2;
        /// <summary>Starting native voice-search value used while finding an effect channel.</summary>
        internal const byte VoiceSearchStart = 9;
        /// <summary>All bits set denotes an active sound-effect slot.</summary>
        internal const byte Active = byte.MaxValue;
        /// <summary>A note-packet note byte of $F6 keeps the channel's previous note instead of setting one.</summary>
        internal const byte RetainedNote = 0xf6;
        /// <summary>Library 3's low-health mode value, in which it rejects every request except the clearing one.</summary>
        internal const byte LowHealthMode = 2;
    }
}
