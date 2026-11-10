using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// One enemy graphics set a room state's header names, by name (#1527, #1599): the tiles and colours the cartridge loads for
/// the room's enemies, which the import decodes once to compose and name the variant each population's enemies draw in.
/// The game knows no set. Two sets of one name are the same set.
/// </summary>
/// <param name="Name">The set's name.</param>
internal sealed record RoomEnemyGraphicsSetDefinition(string Name)
{
    /// <summary>The set's name.</summary>
    public string Name { get; } = Ensure.NotNull(Name);
}
