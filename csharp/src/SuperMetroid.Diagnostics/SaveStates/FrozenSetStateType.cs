using System.Collections.Frozen;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>A frozen set, whatever specialization the framework made of it, rebuilt from its comparer and members (#1681).</summary>
/// <typeparam name="T">The member type.</typeparam>
internal sealed class FrozenSetStateType<T>(StateGraphTypes types) : StateGraphType(typeof(FrozenSet<T>), types)
{
    /// <inheritdoc/>
    protected override string Named() => "System.Collections.Frozen.FrozenSet`1<" + Types.NameOf(typeof(T)) + ">";

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        var set = (FrozenSet<T>)value;
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
        IEqualityComparer<T>? comparer = reader.Read<IEqualityComparer<T>?>();
        int count = reader.ReadLength("hash entry");
        var set = new HashSet<T>(count, comparer);
        for (int index = 0; index < count; index++)
        {
            T member = HashEntries.Key<T>(reader, Name);
            if (!set.Add(member))
                throw HashEntries.Duplicate(Name, member);
        }
        return reader.Register(referenceId, set.ToFrozenSet(comparer));
    }

    /// <inheritdoc/>
    internal override void Visit(object value, IStateGraphChildren children)
    {
        int index = 0;
        foreach (T member in (FrozenSet<T>)value)
            children.Element(index++, member);
    }
}
