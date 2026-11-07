namespace SuperMetroid.Core.Enemies;

/// <summary>
/// One enemy graphics set a room loads, by name: the installed sheets its enemies draw from are composed and named for
/// it (#1512, #1513). Which enemies it loads is the import's knowledge (#1527); two sets of one name are the same set.
/// </summary>
/// <param name="Name">The set's name, which its installed sheets are named by.</param>
public sealed record RoomEnemyGraphicsSetDefinition(string Name)
{
    /// <summary>The set's name.</summary>
    public string Name { get; } = Ensure.NotNull(Name);
}
