namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The installed artwork an encounter's looks are composed from (#2554): its typed assets and the enemy, boss and area
/// characters, all of which an encounter's composition reads, so none is reached by a cast that can fail.
/// </summary>
internal interface IInstalledEncounterArtwork : IInstalledArtworkContent, IInstalledSpriteCharacterArtwork,
    IInstalledBossSpriteCharacterArtwork, IInstalledAreaSpriteCharacterArtwork, IInstalledAnimatedTileArtwork;
