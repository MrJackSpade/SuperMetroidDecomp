using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// The compositions shown by the fourteen fixed bank-$8B ending completion-text lists. Counts preserve the cartridge's
/// progressively revealed two-part-per-letter compositions without treating those visuals as timing data.
/// </summary>
public static class EndingCompletionTextSpriteDefinitions
{
    /// <summary>
    /// Fifteen THE OPERATION WAS prefixes, 21 COMPLETED SUCCESSFULLY prefixes and nine CLEAR TIME prefixes, each revealing
    /// one more letter, then the ten two-part digits and the two-part colon.
    /// </summary>
    public static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("operation-00", CinematicSpritePose.OperationTypedT, 2),
        new("operation-01", CinematicSpritePose.OperationTypedTh, 4),
        new("operation-02", CinematicSpritePose.OperationTypedThe, 6),
        new("operation-03", CinematicSpritePose.OperationTypedTheO, 8),
        new("operation-04", CinematicSpritePose.OperationTypedTheOp, 10),
        new("operation-05", CinematicSpritePose.OperationTypedTheOpe, 12),
        new("operation-06", CinematicSpritePose.OperationTypedTheOper, 14),
        new("operation-07", CinematicSpritePose.OperationTypedTheOpera, 16),
        new("operation-08", CinematicSpritePose.OperationTypedTheOperat, 18),
        new("operation-09", CinematicSpritePose.OperationTypedTheOperati, 20),
        new("operation-10", CinematicSpritePose.OperationTypedTheOperatio, 22),
        new("operation-11", CinematicSpritePose.OperationTypedTheOperation, 24),
        new("operation-12", CinematicSpritePose.OperationTypedTheOperationW, 26),
        new("operation-13", CinematicSpritePose.OperationTypedTheOperationWa, 28),
        new("operation-14", CinematicSpritePose.OperationTypedTheOperationWas, 30),
        new("completed-00", CinematicSpritePose.CompletedTypedC, 2),
        new("completed-01", CinematicSpritePose.CompletedTypedCo, 4),
        new("completed-02", CinematicSpritePose.CompletedTypedCom, 6),
        new("completed-03", CinematicSpritePose.CompletedTypedComp, 8),
        new("completed-04", CinematicSpritePose.CompletedTypedCompl, 10),
        new("completed-05", CinematicSpritePose.CompletedTypedComple, 12),
        new("completed-06", CinematicSpritePose.CompletedTypedComplet, 14),
        new("completed-07", CinematicSpritePose.CompletedTypedComplete, 16),
        new("completed-08", CinematicSpritePose.CompletedTypedCompleted, 18),
        new("completed-09", CinematicSpritePose.CompletedTypedCompletedS, 20),
        new("completed-10", CinematicSpritePose.CompletedTypedCompletedSu, 22),
        new("completed-11", CinematicSpritePose.CompletedTypedCompletedSuc, 24),
        new("completed-12", CinematicSpritePose.CompletedTypedCompletedSucc, 26),
        new("completed-13", CinematicSpritePose.CompletedTypedCompletedSucce, 28),
        new("completed-14", CinematicSpritePose.CompletedTypedCompletedSucces, 30),
        new("completed-15", CinematicSpritePose.CompletedTypedCompletedSuccess, 32),
        new("completed-16", CinematicSpritePose.CompletedTypedCompletedSuccessf, 34),
        new("completed-17", CinematicSpritePose.CompletedTypedCompletedSuccessfu, 36),
        new("completed-18", CinematicSpritePose.CompletedTypedCompletedSuccessful, 38),
        new("completed-19", CinematicSpritePose.CompletedTypedCompletedSuccessfull, 40),
        new("completed-20", CinematicSpritePose.CompletedTypedCompletedSuccessfully, 42),
        new("clear-time-00", CinematicSpritePose.ClearTimeTypedC, 2),
        new("clear-time-01", CinematicSpritePose.ClearTimeTypedCl, 4),
        new("clear-time-02", CinematicSpritePose.ClearTimeTypedCle, 6),
        new("clear-time-03", CinematicSpritePose.ClearTimeTypedClea, 8),
        new("clear-time-04", CinematicSpritePose.ClearTimeTypedClear, 10),
        new("clear-time-05", CinematicSpritePose.ClearTimeTypedClearT, 12),
        new("clear-time-06", CinematicSpritePose.ClearTimeTypedClearTi, 14),
        new("clear-time-07", CinematicSpritePose.ClearTimeTypedClearTim, 16),
        new("clear-time-08", CinematicSpritePose.ClearTimeTypedClearTime, 18),
        new("digit-00", CinematicSpritePose.ClearTimeDigit0, 2),
        new("digit-01", CinematicSpritePose.ClearTimeDigit1, 2),
        new("digit-02", CinematicSpritePose.ClearTimeDigit2, 2),
        new("digit-03", CinematicSpritePose.ClearTimeDigit3, 2),
        new("digit-04", CinematicSpritePose.ClearTimeDigit4, 2),
        new("digit-05", CinematicSpritePose.ClearTimeDigit5, 2),
        new("digit-06", CinematicSpritePose.ClearTimeDigit6, 2),
        new("digit-07", CinematicSpritePose.ClearTimeDigit7, 2),
        new("digit-08", CinematicSpritePose.ClearTimeDigit8, 2),
        new("digit-09", CinematicSpritePose.ClearTimeDigit9, 2),
        new("colon-00", CinematicSpritePose.ClearTimeColon, 2),
    ];
}
