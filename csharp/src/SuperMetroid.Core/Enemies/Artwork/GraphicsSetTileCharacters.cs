namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// An enemy's installed tiles as its room's graphics set shows them over the OBJ characters while the room loads (#1406):
/// a source apart from <see cref="EnemyTileCharacters"/>, which an encounter shows over them later.
/// </summary>
/// <param name="Enemy">The member enemy whose tiles the set shows.</param>
internal sealed record GraphicsSetTileCharacters(EnemyDefinitionId Enemy) : ObjCharacterSource
{
    /// <inheritdoc/>
    /// <exception cref="InvalidDataException">The installed enemy artwork holds no tile set for the enemy.</exception>
    public override ReadOnlyMemory<byte> Resolve(IObjCharacterSourceResolver resolver) =>
        InstalledObjCharacterArtwork.Of(resolver).EnemyTiles.TilesOf(Enemy);

    /// <summary>Names the source for diagnostics and for the pictures of backgrounds that read it.</summary>
    public override string ToString() => $"{Enemy} set tiles";
}
