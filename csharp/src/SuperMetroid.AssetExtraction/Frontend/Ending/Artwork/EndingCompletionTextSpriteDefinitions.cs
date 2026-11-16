using SuperMetroid.Core.Frontend.Ending;
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
    /// Fifteen THE OPERATION WAS steps, 21 COMPLETED SUCCESSFULLY steps and nine CLEAR TIME steps, each revealing one more
    /// letter (#2195), from which the import pictures each letter once.
    /// </summary>
    public static IReadOnlyList<EndingTypedStepDefinition> Typings { get; } =
    [
        new("operation-00", new(EndingTypedText.OperationWas, 1), 2),
        new("operation-01", new(EndingTypedText.OperationWas, 2), 4),
        new("operation-02", new(EndingTypedText.OperationWas, 3), 6),
        new("operation-03", new(EndingTypedText.OperationWas, 4), 8),
        new("operation-04", new(EndingTypedText.OperationWas, 5), 10),
        new("operation-05", new(EndingTypedText.OperationWas, 6), 12),
        new("operation-06", new(EndingTypedText.OperationWas, 7), 14),
        new("operation-07", new(EndingTypedText.OperationWas, 8), 16),
        new("operation-08", new(EndingTypedText.OperationWas, 9), 18),
        new("operation-09", new(EndingTypedText.OperationWas, 10), 20),
        new("operation-10", new(EndingTypedText.OperationWas, 11), 22),
        new("operation-11", new(EndingTypedText.OperationWas, 12), 24),
        new("operation-12", new(EndingTypedText.OperationWas, 13), 26),
        new("operation-13", new(EndingTypedText.OperationWas, 14), 28),
        new("operation-14", new(EndingTypedText.OperationWas, 15), 30),
        new("completed-00", new(EndingTypedText.CompletedSuccessfully, 1), 2),
        new("completed-01", new(EndingTypedText.CompletedSuccessfully, 2), 4),
        new("completed-02", new(EndingTypedText.CompletedSuccessfully, 3), 6),
        new("completed-03", new(EndingTypedText.CompletedSuccessfully, 4), 8),
        new("completed-04", new(EndingTypedText.CompletedSuccessfully, 5), 10),
        new("completed-05", new(EndingTypedText.CompletedSuccessfully, 6), 12),
        new("completed-06", new(EndingTypedText.CompletedSuccessfully, 7), 14),
        new("completed-07", new(EndingTypedText.CompletedSuccessfully, 8), 16),
        new("completed-08", new(EndingTypedText.CompletedSuccessfully, 9), 18),
        new("completed-09", new(EndingTypedText.CompletedSuccessfully, 10), 20),
        new("completed-10", new(EndingTypedText.CompletedSuccessfully, 11), 22),
        new("completed-11", new(EndingTypedText.CompletedSuccessfully, 12), 24),
        new("completed-12", new(EndingTypedText.CompletedSuccessfully, 13), 26),
        new("completed-13", new(EndingTypedText.CompletedSuccessfully, 14), 28),
        new("completed-14", new(EndingTypedText.CompletedSuccessfully, 15), 30),
        new("completed-15", new(EndingTypedText.CompletedSuccessfully, 16), 32),
        new("completed-16", new(EndingTypedText.CompletedSuccessfully, 17), 34),
        new("completed-17", new(EndingTypedText.CompletedSuccessfully, 18), 36),
        new("completed-18", new(EndingTypedText.CompletedSuccessfully, 19), 38),
        new("completed-19", new(EndingTypedText.CompletedSuccessfully, 20), 40),
        new("completed-20", new(EndingTypedText.CompletedSuccessfully, 21), 42),
        new("clear-time-00", new(EndingTypedText.ClearTime, 1), 2),
        new("clear-time-01", new(EndingTypedText.ClearTime, 2), 4),
        new("clear-time-02", new(EndingTypedText.ClearTime, 3), 6),
        new("clear-time-03", new(EndingTypedText.ClearTime, 4), 8),
        new("clear-time-04", new(EndingTypedText.ClearTime, 5), 10),
        new("clear-time-05", new(EndingTypedText.ClearTime, 6), 12),
        new("clear-time-06", new(EndingTypedText.ClearTime, 7), 14),
        new("clear-time-07", new(EndingTypedText.ClearTime, 8), 16),
        new("clear-time-08", new(EndingTypedText.ClearTime, 9), 18),
    ];

    /// <summary>The ten two-part digits and the two-part colon.</summary>
    public static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
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
