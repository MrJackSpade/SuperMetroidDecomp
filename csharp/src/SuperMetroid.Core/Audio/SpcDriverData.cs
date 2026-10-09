namespace SuperMetroid.Core.Audio;

/// <summary>
/// Fixed SPC-RAM locations, protocol values, and timing constants used by Super Metroid's
/// uploaded sound driver. These are addresses in the game's own APU program, not host policy.
/// </summary>
internal static class SpcDriverData
{
    /// <summary>Addressable byte count of the SPC's 64-KiB RAM.</summary>
    internal const int ApuRamSize = 0x10000;
    /// <summary>Number of music voices managed by the resident driver.</summary>
    internal const int ChannelCount = 8;
    /// <summary>Host synthesis rate used for rendered PCM.</summary>
    internal const int HostSampleRate = 48_000;
    /// <summary>Emulation updates per second used to schedule host audio output.</summary>
    internal const int VideoFramesPerSecond = 60;
    /// <summary>Stereo output frames synthesized for one host emulation update.</summary>
    internal const int HostStereoFramesPerVideoFrame = HostSampleRate / VideoFramesPerSecond;
    /// <summary>DSP clock cycles in one normal music-driver tick.</summary>
    internal const int DspCyclesPerDriverTick = 64;
    /// <summary>Maximum tick count accepted by the bounded fast-forward loop.</summary>
    internal const int MaximumFastForwardTicks = 0x10000;
    /// <summary>Sentinel for no pending write from an SPC command handler to an APU port.</summary>
    internal const byte NoPortCommand = byte.MaxValue;

    /// <summary>$1E90: the second half of the $AA/$BB ready signal, left on output port 1 by an upload.</summary>
    internal const byte UploadReadyLibraryOnePort = 0xbb;
    /// <summary>SPC command byte that pauses music sequence advancement.</summary>
    internal const byte PauseMusicCommand = AudioRomData.Apu.PauseMusic;
    /// <summary>SPC command byte that resumes a paused music sequence.</summary>
    internal const byte ResumeMusicCommand = AudioRomData.Apu.ResumeMusic;

    /// <summary>Fixed SPC-RAM work areas used by the resident program.</summary>
    internal static class Ram
    {
        /// <summary>Base of the primary shared work region.</summary>
        internal const int FirstWorkRegion = 0x0500;
        /// <summary>Byte length of the primary shared work region.</summary>
        internal const int FirstWorkRegionLength = 0x1000;
        /// <summary>Base of the small direct-page work region.</summary>
        internal const int SmallWorkRegion = 0x0020;
        /// <summary>Byte length of the small direct-page work region.</summary>
        internal const int SmallWorkRegionLength = 0x000f;
        /// <summary>Base of the resident sound-program pointer table.</summary>
        internal const int SoundPointerRegion = 0x00d0;
        /// <summary>Byte length of the sound-program pointer table.</summary>
        internal const int SoundPointerRegionLength = 0x001f;
        /// <summary>Base of the primary SFX channel-state region.</summary>
        internal const int SoundStateRegion = 0x0391;
        /// <summary>Byte length of the primary SFX channel-state region.</summary>
        internal const int SoundStateRegionLength = 0x006f;
        /// <summary>Base of the secondary SFX channel-state region.</summary>
        internal const int SecondarySoundStateRegion = 0x0440;
        /// <summary>Byte length of the secondary SFX channel-state region.</summary>
        internal const int SecondarySoundStateRegionLength = 0x007f;
        /// <summary>Default resident music pattern pointer loaded during driver initialization.</summary>
        internal const int DefaultMusicPointer = 0x581e;
        /// <summary>Base of the six-byte instrument-definition table.</summary>
        internal const int InstrumentTable = 0x6c00;
        /// <summary>Bytes in one instrument definition.</summary>
        internal const int InstrumentRecordSize = 6;
        /// <summary>Byte capacity reserved for the instrument table.</summary>
        internal const int InstrumentTableByteLength = 0x0100;
        /// <summary>Number of complete instrument records fitting in the table capacity.</summary>
        internal const int InstrumentCount = InstrumentTableByteLength / InstrumentRecordSize;
    }

    /// <summary>SPC echo engine defaults and workspace dimensions.</summary>
    internal static class Echo
    {
        /// <summary>Initial echo delay in native 16-ms units.</summary>
        internal const byte InitialDelay = 1;
        /// <summary>FLG bit used to suppress writes into the echo ring.</summary>
        internal const byte WriteDisable = 0x20;
        /// <summary>SPC page offset reserved before the echo buffer for driver data.</summary>
        internal const int BufferPageBias = 0x16;
        /// <summary>SPC pages allocated per echo-delay unit.</summary>
        internal const int DelayToPages = 8;
        /// <summary>Number of signed coefficients in one DSP FIR pass.</summary>
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

    /// <summary>SPC music-driver defaults, opcodes, and resident lookup-table addresses.</summary>
    internal static class Music
    {
        /// <summary>Tempo accumulator value used while initializing the driver.</summary>
        internal const byte InitialTempo = 0x10;
        /// <summary>Tempo installed when a track does not override its default.</summary>
        internal const byte DefaultTrackTempo = 0x20;
        /// <summary>Initial global music volume.</summary>
        internal const byte DefaultMasterVolume = 0xc0;
        /// <summary>Initial per-channel music volume.</summary>
        internal const byte DefaultChannelVolume = byte.MaxValue;
        /// <summary>Pan table index corresponding to the center position.</summary>
        internal const byte CenterPan = 10;
        /// <summary>SPC1E1D, panningVolumeMultipliers; indexed SBC/ADC at1C55/1C5F
        /// use this base, including bounded reads into subsequent mutable RAM.</summary>
        internal const ushort PanVolumeTableAddress = 0x1e1d;
        /// <summary>SPC1C7A..1C7F loads1400h and subtracts the current8.8 pan bias
        /// to obtain the other speaker's bias with16-bit wrap.</summary>
        internal const ushort FullyLeftPan = 0x1400;
        /// <summary>Lowest opcode byte reserved for music effect commands.</summary>
        internal const byte FirstEffect = 0xe0;
        /// <summary>Pattern byte with the command-marker bit set.</summary>
        internal const byte CommandMarker = 0x80;
        /// <summary>First encoded note value reserved for percussion triggers.</summary>
        internal const byte FirstPercussionNote = 0xca;
        /// <summary>Note value emitted while dispatching a percussion instrument.</summary>
        internal const byte PercussionPlaybackNote = 0xa4;
        /// <summary>Encoded note value that ties the current envelope into the next note.</summary>
        internal const byte TieNote = 0xc8;
        /// <summary>Encoded note value representing a timed rest.</summary>
        internal const byte RestNote = 0xc9;
        /// <summary>SPC pattern command that enables accelerated playback.</summary>
        internal const byte PatternFastForwardOn = 0x80;
        /// <summary>SPC pattern command that disables accelerated playback.</summary>
        internal const byte PatternFastForwardOff = 0x81;
        /// <summary>Minimum high byte accepted for a track pattern pointer.</summary>
        internal const byte PatternPointerHighByteMinimum = 1;
        /// <summary>Driver ticks consumed during track startup before normal sequencing.</summary>
        internal const int TrackStartupTicks = 2;
    }

    /// <summary>Instrument-record interpretation shared by upload and music setup.</summary>
    internal static class Instruments
    {
        /// <summary>High bit selecting a DSP noise rate instead of a BRR source number.</summary>
        internal const byte NoiseMarker = 0x80;
    }

    /// <summary>Resident sound-library counts and voice-allocation sentinels.</summary>
    internal static class SoundEffects
    {
        /// <summary>Number of independently addressed SFX libraries.</summary>
        internal const int LibraryCount = 3;
        /// <summary>Voice slots assigned to sound library one.</summary>
        internal const int LibraryOneChannelCount = 4;
        /// <summary>Voice slots assigned to each of sound libraries two and three.</summary>
        internal const int OtherLibraryChannelCount = 2;
        /// <summary>Initial voice-search cursor used when selecting an available SFX voice.</summary>
        internal const byte VoiceSearchStart = 9;
        /// <summary>Byte value marking a voice as allocated to an active sound effect.</summary>
        internal const byte Active = byte.MaxValue;
    }
}
