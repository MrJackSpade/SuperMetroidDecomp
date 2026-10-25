namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>The Ripper's packed velocity selector.</summary>
internal static class RipperMasks
{
    /// <summary>The one selector bit the direction test ignores: any other set bit selects the positive velocity.</summary>
    internal const int IgnoredSelectorBit = 1 << 8;
}
