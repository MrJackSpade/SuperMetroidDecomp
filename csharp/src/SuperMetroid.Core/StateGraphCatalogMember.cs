namespace SuperMetroid.Core;

/// <summary>
/// One static read-only member of a definition catalog (#1623, #2602): its name and how to read the object it holds.
/// </summary>
/// <param name="Name">The member's name.</param>
/// <param name="Read">Reads the object the member holds.</param>
public sealed partial record StateGraphCatalogMember(string Name, Func<object?> Read);
