using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification's native view of the charge-flare delay programs: their bank-$90 words.</summary>
internal static class ChargeFlareAnimationNative
{
    /// <summary>The flare programs as bank-$90 words.</summary>
    extension(ChargeFlareAnimationDefinitions)
    {
        /// <summary>Reads two compiled cadence bytes as a word, wrapping the second byte within bank $90.</summary>
        internal static ushort ReadWord(int address) => SnesLittleEndian.Word(ChargeFlareAnimationDefinitions.ReadByte(address),
            ChargeFlareAnimationDefinitions.ReadByte(SnesAddressMath.AddWithinBank(address, 1)));
    }
}
