
namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>Verification access to <see cref="MagdollitePhaseDefinitions"/> members production does not use.</summary>
internal static class MagdollitePhaseDefinitionsAccess
{
    /// <summary>Exposes the native arm-height threshold address.</summary>
    extension(MagdollitePhaseDefinitions)
    {
        /// <summary>$A8:AF55, <c>MagdolliteArmHeightThreshold</c>.</summary>
        internal static ushort ArmHeightThresholdTable => 0xaf55;
    }
}
