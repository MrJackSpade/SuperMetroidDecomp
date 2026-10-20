
namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Verification access to <see cref="KraidHealthThresholdDefinitions"/> members production does not use.</summary>
internal static class KraidHealthThresholdDefinitionsAccess
{
    /// <summary>Reproduces the four native thresholds formed by truncating initial health to quarters.</summary>
    extension(KraidHealthThresholdDefinitions)
    {
        /// <summary>$A7:AA23-AA43: truncate health/4, then accumulate four multiples.</summary>
        internal static int Quarter(int initialHealth, int index)
        {
            if ((uint)index >= 4) throw new ArgumentOutOfRangeException(nameof(index));
            return (initialHealth >> 2) * (index + 1);
        }
    }
}
