using System.Collections.Concurrent;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The OBJ pages Samus's projectiles, bombs, charge flare and trails draw from: a base OBJ page with the selected
/// beam's characters and the trail characters where their uploads place them. The equipped beams choose which page is
/// bound; no characters are copied into VRAM.
/// </summary>
public sealed class ProjectileObjectPages
{
    /// <summary>The page the beam and trail characters are placed over.</summary>
    private readonly SpriteArtwork basePage;
    /// <summary>Installed beam characters by combination.</summary>
    private readonly BeamTileCatalog beams;
    /// <summary>Installed ice/wave and missile trail characters.</summary>
    private readonly ProjectileTrailAtlas trails;
    /// <summary>Pages already composed, by beam combination.</summary>
    private readonly ConcurrentDictionary<SamusBeamCombination, SpriteArtwork> pages = new();

    /// <summary>Binds the base page and the installed beam and trail characters.</summary>
    /// <param name="basePage">The context's OBJ page: the standard sheet in gameplay, the intro's sheets in the intro.</param>
    /// <param name="beams">Installed beam characters.</param>
    /// <param name="trails">Installed trail characters.</param>
    public ProjectileObjectPages(SpriteArtwork basePage, BeamTileCatalog beams, ProjectileTrailAtlas trails)
    {
        this.basePage = Ensure.NotNull(basePage);
        this.beams = Ensure.NotNull(beams);
        this.trails = Ensure.NotNull(trails);
    }

    /// <summary>The page bound for an equipped-beam word.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The word selects no beam combination (bits 4-11 set).</exception>
    public SpriteArtwork For(SuperMetroid.Core.Progression.SamusBeamFlags equippedBeams) =>
        For(new SamusBeamLoadoutWord(equippedBeams).Combination ?? throw new ArgumentOutOfRangeException(
            nameof(equippedBeams), equippedBeams, "Equipped-beam index is outside the beam-combination tables."));

    /// <summary>The page bound for a beam combination.</summary>
    public SpriteArtwork For(SamusBeamCombination beam) => pages.GetOrAdd(beam, Compose);

    /// <summary>Places the combination's beam characters and both trail sheets over the base page.</summary>
    private SpriteArtwork Compose(SamusBeamCombination beam)
    {
        var id = new SpriteArtworkId(SpriteArtworkKind.ProjectileObjects, (int)beam);
        SpriteArtwork page = ObjCharacterPage.With(id, basePage, beams.Resolve(BeamTileCatalog.AssetFor(beam)).Span,
            ProjectileObjectPageDefinitions.BeamFirstCharacter);
        page = ObjCharacterPage.With(id, page, trails.IceAndWave.Span, ProjectileObjectPageDefinitions.IceWaveTrailFirstCharacter);
        return ObjCharacterPage.With(id, page, trails.Missile.Span, ProjectileObjectPageDefinitions.MissileTrailFirstCharacter);
    }
}
