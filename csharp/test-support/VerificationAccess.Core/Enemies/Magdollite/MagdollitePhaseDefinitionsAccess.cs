
namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>Verification access to <see cref="MagdollitePhaseDefinitions"/> members production does not use.</summary>
internal static class MagdollitePhaseDefinitionsAccess
{
    /// <summary>Exposes the per-slot Magdollite apex table size and native arm-height threshold address.</summary>
    extension(MagdollitePhaseDefinitions)
    {
        /// <summary>Number of enemy slots covered by <see cref="MagdollitePhaseDefinitions.ApexGrowsFinalPhase"/>.</summary>
        internal static int ApexEnemySlotCount =>
            MagdollitePhaseDefinitions.s_apexGrowsByEnemySlot.Length;

        /// <summary>$A8:AF55, <c>MagdolliteArmHeightThreshold</c>.</summary>
        internal static ushort ArmHeightThresholdTable => 0xaf55;
    }
}
