using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Enemies;

/// <summary>Verification's view of a callback's bank-local cartridge word.</summary>
internal static class EnemyInteractionCallbackAccess
{
    /// <summary>The bank-local word of <paramref name="callback"/>'s entry point.</summary>
    internal static ushort Word(EnemyInteractionCallback callback) => SnesLittleEndian.LowWord((int)callback);
}
