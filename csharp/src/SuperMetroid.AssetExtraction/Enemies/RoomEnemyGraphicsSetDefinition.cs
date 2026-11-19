using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// One enemy graphics set a room state's header names (#1527, #1599): the tiles and colours the cartridge loads for the
/// room's enemies, which the import decodes once to compose and name the variant each population's enemies draw in. The
/// game knows no set. Each set is its own identity (#2548): the import's tables pair with the set itself, and its name is
/// only the text its diagnostics show.
/// </summary>
/// <param name="name">The set's name, written out where the set is declared.</param>
internal sealed class RoomEnemyGraphicsSetDefinition(string name)
{
    /// <summary>The set's name, written out where the set is declared.</summary>
    public string Name { get; } = Ensure.NotNull(name);

    /// <inheritdoc cref="Name"/>
    public override string ToString() => Name;
}
