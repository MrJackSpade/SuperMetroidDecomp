namespace SuperMetroid.Core;

/// <summary>
/// Writes the fields of one object of the game into a game snapshot (#2602). Each type's generated members use it: the
/// count of the fields they write, then each field's name and value, in the order a restore reads them back in.
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

    /// <summary>Writes a number, truth value or other scalar without boxing it.</summary>
    /// <typeparam name="T">The scalar's type.</typeparam>
    /// <param name="value">The value.</param>
    void WriteScalar<T>(T value) where T : unmanaged;

    /// <summary>Writes an enum member by its name, so a snapshot does not depend on member numbering.</summary>
    /// <typeparam name="TEnum">The enum.</typeparam>
    /// <param name="value">The member.</param>
    void WriteEnum<TEnum>(TEnum value) where TEnum : struct, Enum;

    /// <summary>
    /// Writes a value of a value type, as <see cref="Write"/> writes it boxed: a writer that knows the type writes its fields
    /// without boxing it.
    /// </summary>
    /// <typeparam name="T">The value's type.</typeparam>
    /// <param name="value">The value.</param>
    void WriteValue<T>(T value) where T : struct;
}
