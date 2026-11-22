namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// A delegate of <typeparamref name="TDelegate"/> (#2602): each call of its invocation list as the name of the method it
/// calls, which the compiler registered with the type declaring it, and its target. The method is found by remaking each
/// registered method's delegate for the call's target and comparing it with the call, so a state never asks a delegate
/// what method it calls.
/// </summary>
/// <typeparam name="TDelegate">The delegate type.</typeparam>
internal sealed class DelegateStateType<TDelegate>(StateGraphTypes types) : StateGraphType(typeof(TDelegate), types)
    where TDelegate : Delegate
{
    /// <summary>The methods delegates of the type are made from, each by name.</summary>
    private readonly List<(string Name, Func<object?, TDelegate?> Create)> methods = [];

    /// <summary>Each method by name.</summary>
    private readonly Dictionary<string, Func<object?, TDelegate?>> byName = new(StringComparer.Ordinal);

    /// <inheritdoc/>
    protected override string Named() => DelegateName();

    private static string DelegateName() => typeof(TDelegate).FullName ?? typeof(TDelegate).Name;

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        Delegate[] calls = ((Delegate)value).GetInvocationList();
        writer.Kind(PayloadKind.Delegate);
        writer.Output.Write(calls.Length);
        foreach (Delegate call in calls)
        {
            writer.WriteName(MethodOf(call));
            writer.Write(call.Target);
        }
    }

    /// <summary>The name of the registered method <paramref name="call"/> calls.</summary>
    /// <exception cref="NotSupportedException">No registered method makes the call: it calls a lambda or another assembly's method.</exception>
    private string MethodOf(Delegate call)
    {
        foreach ((string name, Func<object?, TDelegate?> create) in methods)
        {
            if (create(call.Target) is { } remade && remade.Equals(call))
                return name;
        }
        throw new NotSupportedException(
            $"Debugger state cannot hold a {DelegateName()} made from a method no type registers, such as a lambda; its target is {call.Target?.GetType().FullName ?? "none"}");
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind)
    {
        if (kind != PayloadKind.Delegate)
            throw UnexpectedKind(kind);
        int count = reader.ReadLength("delegate invocation");
        if (count == 0)
            throw new InvalidDataException("A saved delegate has an empty invocation list.");
        Delegate? combined = null;
        for (int index = 0; index < count; index++)
        {
            string name = reader.ReadName();
            if (!byName.TryGetValue(name, out Func<object?, TDelegate?>? create))
                throw new InvalidDataException($"{DebuggerObjectGraphSerializer.UnsupportedVersion}: a saved {DelegateName()} calls {name}, which this build does not have.");
            object? target = reader.Read();
            TDelegate call = create(target) ?? throw new InvalidDataException(
                $"{DebuggerObjectGraphSerializer.UnsupportedVersion}: a saved {DelegateName()} calls {name} on a {target?.GetType().FullName ?? "missing target"}, which it does not take.");
            combined = combined is null ? call : Delegate.Combine(combined, call);
        }
        return reader.Register(referenceId, combined!);
    }
}
