using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// The SPC driver's packed note, instrument, sample-source and DSP-flag bytes (#1461): the one place their marker bits and
/// fields are read, for the sequencer, the voices, the definition uploads and the extraction scans alike.
/// </summary>
[EncodingOwner("SPC driver note, instrument, source and DSP flag bytes")]
internal static class SpcDriverBytes
{
    /// <summary>The note number a pitch or note byte names, without its tie or rest bit.</summary>
    /// <param name="noteOrPitch">A note byte, or the whole part of a pitch.</param>
    public static int NoteNumber(int noteOrPitch) => noteOrPitch & SpcDriverDefinitions.Music.NoteNumberMask;

    /// <summary>Whether an instrument byte names a percussion instrument rather than an instrument table entry.</summary>
    /// <param name="instrument">An instrument byte.</param>
    public static bool IsPercussion(int instrument) => (instrument & SpcDriverDefinitions.Instruments.PercussionMarker) != 0;

    /// <summary>Whether an instrument record's source byte selects the noise generator rather than a sample source.</summary>
    /// <param name="sourceOrNoiseRate">An instrument record's source-or-noise-rate byte.</param>
    public static bool UsesNoise(int sourceOrNoiseRate) => (sourceOrNoiseRate & SpcDriverDefinitions.Instruments.NoiseMarker) != 0;

    /// <summary>The echo write-disable bit of a DSP flags byte, kept while the rest of the byte is replaced.</summary>
    /// <param name="dspFlags">A DSP flags byte.</param>
    public static int EchoWriteDisableOf(int dspFlags) => dspFlags & SpcDriverDefinitions.Echo.WriteDisable;
}
