using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    // Host content is rebound after restoring a graph; saved state must not freeze
    // an old user override into the simulation. Only composition emission uses this.
    [NonSerialized] private ProjectileSpriteCatalog? projectileCompositions;

    /// <summary>Current external timed-projectile composition catalog, independent of mechanics.</summary>
    public ProjectileSpriteCatalog? ProjectileCompositions
    {
        get => projectileCompositions;
        set => projectileCompositions = value;
    }
}
