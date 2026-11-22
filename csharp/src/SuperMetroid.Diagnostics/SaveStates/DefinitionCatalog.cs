using SuperMetroid.Core;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// One type of the game that declares static read-only members holding an object, and the index of the objects they reach
/// (#1623). Each member is a root named by the type and the member (<c>Type::member</c>). The members are read, which runs
/// the type's initializer, and the objects they reach are indexed only when a capture or restore first needs this catalog
/// (#1716): a state of a game that never reaches a catalog never runs its initializer.
/// </summary>
/// <param name="types">The registry naming every type the catalog's objects hold.</param>
/// <param name="type">The declaring type's name.</param>
/// <param name="members">Its static read-only members holding an object other than a string, in their names' order.</param>
internal sealed class DefinitionCatalog(StateGraphTypes types, string type, IReadOnlyList<StateGraphCatalogMember> members)
{
    /// <summary>Separates a root's declaring type from its member's name.</summary>
    internal const string MemberSeparator = "::";

    /// <summary>
    /// What every root this catalog names begins with: the declaring type's name and <see cref="MemberSeparator"/>.
    /// Catalogs in the ordinal order of their prefixes hold their roots in the ordinal order of the roots' names.
    /// </summary>
    internal string Prefix { get; } = type + MemberSeparator;

    /// <summary>The declaring type's static read-only members holding an object.</summary>
    internal IReadOnlyList<StateGraphCatalogMember> Members { get; } = members;

    /// <summary>
    /// The index of every object the members reach, built breadth first over the members in name order the first time it
    /// is asked for. Reading the members runs the declaring type's initializer; a failing initializer fails the capture or
    /// restore that needed it. Static members hold definition data, fixed once initialized, so the index is never refreshed.
    /// </summary>
    internal BoundObjectGraph Graph => BuiltGraph ??= new BoundObjectGraph(types, Roots());

    /// <summary>The index once a capture or restore has first needed it, and none before.</summary>
    internal BoundObjectGraph? BuiltGraph { get; private set; }

    /// <summary>Each member holding an object other than a string, named by its prefix and member, in name order.</summary>
    private (string Name, object Root)[] Roots()
    {
        var roots = new List<(string Name, object Root)>(Members.Count);
        foreach (StateGraphCatalogMember member in Members)
            if (member.Read() is { } value && value is not string)
                roots.Add((Prefix + member.Name, value));
        roots.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
        return [.. roots];
    }
}
