namespace SuperMetroid.Core;

/// <summary>
/// One static read-only member of a definition catalog (#1623, #2602): its name, the name of the type it is declared with,
/// and how to read the object it holds, which builds its declaring type's definitions only when a snapshot first needs them.
/// </summary>
/// <param name="Name">The member's name.</param>
/// <param name="Declared">The name of the type the member is declared with.</param>
/// <param name="Read">Reads the object the member holds.</param>
public sealed partial record StateGraphCatalogMember(string Name, string Declared, Func<object?> Read);
