using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Core.Audio;

/// <summary>Verification views of FLG's two meanings: the control bits and the noise rate (#631 alias proof).</summary>
internal static class DspFlagsAccess
{
    /// <summary>Separates FLG control bits from the aliased noise-rate field.</summary>
    extension(DspFlags self)
    {
        /// <summary>The reset (bit 7), mute (bit 6) and echo-write-disable (bit 5) bits.</summary>
        internal byte Control => unchecked((byte)(self.Value & ~SnesDspRegisterLayout.Fields.NoiseRateMask));

        /// <summary>The noise clock rate index (bits 0-4).</summary>
        internal int NoiseRate => self.Value & SnesDspRegisterLayout.Fields.NoiseRateMask;
    }
}
