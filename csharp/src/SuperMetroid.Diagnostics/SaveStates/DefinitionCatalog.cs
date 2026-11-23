using SuperMetroid.Core;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// One type of the game that declares static read-only members holding an object (#1623). Each member is a root named by
/// the type and the member (<c>Type::member</c>). Reading the members runs the type's initializer, so the index of the
/// game's static members reads them only once the type has initialized (<see cref="StateGraphCatalogInitializations"/>,
/// #2619), or when a restore names one of them.
/// </summary>
/// <param name="type">The declaring type.</param>
/// <param name="name">The declaring type's name.</param>
/// <param name="members">Its static read-only members holding an object other than a string, in their names' order.</param>
internal sealed class DefinitionCatalog(Type type, string name, IReadOnlyList<StateGraphCatalogMember> members)
{
    /// <summary>Separates a root's declaring type from its member's name.</summary>
    internal const string MemberSeparator = "::";

    /// <summary>The declaring type.</summary>
    internal Type Type { get; } = type;

    /// <summary>What every root this catalog names begins with: the declaring type's name and <see cref="MemberSeparator"/>.</summary>
    internal string Prefix { get; } = name + MemberSeparator;

    /// <summary>The declaring type's static read-only members holding an object.</summary>
    internal IReadOnlyList<StateGraphCatalogMember> Members { get; } = members;

    /// <summary>
    /// Each member holding an object other than a string, named by its prefix and member, in name order. Reading them runs
    /// the declaring type's initializer unless it has run; a failing initializer fails the capture or restore that read them.
    /// </summary>
    internal (string Name, object Root)[] Roots()
    {
        var roots = new List<(string Name, object Root)>(Members.Count);
        foreach (StateGraphCatalogMember member in Members)
            if (member.Read() is { } value && value is not string)
                roots.Add((Prefix + member.Name, value));
        roots.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
        return [.. roots];
    }
}
