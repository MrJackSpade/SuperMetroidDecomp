
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

        /// <summary>$91:B5D1: the bank-$91 start of the ordinary ten-frame/two-tick running cadence.</summary>
        internal static ushort DefaultRunningDelayListPointer => 0xb5d3;

        /// <summary>The bank-$91 start of the cadence a Speed Booster sound-queue selection picks.</summary>
        internal static ushort ReadSpeedBoostDelayListPointer(byte selection) =>
            Start(SamusRunningCadenceDefinitions.SpeedBoostCadence(selection));

        /// <summary>Reads a cadence byte from the cadence starting at bank-$91 <paramref name="pointer"/>.</summary>
        internal static byte ReadAnimationByte(SuperMetroid.Core.Compatibility.IFloorUpdateResidue? mode7Residue,
            ushort pointer, int byteIndex) =>
            SamusRunningCadenceDefinitions.ReadAnimationByte(mode7Residue,
                Enum.GetValues<SamusRunningCadence>().Single(cadence => Start(cadence) == pointer), byteIndex);
    }

    /// <summary>The bank-$91 start of <paramref name="cadence"/>.</summary>
    private static ushort Start(SamusRunningCadence cadence) => cadence switch
    {
        SamusRunningCadence.Ordinary => 0xb5d3,
        SamusRunningCadence.SoundQueueClobber => 0x0303,
        _ => (ushort)(0xb5e8 + (cadence - SamusRunningCadence.BoostStage0) * 11),
    };
}
