namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>A string, written by its text.</summary>
internal sealed class StringStateType(StateGraphTypes types) : StateGraphType(typeof(string), types)
{
    /// <inheritdoc/>
    protected override string Named() => "System.String";

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        writer.Kind(PayloadKind.String);
        writer.Output.Write((string)value);
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind) =>
        kind == PayloadKind.String ? reader.Register(referenceId, reader.Input.ReadString()) : throw UnexpectedKind(kind);
}
