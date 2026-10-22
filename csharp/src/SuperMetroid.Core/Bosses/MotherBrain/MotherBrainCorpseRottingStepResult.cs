namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Observable work produced by one shared corpse-rotting processor call.</summary>
[IndependentFlag("whether the corpse is still rotting after this step")]
public readonly record struct MotherBrainCorpseRottingStepResult(
    bool StillRotting,
    IReadOnlyList<MotherBrainCorpseRotUpload> VramTransfers,
    IReadOnlyList<MotherBrainCorpseDustRequest> DustRequests);
