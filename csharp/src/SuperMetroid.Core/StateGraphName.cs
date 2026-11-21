namespace SuperMetroid.Core;

/// <summary>
/// The name a debugger state gives a type (#2602): its declared name, and for a constructed generic type the types it is
/// constructed with, whose own names the state's registry supplies once they are known.
/// </summary>
public sealed partial class StateGraphName
{
    /// <summary>Names a type <paramref name="declared"/>, constructed with <paramref name="arguments"/> when it is generic.</summary>
    /// <param name="declared">The declared name: namespace, containing types after <c>+</c>, and a generic arity after a backtick.</param>
    /// <param name="arguments">The types a generic type is constructed with, in order; none for any other type.</param>
    public StateGraphName(string declared, params Type[] arguments)
    {
        Declared = Ensure.NotNull(declared);
        Arguments = Ensure.NotNull(arguments);
    }

    /// <summary>The declared name.</summary>
    public string Declared { get; }

    /// <summary>The types a generic type is constructed with, in order.</summary>
    public IReadOnlyList<Type> Arguments { get; }
}
