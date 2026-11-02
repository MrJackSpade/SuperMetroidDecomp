using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The enemy graphics sets the import walks (#1502): every retail set to source each enemy's tiles from, and every set
/// rooms draw under to compose a sheet for. The game only loads the set a room names.
/// </summary>
internal static class RoomEnemyGraphicsSetLists
{
    /// <summary>Every retail graphics set, in the game's list order.</summary>
    internal static IReadOnlyList<RoomEnemyGraphicsSetDefinition> All => RoomEnemyGraphicsSetDefinitions.Retail;

    /// <summary>Every graphics set rooms draw under: each retail set, then the session start's.</summary>
    internal static IReadOnlyList<RoomEnemyGraphicsSetDefinition> DrawnUnder { get; } =
        [.. RoomEnemyGraphicsSetDefinitions.Retail, RoomEnemyGraphicsSetDefinitions.SessionStart];
}
