namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// The debugger-state boundary for closed domains (#627): saved enum values are validated
/// when they re-enter owned code.
/// </summary>
internal static class DebuggerEnumValues
{
    /// <summary>
    /// Each enum name a restore decoded to a defined member, by its enum, with the boxed member: an enum's boxed member is
    /// immutable, and a field or element it is stored into copies it. Rejected names, and zero storage slots decoded only
    /// within struct array storage, are not kept, so they are validated again each time.
    /// </summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<(Type Enum, string Name), object> decodedNames = new();

    /// <summary>
    /// The member of <paramref name="enumType"/> a state names <paramref name="name"/> (#1507), validated as
    /// <see cref="Decode"/> validates its value.
    /// </summary>
    /// <param name="enumType">The domain the value re-enters.</param>
    /// <param name="name">The saved member name, or names joined for flags.</param>
    /// <param name="zeroStorageSlot">True inside a struct element of array storage, as for <see cref="Decode"/>.</param>
    /// <exception cref="InvalidDataException">The name names no member of this build, or an undefined value.</exception>
    public static object DecodeName(Type enumType, string name, bool zeroStorageSlot)
    {
        // A name that decoded to a defined member decodes to it in every context, so it is parsed and validated once.
        if (decodedNames.TryGetValue((enumType, name), out object? known))
            return known;
        if (!Enum.TryParse(enumType, name, ignoreCase: false, out object? parsed))
            throw new InvalidDataException($"Debugger state {enumType.FullName} value '{name}' names no member of this build.");
        object underlying = Convert.ChangeType(parsed, Enum.GetUnderlyingType(enumType), System.Globalization.CultureInfo.InvariantCulture);
        object decoded = Decode(enumType, underlying, zeroStorageSlot);
        if (!zeroStorageSlot)
            decodedNames.TryAdd((enumType, name), decoded);
        return decoded;
    }

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

    /// <summary>Normalizes any enum backing type to a 64-bit bit pattern while
    /// retaining the two's-complement representation of signed values.</summary>
    private static ulong Bits(object enumValue) => Type.GetTypeCode(Enum.GetUnderlyingType(enumValue.GetType())) switch
    {
        TypeCode.SByte or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 =>
            unchecked((ulong)Convert.ToInt64(enumValue, System.Globalization.CultureInfo.InvariantCulture)),
        TypeCode.Empty or
            TypeCode.Object or
            TypeCode.DBNull or
            TypeCode.Boolean or
            TypeCode.Char or
            TypeCode.Byte or
            TypeCode.UInt16 or
            TypeCode.UInt32 or
            TypeCode.UInt64 or
            TypeCode.Single or
            TypeCode.Double or
            TypeCode.Decimal or
            TypeCode.DateTime or
            TypeCode.String => Convert.ToUInt64(enumValue, System.Globalization.CultureInfo.InvariantCulture),
        _ => Convert.ToUInt64(enumValue, System.Globalization.CultureInfo.InvariantCulture),
    };
}
