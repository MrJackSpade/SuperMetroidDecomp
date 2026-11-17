namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What one update of her corpse's rot produces: whether it still rots, and the dust its fallen rows raise.</summary>
[IndependentFlag("whether the corpse is still rotting after this step")]
public readonly record struct MotherBrainCorpseRottingStepResult(
    bool StillRotting,
    IReadOnlyList<MotherBrainCorpseDustRequest> DustRequests);
