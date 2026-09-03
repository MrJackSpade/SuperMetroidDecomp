namespace SuperMetroid.Core.Audio;

/// <summary>
/// Fixed SPC-RAM locations, protocol values, and timing constants used by Super Metroid's
/// uploaded sound driver. These are addresses in the game's own APU program, not host policy.
/// </summary>
internal static class SpcDriverData
{
    internal const int ApuRamSize = 0x10000;
    internal const int ChannelCount = 8;
    internal const int HostSampleRate = 48_000;
    internal const int VideoFramesPerSecond = 60;
    internal const int HostStereoFramesPerVideoFrame = HostSampleRate / VideoFramesPerSecond;
    internal const int DspCyclesPerDriverTick = 64;
    internal const int MaximumFastForwardTicks = 0x10000;
    internal const byte NoPortCommand = byte.MaxValue;
    internal const byte PauseMusicCommand = 0xf0;
    internal const byte ResumeMusicCommand = 0xf1;

    internal static class Ram
    {
        internal const int FirstWorkRegion = 0x0500;
        internal const int FirstWorkRegionLength = 0x1000;
        internal const int SmallWorkRegion = 0x0020;
        internal const int SmallWorkRegionLength = 0x000f;
        internal const int SoundPointerRegion = 0x00d0;
        internal const int SoundPointerRegionLength = 0x001f;
        internal const int SoundStateRegion = 0x0391;
        internal const int SoundStateRegionLength = 0x006f;
        internal const int SecondarySoundStateRegion = 0x0440;
        internal const int SecondarySoundStateRegionLength = 0x007f;
        internal const int DefaultMusicPointer = 0x581e;
        internal const int MusicTrackPointerTable = 0x5820; // magic-number-audit: allow(AudioId) - resident SPC music pointer table address
        internal const int InstrumentTable = 0x6c00;
        internal const int InstrumentRecordSize = 6;
    }

    internal static class Echo
    {
        internal const byte InitialDelay = 1;
        internal const byte WriteDisable = 0x20;
        internal const int BufferPageBias = 0x16;
        internal const int DelayToPages = 8;
        internal const int FirTapCount = 8;
    }

    internal static class Music
    {
        internal const byte InitialTempo = 0x10;
        internal const byte DefaultTrackTempo = 0x20;
        internal const byte DefaultMasterVolume = 0xc0;
        internal const byte DefaultChannelVolume = byte.MaxValue;
        internal const byte CenterPan = 10;
        internal const byte FirstEffect = 0xe0;
        internal const byte FirstPercussionNote = 0xca;
        internal const byte PercussionPlaybackNote = 0xa4;
        internal const byte TieNote = 0xc8;
        internal const byte RestNote = 0xc9;
        internal const byte PatternFastForwardOn = 0x80;
        internal const byte PatternFastForwardOff = 0x81;
        internal const byte PatternPointerHighByteMinimum = 1;
        internal const int TrackStartupTicks = 2;
    }

    internal static class SoundEffects
    {
        internal const int LibraryCount = 3;
        internal const int LibraryOneChannelCount = 4;
        internal const int OtherLibraryChannelCount = 2;
        internal const byte VoiceSearchStart = 9;
        internal const byte Active = byte.MaxValue;
    }
}
