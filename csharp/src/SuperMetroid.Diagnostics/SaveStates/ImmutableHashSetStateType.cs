using System.Collections.Immutable;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>An immutable hash set, rebuilt from its comparer and members as soon as they are read (#1681).</summary>
/// <typeparam name="T">The member type.</typeparam>
internal sealed class ImmutableHashSetStateType<T>(StateGraphTypes types) : StateGraphType(typeof(ImmutableHashSet<T>), types)
{
    /// <inheritdoc/>
    protected override string Named() => "System.Collections.Immutable.ImmutableHashSet`1<" + Types.NameOf(typeof(T)) + ">";

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        var set = (ImmutableHashSet<T>)value;
        writer.Kind(PayloadKind.HashEntries);
        writer.Write(HashEntries.Saved(set.KeyComparer));
        writer.Output.Write(set.Count);
        foreach (T member in set)
            writer.WriteElement(member);
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind)
    {
        if (kind != PayloadKind.HashEntries)
            throw UnexpectedKind(kind);
        ImmutableHashSet<T>.Builder builder = ImmutableHashSet<T>.Empty.WithComparer(reader.Read<IEqualityComparer<T>?>()).ToBuilder();
        int count = reader.ReadLength("hash entry");
        for (int index = 0; index < count; index++)
        {
            T member = HashEntries.Key<T>(reader, Name);
            if (!builder.Add(member))
                throw HashEntries.Duplicate(Name, member);
        }
        return reader.Register(referenceId, builder.ToImmutable());
    }

    /// <inheritdoc/>
    internal override void Visit(object value, IStateGraphChildren children)
    {
        int index = 0;
        foreach (T member in (ImmutableHashSet<T>)value)
            children.Element(index++, member);
    }
}
