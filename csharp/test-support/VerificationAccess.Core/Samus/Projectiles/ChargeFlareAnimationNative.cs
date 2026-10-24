using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification's native view of the charge-flare delay programs: their bank-$90 words.</summary>
internal static class ChargeFlareAnimationNative
{
    /// <summary>The flare programs as words.</summary>
    extension(ChargeFlareAnimationDefinitions)
    {
        /// <summary>Reads two compiled cadence bytes, <paramref name="offset"/> bytes into the programs, as a word.</summary>
        internal static ushort ReadWord(int offset) => SnesLittleEndian.Word(ChargeFlareAnimationDefinitions.ReadByte(offset),
            ChargeFlareAnimationDefinitions.ReadByte(offset + 1));
    }
}
