namespace SuperMetroid.Core.Assets;

/// <summary>Verification access to colour-channel arithmetic production does not need.</summary>
internal static class RgbColorAccess
{
    /// <summary>Channel arithmetic only verification runs.</summary>
    extension(RgbColor)
    {
        /// <summary>Expands a five-bit channel to eight bits by replicating its high bits into the low ones.</summary>
        public static byte ExpandChannel(int fiveBit) =>
            (byte)(((fiveBit & RgbColor.MaxChannel) << 3) | ((fiveBit & RgbColor.MaxChannel) >> 2));
    }
}
