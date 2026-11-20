namespace SuperMetroid.Core.Enemies;

/// <summary>
/// How a room's population places one kind of multipart enemy (#1542, #2556): the role each of its members plays, in the
/// order the population lists the members together. A member finds the member playing another role through
/// <see cref="EnemyCompositeListing"/> once, as it is placed, and keeps the place it found as its reference.
/// </summary>
/// <typeparam name="TRole">The roles the members play.</typeparam>
/// <param name="ListedRoles">Every member's role, in the order the population lists the members.</param>
internal sealed record EnemyComposite<TRole>(IReadOnlyList<TRole> ListedRoles) where TRole : notnull;
