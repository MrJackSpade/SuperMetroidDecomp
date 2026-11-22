namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>A value that is a type (#2060), such as a key of a store of catalogs found by their type: held as the type's name.</summary>
internal sealed class TypeIdentityStateType(StateGraphTypes types) : StateGraphType(typeof(Type), types)
{
    /// <inheritdoc/>
    protected override string Named() => "System.Type";

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        writer.Kind(PayloadKind.TypeIdentity);
        writer.WriteName(Types.NameOf((Type)value));
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind) =>
        kind == PayloadKind.TypeIdentity ? reader.Register(referenceId, Types.TypeNamed(reader.ReadName())) : throw UnexpectedKind(kind);
}
