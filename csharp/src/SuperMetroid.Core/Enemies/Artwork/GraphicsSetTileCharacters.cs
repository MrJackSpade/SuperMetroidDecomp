using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// A graphics-set member's look as its room's load shows it (#1406, #1463): a look apart from
/// <see cref="EnemyTileCharacters"/>, which an encounter shows later. Two sets that load the same member alike show the
/// same look; where it lands is the import pipeline's knowledge.
/// </summary>
/// <param name="Entry">The member as its room's graphics set loads it.</param>
internal sealed record GraphicsSetTileCharacters(RoomEnemyGraphicsSetEntry Entry) : SpriteArtworkSource
{
    /// <summary>The member enemy whose look the set shows.</summary>
    internal EnemyDefinitionId Enemy => Entry.DefinitionId;

    /// <summary>Names the look for diagnostics and for the pictures of backgrounds that read it.</summary>
    public override string ToString() => $"{Enemy} set tiles";
}
