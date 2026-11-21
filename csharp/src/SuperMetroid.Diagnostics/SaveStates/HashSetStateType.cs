namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>A hash set, rebuilt from its comparer and members once the whole state is read (#1681).</summary>
/// <typeparam name="T">The member type.</typeparam>
internal sealed class HashSetStateType<T>(StateGraphTypes types) : StateGraphType(typeof(HashSet<T>), types)
{
    /// <inheritdoc/>
    protected override string Named() => "System.Collections.Generic.HashSet`1<" + Types.NameOf(typeof(T)) + ">";

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        var set = (HashSet<T>)value;
        writer.Kind(PayloadKind.HashEntries);
        writer.Write(HashEntries.Saved(set.Comparer));
        writer.Output.Write(set.Count);
        foreach (T member in set)
            writer.WriteElement(member);
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind)
    {
        if (kind != PayloadKind.HashEntries)
            throw UnexpectedKind(kind);
        var set = new HashSet<T>(reader.Read<IEqualityComparer<T>?>());
        reader.Register(referenceId, set);
        var members = new T[reader.ReadLength("hash entry")];
        for (int index = 0; index < members.Length; index++)
            members[index] = reader.ReadElement<T>();
        reader.Defer(() =>
        {
            set.EnsureCapacity(members.Length);
            foreach (T member in members)
            {
                if (!set.Add(member))
                    throw HashEntries.Duplicate(Name, member);
            }
        });
        return set;
    }

    /// <inheritdoc/>
    internal override void Visit(object value, IStateGraphChildren children)
    {
        int index = 0;
        foreach (T member in (HashSet<T>)value)
            children.Element(index++, member);
    }

    /// <inheritdoc/>
    internal override int? Version(object value) => ((HashSet<T>)value).Count;
}
