using SuperMetroid.Core.Assets;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The uploads each encounter makes over the OBJ characters after its room loads, the regions a graphics set's uploads
/// divide the characters into, and the picture of a frame drawn over them that shows (#1392). The encounters' own module
/// supplies the uploads; the installed sheets carry the catalogue to the enemies drawing from them.
/// </summary>
public sealed class EncounterCharacterCatalog
{
    /// <summary>Each enemy's encounter's uploads, by the enemy whose graphics set loads the encounter.</summary>
    internal readonly IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyList<SpriteArtworkPlacement>> byEnemy;

    /// <summary>Every catalogued upload, once, in a fixed order whose index names it in picture names.</summary>
    internal readonly ImmutableArray<SpriteArtworkPlacement> all;

    /// <summary>Catalogues each enemy's encounter's uploads, by the enemy whose graphics set loads the encounter.</summary>
    public EncounterCharacterCatalog(IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyList<SpriteArtworkPlacement>> byEnemy) =>
        (this.byEnemy, all) = (Ensure.NotNull(byEnemy), EncounterUploads.InCatalogueOrder(byEnemy));

    /// <summary>The catalogued placement that shows <paramref name="source"/>.</summary>
    /// <exception cref="InvalidDataException">No catalogued encounter shows the source, or several place it differently.</exception>
    public SpriteArtworkPlacement Placement(SpriteArtworkSource source) => EncounterUploads.Placement(this, source);

    /// <summary>The uploads the encounters of graphics set <paramref name="set"/>'s members make, in catalogue order.</summary>
    public IReadOnlyList<SpriteArtworkPlacement> Of(RoomEnemyGraphicsSetDefinition set) => EncounterUploads.Of(this, set);

    /// <summary>
    /// The name of the picture of layer <paramref name="layer"/> of <paramref name="frame"/> whose regions each show what
    /// <paramref name="shown"/> pairs with it.
    /// </summary>
    public string PictureName(string frame, int layer, IReadOnlyList<CharacterRegionContent> shown) =>
        EncounterUploads.PictureName(this, frame, layer, shown);

    /// <summary>
    /// What shows over <paramref name="region"/> now: the frame an animation the set's encounters paint the region with
    /// shows in <paramref name="frames"/> (#1406); else, of the sources <paramref name="coverage"/> says cover it, the upload
    /// among <paramref name="setUploads"/> shown last, or none while the graphics set's own characters, which the room load
    /// showed, or the standard sprites show there.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// An upload the set's encounters do not make shows over its characters, or an animation shows a frame the set's
    /// encounters do not paint the region with.
    /// </exception>
    public CharacterRegionContent Showing(SpriteArtworkPlacements uploads, IReadOnlyList<SpriteArtworkPlacement> setUploads,
        CharacterRegion region, ArtworkFrameSelections frames, SetCharacterCoverage coverage) =>
        EncounterUploads.Showing(this, uploads, setUploads, region, frames, coverage);
}
