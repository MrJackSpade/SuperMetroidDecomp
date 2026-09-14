using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    [NonSerialized] private ProjectileSpriteCatalog? projectileCompositions;
    [NonSerialized] private BeamTileCatalog? beamArtwork;

    /// <summary>Supplies current beam PNG content across runtime creation and state restoration.</summary>
    public void BindBeamArtwork(BeamTileCatalog? catalog)
    {
        beamArtwork = catalog;
        if (runtime is not null) runtime.BeamArtwork = catalog;
    }

    /// <summary>Attaches current host content to an existing runtime and future game/demo runtimes.</summary>
    public void BindProjectileCompositions(ProjectileSpriteCatalog? catalog)
    {
        projectileCompositions = catalog;
        if (runtime is not null) runtime.ProjectileCompositions = catalog;
    }
}
