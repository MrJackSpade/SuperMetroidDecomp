namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// The enum members one packed run holds (#1696): each enum's members in the order the run first holds each, which the
/// run's values name by their place, and which the state stores once, by name, before the values.
/// </summary>
/// <param name="types">The registry naming each enum.</param>
internal sealed class PackedEnumTables(StateGraphTypes types)
{
    /// <summary>Each enum's table, in the order the run first holds one of its members.</summary>
    private readonly List<IPackedEnumTable> tables = [];

    /// <summary>Each enum's table, by the enum.</summary>
    private readonly Dictionary<Type, IPackedEnumTable> byEnum = [];

    /// <summary>The place of <paramref name="member"/> in its enum's table, adding it when the run first holds it.</summary>
    internal int Index<TEnum>(TEnum member) where TEnum : struct, Enum => Table<TEnum>().Index(member);

    /// <summary>The member at <paramref name="index"/> of <typeparamref name="TEnum"/>'s table.</summary>
    /// <exception cref="InvalidDataException">The run holds no such member.</exception>
    internal TEnum Member<TEnum>(int index) where TEnum : struct, Enum => byEnum.TryGetValue(typeof(TEnum), out IPackedEnumTable? table)
        ? ((PackedEnumTable<TEnum>)table).Member(index)
        : throw new InvalidDataException($"A packed run holds a {typeof(TEnum).FullName} but stores no members of it.");

    /// <summary>
    /// The member at <paramref name="index"/> of the table of <typeparamref name="T"/>, an enum named only as a type
    /// argument, as a primary-constructor argument's is.
    /// </summary>
    /// <exception cref="InvalidDataException">The run holds no such member.</exception>
    internal T MemberOf<T>(int index) => byEnum.TryGetValue(typeof(T), out IPackedEnumTable? table)
        ? (T)table.MemberAt(index)
        : throw new InvalidDataException($"A packed run holds a {typeof(T).FullName} but stores no members of it.");

    private PackedEnumTable<TEnum> Table<TEnum>() where TEnum : struct, Enum
    {
        if (byEnum.TryGetValue(typeof(TEnum), out IPackedEnumTable? known))
            return (PackedEnumTable<TEnum>)known;
        var table = new PackedEnumTable<TEnum>(types.EnumOf<TEnum>());
        byEnum.Add(typeof(TEnum), table);
        tables.Add(table);
        return table;
    }

    /// <summary>Writes every table: how many, then each enum's name and its members' names.</summary>
    internal void Write(BinaryWriter writer)
    {
        writer.Write(tables.Count);
        foreach (IPackedEnumTable table in tables)
            table.Write(writer);
    }

    /// <summary>Reads the tables a run stores, each member validated as a packed element's is.</summary>
    internal static PackedEnumTables Read(GraphReader reader)
    {
        var read = new PackedEnumTables(reader.Types);
        int count = reader.ReadLength("packed enum table");
        for (int index = 0; index < count; index++)
        {
            string name = reader.ReadName();
            if (reader.Types.Named(name) is not IPackedEnumType enumType)
                throw new InvalidDataException($"{DebuggerObjectGraphSerializer.UnsupportedVersion}: a packed run's table names {name}, which is no enum of this build.");
            IPackedEnumTable table = enumType.ReadTable(reader);
            read.byEnum.Add(table.Enum, table);
            read.tables.Add(table);
        }
        return read;
    }
}
