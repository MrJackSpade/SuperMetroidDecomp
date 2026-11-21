namespace SuperMetroid.Core;

/// <summary>
/// Writes the fields of one object of the game into a debugger state (#2602). The compiler writes each type's use of it:
/// the count of the fields it writes, then each field's name and value, in an order a restore reads them back in.
/// </summary>
public interface IStateGraphWriter
{
    /// <summary>Begins an object's fields: how many follow.</summary>
    /// <param name="count">How many fields the object writes.</param>
    void Fields(int count);

    /// <summary>Names the field whose value follows.</summary>
    /// <param name="field">The field.</param>
    void Field(StateGraphField field);

    /// <summary>Writes any value: an object by its type, a shared object once, a value type boxed.</summary>
    /// <param name="value">The value, or none.</param>
    void Write(object? value);

    /// <summary>Writes a truth value.</summary>
    /// <param name="value">The value.</param>
    void WriteBoolean(bool value);

    /// <summary>Writes an unsigned byte.</summary>
    /// <param name="value">The value.</param>
    void WriteByte(byte value);

    /// <summary>Writes a signed byte.</summary>
    /// <param name="value">The value.</param>
    void WriteSByte(sbyte value);

    /// <summary>Writes a signed 16-bit integer.</summary>
    /// <param name="value">The value.</param>
    void WriteInt16(short value);

    /// <summary>Writes an unsigned 16-bit integer.</summary>
    /// <param name="value">The value.</param>
    void WriteUInt16(ushort value);

    /// <summary>Writes a signed 32-bit integer.</summary>
    /// <param name="value">The value.</param>
    void WriteInt32(int value);

    /// <summary>Writes an unsigned 32-bit integer.</summary>
    /// <param name="value">The value.</param>
    void WriteUInt32(uint value);

    /// <summary>Writes a signed 64-bit integer.</summary>
    /// <param name="value">The value.</param>
    void WriteInt64(long value);

    /// <summary>Writes an unsigned 64-bit integer.</summary>
    /// <param name="value">The value.</param>
    void WriteUInt64(ulong value);

    /// <summary>Writes a character.</summary>
    /// <param name="value">The value.</param>
    void WriteChar(char value);

    /// <summary>Writes a single-precision number.</summary>
    /// <param name="value">The value.</param>
    void WriteSingle(float value);

    /// <summary>Writes a double-precision number.</summary>
    /// <param name="value">The value.</param>
    void WriteDouble(double value);

    /// <summary>Writes a decimal number.</summary>
    /// <param name="value">The value.</param>
    void WriteDecimal(decimal value);

    /// <summary>Writes an enum member by its name, so a state does not depend on member numbering.</summary>
    /// <typeparam name="TEnum">The enum.</typeparam>
    /// <param name="value">The member.</param>
    void WriteEnum<TEnum>(TEnum value) where TEnum : struct, Enum;
}
