namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The installed artwork an encounter's looks copy from: the named installed content, and the enemy, boss and area
/// characters the looks' pages are cut from.
/// </summary>
internal interface IInstalledEncounterArtwork
    : IInstalledArtworkContent, IInstalledSpriteCharacterArtwork, IInstalledBossSpriteCharacterArtwork, IInstalledAreaSpriteCharacterArtwork;
