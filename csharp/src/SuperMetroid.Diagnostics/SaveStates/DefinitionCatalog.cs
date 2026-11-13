using SuperMetroid.Core;
using System.Reflection;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// One type of the game's assembly that declares static read-only members holding a reference, and the index of the
/// objects they reach (#1623). Each member is a root named by the type and the field (<c>Type::field</c>). The members are
/// read, which runs the type's initializer, and the objects they reach are indexed only when a capture or restore first
/// needs this catalog (#1716): a state of a game that never reaches a catalog never runs its initializer.
/// </summary>
/// <param name="type">The declaring type.</param>
/// <param name="members">Its static read-only fields that can hold a reference other than a string.</param>
[ReflectionAudit("debugger states name the objects the game's static members hold by the member that holds them")]
internal sealed class DefinitionCatalog(Type type, IReadOnlyList<FieldInfo> members)
{
    /// <summary>Separates a root's declaring type from its field name.</summary>
    internal const string MemberSeparator = "::";

    /// <summary>The declaring type.</summary>
    internal Type Type { get; } = type;

    /// <summary>
    /// What every root this catalog names begins with: the declaring type's name and <see cref="MemberSeparator"/>.
    /// Catalogs in the ordinal order of their prefixes hold their roots in the ordinal order of the roots' names.
    /// </summary>
    internal string Prefix { get; } = Ensure.NotNull(type.FullName) + MemberSeparator;

    /// <summary>The declaring type's static read-only fields that can hold a reference other than a string.</summary>
    internal IReadOnlyList<FieldInfo> Members { get; } = members;

    /// <summary>The index of the objects the members reach, or null until a capture or restore first needs it.</summary>
    internal BoundObjectGraph? graph;

    /// <summary>
    /// The index of every object the members reach, built breadth first over the members in name order the first time it
    /// is asked for. Reading the members runs the declaring type's initializer; a failing initializer fails the capture
    /// or restore that needed it. Static members hold definition data, fixed once initialized, so the index is never
    /// refreshed: a part a member loads on first use after that is written by its contents.
    /// </summary>
    internal BoundObjectGraph Graph => graph ??= new BoundObjectGraph(Roots());

    /// <summary>Each member holding an object other than a string, named by its prefix and field, in name order.</summary>
    private (string Name, object Root)[] Roots()
    {
        var roots = new List<(string Name, object Root)>(Members.Count);
        foreach (FieldInfo member in Members)
            if (member.GetValue(null) is { } value && value is not string)
                roots.Add((Prefix + member.Name, value));
        roots.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
        return [.. roots];
    }
}
