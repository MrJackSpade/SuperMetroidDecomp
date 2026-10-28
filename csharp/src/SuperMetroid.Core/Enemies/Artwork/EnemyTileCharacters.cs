namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>An enemy's installed tile set, shown in the OBJ character table outside its room's population load.</summary>
/// <param name="Enemy">The enemy whose tile set is shown.</param>
internal sealed record EnemyTileCharacters(EnemyDefinitionId Enemy) : SpriteArtworkSource
{
    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"{Enemy} tiles";
}
