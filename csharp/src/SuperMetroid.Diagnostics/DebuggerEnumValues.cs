using System.Reflection;

namespace SuperMetroid.Desktop;

/// <summary>
/// The debugger-state boundary for closed domains (#627): saved enum values are validated
/// when they re-enter owned code.
/// </summary>
internal static class DebuggerEnumValues
{
    /// <summary>Converts a saved underlying value to <paramref name="enumType"/>, rejecting undefined values.</summary>
    /// <param name="enumType">The domain the value re-enters.</param>
    /// <param name="underlying">The saved underlying value.</param>
    /// <param name="zeroStorageSlot">
    /// True inside a struct element of array storage, where an unused slot is zero-filled; a
    /// zero there is storage, not a domain value. Every other zero must be a defined member.
    /// </param>
    public static object Decode(Type enumType, object underlying, bool zeroStorageSlot = false)
    {
        object value = Enum.ToObject(enumType, underlying);
        if (zeroStorageSlot && Bits(value) == 0)
            return value;
        if (enumType.IsDefined(typeof(FlagsAttribute), inherit: false))
        {
            ulong defined = 0;
            foreach (object member in Enum.GetValues(enumType))
                defined |= Bits(member);
            if ((Bits(value) & ~defined) != 0)
                throw new InvalidDataException($"Debugger state {enumType.FullName} flags value {underlying} has undefined bits.");
            return value;
        }
        if (!Enum.IsDefined(enumType, value))
            throw new InvalidDataException($"Debugger state {enumType.FullName} value {underlying} is not a defined member.");
        return value;
    }

    // Every enum underlying type fits in 64 bits; signed values keep their two's-complement bits.
    /// <summary>Bits.</summary>
    private static ulong Bits(object enumValue) => Type.GetTypeCode(Enum.GetUnderlyingType(enumValue.GetType())) switch
    {
        TypeCode.SByte or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 =>
            unchecked((ulong)Convert.ToInt64(enumValue, System.Globalization.CultureInfo.InvariantCulture)),
        _ => Convert.ToUInt64(enumValue, System.Globalization.CultureInfo.InvariantCulture),
    };
}
