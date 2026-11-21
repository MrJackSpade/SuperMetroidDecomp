namespace SuperMetroid.Core;

/// <summary>
/// Reads the fields of one object of the game back from a debugger state (#2602), in the order the object's writer wrote
/// them, checking each field's name against this build's so a state of another layout is refused rather than misread.
/// </summary>
public interface IStateGraphReader
{
    /// <summary>Begins an object's fields, checking how many the state holds.</summary>
    /// <param name="count">How many fields this build's object reads.</param>
    void Fields(int count);

    /// <summary>Checks that the field whose value follows is <paramref name="field"/>.</summary>
    /// <param name="field">The field this build reads next.</param>
    void Field(StateGraphField field);

    /// <summary>
    /// Records the object being restored as soon as it exists, so objects inside it that hold it are restored holding it.
    /// </summary>
    /// <param name="instance">The object.</param>
    void Created(object instance);

    /// <summary>Reads any value, checking that it is a <typeparamref name="T"/> or none.</summary>
    /// <typeparam name="T">The type the field holds.</typeparam>
    T Read<T>();

    /// <summary>
    /// Reads the value of a constructor argument named <paramref name="field"/>, which it also returns; the first of an
    /// object's arguments checks how many the state holds.
    /// </summary>
    /// <typeparam name="T">The argument's type.</typeparam>
    /// <param name="ordinal">The argument's place among the arguments the state holds, from zero.</param>
    /// <param name="count">How many arguments this build's object reads.</param>
    /// <param name="field">The argument.</param>
    /// <param name="value">Receives the value.</param>
    T Argument<T>(int ordinal, int count, StateGraphField field, out T value);

    /// <summary>
    /// Reads the value of a field that only an object's construction sets, checking that it is what the construction set:
    /// <paramref name="current"/>, or a value equal to it.
    /// </summary>
    /// <typeparam name="T">The field's type.</typeparam>
    /// <param name="current">What the construction set.</param>
    void Fixed<T>(T current);

    /// <summary>Reads a truth value.</summary>
    bool ReadBoolean();

    /// <summary>Reads an unsigned byte.</summary>
    byte ReadByte();

    /// <summary>Reads a signed byte.</summary>
    sbyte ReadSByte();

    /// <summary>Reads a signed 16-bit integer.</summary>
    short ReadInt16();

    /// <summary>Reads an unsigned 16-bit integer.</summary>
    ushort ReadUInt16();

    /// <summary>Reads a signed 32-bit integer.</summary>
    int ReadInt32();

    /// <summary>Reads an unsigned 32-bit integer.</summary>
    uint ReadUInt32();

    /// <summary>Reads a signed 64-bit integer.</summary>
    long ReadInt64();

    /// <summary>Reads an unsigned 64-bit integer.</summary>
    ulong ReadUInt64();

    /// <summary>Reads a character.</summary>
    char ReadChar();

    /// <summary>Reads a single-precision number.</summary>
    float ReadSingle();

    /// <summary>Reads a double-precision number.</summary>
    double ReadDouble();

    /// <summary>Reads a decimal number.</summary>
    decimal ReadDecimal();

    /// <summary>Reads an enum member stored by its name.</summary>
    /// <typeparam name="TEnum">The enum.</typeparam>
    TEnum ReadEnum<TEnum>() where TEnum : struct, Enum;
}
