namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Observable work produced by one shared corpse-rotting processor call.</summary>
public readonly record struct MotherBrainCorpseRottingStepResult(
    bool StillRotting,
    IReadOnlyList<MotherBrainSpriteTileTransferRequest> VramTransfers,
    IReadOnlyList<MotherBrainCorpseDustRequest> DustRequests);
