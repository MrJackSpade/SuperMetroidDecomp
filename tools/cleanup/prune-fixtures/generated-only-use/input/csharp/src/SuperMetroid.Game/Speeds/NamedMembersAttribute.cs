namespace SuperMetroid.Game.Speeds;

/// <summary>
/// Marks a static partial method the compiler implements with the named members of the owner types: every public static
/// property of the method's value type, with its value, its type's name and its own name.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class NamedMembersAttribute : Attribute
{
    /// <summary>Names the catalog types whose members are listed, in order.</summary>
    /// <param name="owners">The catalog types.</param>
    public NamedMembersAttribute(params Type[] owners) => ArgumentNullException.ThrowIfNull(owners);
}
