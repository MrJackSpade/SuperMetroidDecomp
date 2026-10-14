/// <summary>
/// Declares a verification command on the handler method that owns it (#621). The dispatcher discovers every
/// attributed handler, so adding one makes it dispatchable and listed in usage and help without editing a registry.
/// Several attributes on one handler declare aliases or intentional arity variants.
/// </summary>
/// <param name="name">The command-line name, such as <c>--crystal-flash</c>.</param>
/// <param name="arguments">
/// The arguments that follow the name, in order: a placeholder binds one handler parameter; an entry starting with
/// <c>--</c> is a literal keyword that must appear at that position, such as <c>--trace-from</c>.
/// </param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
internal sealed class VerificationCommandAttribute(string name, params string[] arguments) : Attribute
{
    /// <summary>The command-line name.</summary>
    public string Name { get; } = name;

    /// <summary>The placeholders and literal keywords that follow the name, in order.</summary>
    public IReadOnlyList<string> Arguments { get; } = arguments;

    /// <summary>A one-line description shown by <c>--help</c>.</summary>
    public string Description { get; init; } = "";
}
