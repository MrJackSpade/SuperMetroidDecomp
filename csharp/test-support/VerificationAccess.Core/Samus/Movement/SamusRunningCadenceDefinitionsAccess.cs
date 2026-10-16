
namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Verification access to <see cref="SamusRunningCadenceDefinitions"/> members production does not use.</summary>
internal static class SamusRunningCadenceDefinitionsAccess
{
    /// <summary>Reads only bytes compiled into Samus's running cadence instruction data.</summary>
    extension(SamusRunningCadenceDefinitions)
    {
        /// <summary>Returns one byte from the complete bounded native cadence catalog.</summary>
        internal static byte ReadCompiledByte(int address)
        {
            if (SamusRunningCadenceDefinitions.TryReadCompiledByte(address, out byte value))
                return value;
            throw new ArgumentOutOfRangeException(
                nameof(address), address, "Address is outside the compiled Samus running-cadence catalog.");
        }
    }
}
