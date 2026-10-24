namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>Verified cartridge values the audio import reads upload streams by.</summary>
public static class AudioRomData
{
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
        /// <summary>Final mapped source offset in a LoROM bank; the following physical cartridge byte lies at the next bank's $8000.</summary>
        public const int LastBankOffset = 0xffff;
        /// <summary>First mapped source offset selected in the next bank when an upload stream crosses $FFFF.</summary>
        public const int FirstMappedBankOffset = 0x8000;
    }
}
