namespace SuperMetroid.Tooling;

/// <summary>
/// Marks a development-tool adapter as the catalog view of a shipped type: the type keeps its
/// gameplay members and the adapter carries what only tools read.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class ToolingForAttribute(Type owner) : Attribute
{
    /// <summary>The shipped type this adapter describes.</summary>
    public Type Owner { get; } = owner;
}
