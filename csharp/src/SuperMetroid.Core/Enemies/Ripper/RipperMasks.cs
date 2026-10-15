namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>The Ripper's packed velocity selector.</summary>
internal static class RipperMasks
{
    /// <summary>Every selector bit but bit 8: any set selects the positive velocity.</summary>
    internal const ushort VelocitySelectorBits = 0xfeff;
}
