namespace SuperMetroid.Core.Audio;

/// <summary>Verified cartridge addresses and fixed engine values for SNES audio dispatch.</summary>
public static class AudioRomData
{
    /// <summary>Initial SPC driver upload identity, separate from the subsequently selected music-data banks.</summary>
    public static class Assets
    {
        /// <summary><c>SPC_Engine</c> at <c>$CF:8000</c>; the boot-time upload stream containing the SPC engine and its shared audio data.</summary>
        public const int InitialAudioBank = 0xcf8000;
    }

    /// <summary>Ring-buffer sizes and update-counted scheduling values used by the bank-$80 music and sound handlers.</summary>
    public static class Queues
    {
        /// <summary>Number of word-sized command slots, with matching delay words, in the native music queue.</summary>
        public const int MusicCapacity = 8;
        /// <summary>Wrap mask for host music-slot indices $00-$07; native word-byte indices use $000E instead.</summary>
        public const int MusicIndexMask = MusicCapacity - 1;
        /// <summary>Number of independently queued sound libraries, mapped to APU ports one through three.</summary>
        public const int SoundLibraryCount = 3;
        /// <summary>Number of byte-sized command slots in each sound-library ring, including its reserved empty slot.</summary>
        public const int SoundCapacity = 16;
        /// <summary>Wrap and occupancy-difference mask for the sound-library ring's byte indices $00-$0F.</summary>
        public const int SoundIndexMask = SoundCapacity - 1;
        /// <summary>Largest permitted pending-command occupancy, fifteen; one sound-ring slot must remain empty to distinguish full from empty.</summary>
        public const byte MaximumSoundOccupancy = SoundCapacity - 1;
        /// <summary>Minimum effective music countdown, eight calls to the native music handler; delayed-Y requests are clamped to this value.</summary>
        public const ushort MinimumMusicDelayFrames = 8;
        /// <summary>Eight sound-handler invocations that clear sound ports after a music-track write or music-data upload.</summary>
        public const int MusicAndSfxDowntimeFrames = 8;
        /// <summary>$0168 (360) music-countdown updates before the queued stop following a permanent-item fanfare; the saved room track is queued eight updates after that stop.</summary>
        public const ushort PermanentItemFanfareFrames = 0x0168;
        /// <summary>Track two of the currently loaded music data, queued by <c>Instruction_PLM_ClearMusicQueue_QueueMusicTrack</c> at <c>$84:8BDD</c> for permanent-item fanfares.</summary>
        public const byte PermanentItemTrack = 2;
    }

    /// <summary>Bit fields in the native music queue word; kept separate from queue policy.</summary>
    public static class MusicWireFormat
    {
        /// <summary>Canonical $FF high byte combined with a low-byte music-data pointer-table offset to request an upload.</summary>
        public const ushort DataCommandPrefix = 0xff00;
        /// <summary>High-byte mask used to recognize canonical $FFxx data commands, distinct from the native sign-bit upload test.</summary>
        public const ushort DataCommandKindMask = 0xff00;
        /// <summary>Native command sign bit tested by <c>HandleMusicQueue</c> at <c>$80:8F1B</c>; any set value takes the data-upload path, including noncanonical words.</summary>
        public const ushort UploadPathBit = 0x8000;
        /// <summary>Low-seven-bit mask applied at <c>$80:8F1F</c> before a track command is written to APU port zero.</summary>
        public const ushort TrackMask = 0x007f;
        /// <summary>Largest canonical track selector, $7F; zero instead denotes stopping music.</summary>
        public const byte MaximumTrack = 0x7f;
        /// <summary>Music-timer sign bit tested after decrement at <c>$80:8F12</c>; a set bit takes the next-entry fetch path, rather than continuing an active positive countdown.</summary>
        public const ushort ActiveTimerBit = 0x8000;
    }

    /// <summary>Physical shape and safety bounds of a native APU upload stream.</summary>
    /// <remarks>Streams consumed by <c>SendAPUData</c> at <c>$80:8059</c> contain little-endian byte-count and SPC-destination words followed by payload bytes; a zero count terminates the blocks and precedes the SPC execution-address word.</remarks>
    public static class SpcUpload
    {
        /// <summary>Host import safety threshold of $11000 collected bytes, checked before each upload block; this is not a native block-length field or exact stream size.</summary>
        public const int MaximumStreamBytes = 0x1_1000;
        /// <summary>Largest representable 24-bit SNES source address, $FF:FFFF, accepted before upper-window validation.</summary>
        public const int MaximumSnesAddress = 0x00ff_ffff;
        /// <summary>Offset bit selecting a LoROM bank's cartridge-mapped upper half, $8000-$FFFF.</summary>
        public const int LoRomUpperWindowBit = 0x8000;
        /// <summary>Eight-bit mask for extracting or wrapping the bank byte when advancing an import stream.</summary>
        public const int AddressBankMask = 0xff;
        /// <summary>Sixteen-bit mask isolating a SNES address's within-bank offset.</summary>
        public const int AddressOffsetMask = 0xffff;
        /// <summary>Final mapped source offset in a LoROM bank; the following physical cartridge byte lies at the next bank's $8000.</summary>
        public const int LastBankOffset = 0xffff;
        /// <summary>First mapped source offset selected in the next bank when an upload stream crosses $FFFF.</summary>
        public const int FirstMappedBankOffset = 0x8000;
    }

    /// <summary>Zero-based APU I/O port selectors and SPC music-control command bytes, not host audio-channel indices.</summary>
    public static class Apu
    {
        /// <summary>Number of byte-wide bidirectional APU I/O ports, indexed zero through three and mapped to SNES registers <c>$2140-$2143</c>.</summary>
        public const int PortCount = 4;

        /// <summary>SPC driver command that freezes music sequencing without discarding it.</summary>
        public const byte PauseMusic = 0xf0;

        /// <summary>SPC driver command that resumes a previously paused sequence.</summary>
        public const byte ResumeMusic = 0xf1;
    }
}

/// <summary>The four byte-wide bidirectional APU I/O ports, SNES registers <c>$2140-$2143</c>.</summary>
public enum ApuPort : byte
{
    /// <summary>Port zero, <c>$2140</c>, used for music commands and upload handshakes.</summary>
    Music = 0,
    /// <summary>Port one, <c>$2141</c>: sound library one; adding a zero-based library index selects ports one through three.</summary>
    SoundLibrary1 = 1,
    /// <summary>Port two, <c>$2142</c>: sound library two.</summary>
    SoundLibrary2 = 2,
    /// <summary>Port three, <c>$2143</c>: sound library three.</summary>
    SoundLibrary3 = 3,
}
