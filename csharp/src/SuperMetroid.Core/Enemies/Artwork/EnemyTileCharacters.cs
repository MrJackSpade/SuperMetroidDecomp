namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>An enemy's installed tile set, shown in the OBJ character table outside its room's population load.</summary>
/// <param name="Enemy">The enemy whose tile set is shown.</param>
internal sealed record EnemyTileCharacters(EnemyDefinitionId Enemy) : ObjCharacterSource
{
    /// <inheritdoc/>
    /// <exception cref="InvalidDataException">The installed enemy artwork holds no tile set for the enemy.</exception>
    public override ReadOnlyMemory<byte> Resolve(IObjCharacterSourceResolver resolver)
    {
        RoomEnemyDefinition definition = RoomEnemyDefinitionCatalog.Get(Enemy);
        return InstalledObjCharacterArtwork.Of(resolver).EnemyTiles.TryResolve(definition.TileDataAddress,
            definition.TileDataSize & short.MaxValue, out ReadOnlyMemory<byte> tiles)
            ? tiles
            : throw new InvalidDataException($"Installed enemy artwork holds no {Enemy} tile set.");
    }

    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"{Enemy} tiles";
}
