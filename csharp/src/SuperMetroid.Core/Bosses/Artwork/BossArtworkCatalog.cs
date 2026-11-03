using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>
/// The bosses' installed artwork beside the shared enemy sheets (#1395) that gameplay reads: each boss's colors. Each
/// boss reads only its own entries. The characters and frames the bosses' pre-coloured pictures and body sheets are
/// composed from, Mother Brain's corpse characters among them, belong to the import pipeline's subclass (#1463, #1466).
/// </summary>
[ArchitectureReview("context-bag", "The bosses' installed artwork catalogs gameplay reads; its members are data, not services.")]
public abstract record BossArtworkCatalog
{
    /// <summary>Five editable Crocomire RGB5 transfer images; fight and death phases stay compiled.</summary>
    public CrocomireColorCatalog? CrocomireColors { get; init; }

    /// <summary>Installed Kraid RGB5 artwork; null only for constructed fixtures.</summary>
    public KraidColorCatalog? KraidColors { get; init; }

    /// <summary>Editable Mother Brain death-fade and exploded-door colors.</summary>
    public MotherBrainDeathColorCatalog? MotherBrainDeathColors { get; init; }

    /// <summary>Editable initial and fade-to-black colors for the Mother Brain cutscene Baby.</summary>
    public BabyMetroidCutsceneColorCatalog? BabyMetroidCutsceneColors { get; init; }

    /// <summary>Editable live-Shitroid normal-cycle and target sprite colors.</summary>
    public ShitroidColorCatalog? ShitroidColors { get; init; }
}
