using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>
/// The bosses' installed artwork beside the shared enemy sheets (#1395) that gameplay reads: Mother Brain's corpse
/// characters and each boss's colors. Each boss reads only its own entries. The characters and frames the bosses'
/// pre-coloured pictures and body sheets are composed from belong to the import pipeline's subclass (#1463, #1466).
/// </summary>
[ArchitectureReview("context-bag", "The bosses' installed artwork catalogs gameplay reads; its members are data, not services.")]
public abstract record BossArtworkCatalog
{
    /// <summary>Five editable Crocomire RGB5 transfer images; fight and death phases stay compiled.</summary>
    public CrocomireColorCatalog? CrocomireColors { get; init; }

    /// <summary>Editable Phantoon health, fade-out and Wrecked Ship power-on colors.</summary>
    public PhantoonColorCatalog? PhantoonColors { get; init; }

    /// <summary>Editable Draygon opening, normal, flash and health-band colors.</summary>
    public DraygonColorCatalog? DraygonColors { get; init; }

    /// <summary>Installed Kraid RGB5 artwork; null only for constructed fixtures.</summary>
    public KraidColorCatalog? KraidColors { get; init; }

    /// <summary>Editable source tile sheet for Mother Brain's corpse-rotting WRAM staging.</summary>
    public RoomCharacterAtlas? MotherBrainCorpse { get; init; }

    /// <summary>Editable Mother Brain death-fade and exploded-door colors.</summary>
    public MotherBrainDeathColorCatalog? MotherBrainDeathColors { get; init; }

    /// <summary>Editable initial and fade-to-black colors for the Mother Brain cutscene Baby.</summary>
    public BabyMetroidCutsceneColorCatalog? BabyMetroidCutsceneColors { get; init; }

    /// <summary>Editable live-Shitroid normal-cycle and target sprite colors.</summary>
    public ShitroidColorCatalog? ShitroidColors { get; init; }

    /// <summary>Editable Tourian Zebetite two-color pulse; actor selection stays in code.</summary>
    public ZebetiteColorCatalog? ZebetiteColors { get; init; }

    /// <summary>Editable Spore Spawn spore, health and death-sequence colors.</summary>
    public SporeSpawnColorCatalog? SporeSpawnColors { get; init; }

    /// <summary>Editable Botwoon sprite colors for all eight health bands.</summary>
    public BotwoonColorCatalog? BotwoonColors { get; init; }

    /// <summary>Editable initial and arena-reveal colors; Ridley's fade cadence stays in code.</summary>
    public NorfairRidleyColorCatalog? NorfairRidleyColors { get; init; }

    /// <summary>The selected boss presentation, computed rather than persisted in an exact debugger snapshot.</summary>
    public abstract string ContentIdentity { get; }
}
