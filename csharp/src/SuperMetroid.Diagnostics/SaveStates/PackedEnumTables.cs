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

    /// <summary>Writes every table: how many, then each enum's name and its members' names.</summary>
    internal void Write(GraphWriter writer)
    {
        writer.Output.Write(tables.Count);
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
