namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What one update of her corpse's rot produces: whether it still rots, and the dust its fallen rows raise.</summary>
public readonly partial record struct MotherBrainCorpseRottingStepResult(
    bool StillRotting,
    IReadOnlyList<MotherBrainCorpseDustRequest> DustRequests);
