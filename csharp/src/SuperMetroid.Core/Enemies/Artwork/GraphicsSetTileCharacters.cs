using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// A graphics-set member's look as its room's load shows it (#1406, #1463): a look apart from
/// <see cref="EnemyTileCharacters"/>, which an encounter shows later. Where it lands is the import pipeline's knowledge
/// (#1513).
/// </summary>
/// <param name="Set">The graphics set whose load shows the look.</param>
/// <param name="Member">The member's place in the set's order.</param>
internal sealed record GraphicsSetTileCharacters(RoomEnemyGraphicsSetDefinition Set, int Member) : SpriteArtworkSource
{
    /// <summary>The member enemy whose look the set shows.</summary>
    internal EnemyDefinitionId Enemy => Set.Records.Span[Member].DefinitionId;

    /// <summary>Names the look for diagnostics and for the pictures of backgrounds that read it.</summary>
    public override string ToString() => $"{Enemy} set tiles";
}
