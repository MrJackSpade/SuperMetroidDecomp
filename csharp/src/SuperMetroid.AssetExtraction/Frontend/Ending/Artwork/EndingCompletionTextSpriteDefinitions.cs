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
        new("operation-00", CinematicSpritePose.OperationText00, 2),
        new("operation-01", CinematicSpritePose.OperationText01, 4),
        new("operation-02", CinematicSpritePose.OperationText02, 6),
        new("operation-03", CinematicSpritePose.OperationText03, 8),
        new("operation-04", CinematicSpritePose.OperationText04, 10),
        new("operation-05", CinematicSpritePose.OperationText05, 12),
        new("operation-06", CinematicSpritePose.OperationText06, 14),
        new("operation-07", CinematicSpritePose.OperationText07, 16),
        new("operation-08", CinematicSpritePose.OperationText08, 18),
        new("operation-09", CinematicSpritePose.OperationText09, 20),
        new("operation-10", CinematicSpritePose.OperationText10, 22),
        new("operation-11", CinematicSpritePose.OperationText11, 24),
        new("operation-12", CinematicSpritePose.OperationText12, 26),
        new("operation-13", CinematicSpritePose.OperationText13, 28),
        new("operation-14", CinematicSpritePose.OperationText14, 30),
        new("completed-00", CinematicSpritePose.CompletedText00, 2),
        new("completed-01", CinematicSpritePose.CompletedText01, 4),
        new("completed-02", CinematicSpritePose.CompletedText02, 6),
        new("completed-03", CinematicSpritePose.CompletedText03, 8),
        new("completed-04", CinematicSpritePose.CompletedText04, 10),
        new("completed-05", CinematicSpritePose.CompletedText05, 12),
        new("completed-06", CinematicSpritePose.CompletedText06, 14),
        new("completed-07", CinematicSpritePose.CompletedText07, 16),
        new("completed-08", CinematicSpritePose.CompletedText08, 18),
        new("completed-09", CinematicSpritePose.CompletedText09, 20),
        new("completed-10", CinematicSpritePose.CompletedText10, 22),
        new("completed-11", CinematicSpritePose.CompletedText11, 24),
        new("completed-12", CinematicSpritePose.CompletedText12, 26),
        new("completed-13", CinematicSpritePose.CompletedText13, 28),
        new("completed-14", CinematicSpritePose.CompletedText14, 30),
        new("completed-15", CinematicSpritePose.CompletedText15, 32),
        new("completed-16", CinematicSpritePose.CompletedText16, 34),
        new("completed-17", CinematicSpritePose.CompletedText17, 36),
        new("completed-18", CinematicSpritePose.CompletedText18, 38),
        new("completed-19", CinematicSpritePose.CompletedText19, 40),
        new("completed-20", CinematicSpritePose.CompletedText20, 42),
        new("clear-time-00", CinematicSpritePose.ClearTimeText00, 2),
        new("clear-time-01", CinematicSpritePose.ClearTimeText01, 4),
        new("clear-time-02", CinematicSpritePose.ClearTimeText02, 6),
        new("clear-time-03", CinematicSpritePose.ClearTimeText03, 8),
        new("clear-time-04", CinematicSpritePose.ClearTimeText04, 10),
        new("clear-time-05", CinematicSpritePose.ClearTimeText05, 12),
        new("clear-time-06", CinematicSpritePose.ClearTimeText06, 14),
        new("clear-time-07", CinematicSpritePose.ClearTimeText07, 16),
        new("clear-time-08", CinematicSpritePose.ClearTimeText08, 18),
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
