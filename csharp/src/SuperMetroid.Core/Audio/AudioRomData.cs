namespace SuperMetroid.Core.Audio;

/// <summary>Verified cartridge addresses and fixed engine values for SNES audio dispatch.</summary>
public static class AudioRomData
{
    public static class Assets
    {
        public const int InitialAudioBank = 0xcf8000;
        public const int MusicPointerTable = 0x8fe7e1;
    }

    public static class MusicBanks
    {
        /// <summary>Title-screen music data selected by the retail frontend.</summary>
        public const byte Title = 0x03;
    }

    public static class MusicTracks
    {
        /// <summary>Title-screen track within <see cref="MusicBanks.Title"/>.</summary>
        public const byte Title = 0x05;
    }

    public static class Queues
    {
        public const int MusicCapacity = 8;
        public const int MusicIndexMask = MusicCapacity - 1;
        public const int SoundLibraryCount = 3;
        public const int SoundCapacity = 16;
        public const int SoundIndexMask = SoundCapacity - 1;
        public const byte MaximumSoundOccupancy = SoundCapacity - 1;
        public const ushort MinimumMusicDelayFrames = 8;
        public const int MusicAndSfxDowntimeFrames = 8;
        public const ushort PermanentItemFanfareFrames = 0x0168;
        public const byte PermanentItemTrack = 2;
    }

    /// <summary>Bit fields in the native music queue word; kept separate from queue policy.</summary>
    public static class MusicWireFormat
    {
        public const ushort DataCommandPrefix = 0xff00;
        public const ushort DataCommandKindMask = 0xff00;
        public const ushort UploadPathBit = 0x8000;
        public const ushort TrackMask = 0x007f;
        public const byte MaximumTrack = 0x7f;
        public const ushort ActiveTimerBit = 0x8000;
    }

    /// <summary>Physical shape and safety bounds of a native APU upload stream.</summary>
    public static class SpcUpload
    {
        public const int MaximumStreamBytes = 0x1_1000;
        public const int MaximumSnesAddress = 0x00ff_ffff;
        public const int LoRomUpperWindowBit = 0x8000;
        public const int AddressBankMask = 0xff;
        public const int AddressOffsetMask = 0xffff;
        public const int LastBankOffset = 0xffff;
        public const int FirstMappedBankOffset = 0x8000;
    }

    public static class Apu
    {
        public const byte MusicPort = 0;
        public const byte FirstSoundPort = 1;
        public const byte LibraryOnePort = FirstSoundPort;
        public const byte LibraryTwoPort = 2;
        public const byte LibraryThreePort = 3;
        public const int PortCount = 4;

        /// <summary>SPC driver command that freezes music sequencing without discarding it.</summary>
        public const byte PauseMusic = 0xf0;

        /// <summary>SPC driver command that resumes a previously paused sequence.</summary>
        public const byte ResumeMusic = 0xf1;
    }
}
