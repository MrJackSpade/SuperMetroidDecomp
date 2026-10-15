
namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Verification access to <see cref="KraidEnemyState"/> members production does not use.</summary>
internal static class KraidEnemyStateAccess
{
    /// <summary>Derives Kraid's quarter-health threshold from the health captured at encounter start.</summary>
    extension(KraidEnemyState self)
    {
        /// <summary>Derives a quarter-health threshold from Kraid's captured initial health.</summary>
        internal ushort HealthQuarterThreshold(int index) => KraidHealthThresholdDefinitions.Quarter(self.InitialHealth, index);
    }
}
