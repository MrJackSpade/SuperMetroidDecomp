using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// The compositions shown by the fourteen fixed bank-$8B ending completion-text lists. Counts preserve the cartridge's
/// progressively revealed two-part-per-letter compositions without treating those visuals as timing data.
/// </summary>
public static partial class EndingCompletionTextSpriteDefinitions
{
    /// <summary>
    /// Fifteen THE OPERATION WAS steps, 21 COMPLETED SUCCESSFULLY steps and nine CLEAR TIME steps, each revealing one more
    /// letter (#2195), from which the import pictures each letter once.
    /// </summary>
    public static IReadOnlyList<EndingTypedStepDefinition> Typings { get; } =
    [
        new(new(EndingTypedText.OperationWas, 1), 2),
        new(new(EndingTypedText.OperationWas, 2), 4),
        new(new(EndingTypedText.OperationWas, 3), 6),
        new(new(EndingTypedText.OperationWas, 4), 8),
        new(new(EndingTypedText.OperationWas, 5), 10),
        new(new(EndingTypedText.OperationWas, 6), 12),
        new(new(EndingTypedText.OperationWas, 7), 14),
        new(new(EndingTypedText.OperationWas, 8), 16),
        new(new(EndingTypedText.OperationWas, 9), 18),
        new(new(EndingTypedText.OperationWas, 10), 20),
        new(new(EndingTypedText.OperationWas, 11), 22),
        new(new(EndingTypedText.OperationWas, 12), 24),
        new(new(EndingTypedText.OperationWas, 13), 26),
        new(new(EndingTypedText.OperationWas, 14), 28),
        new(new(EndingTypedText.OperationWas, 15), 30),
        new(new(EndingTypedText.CompletedSuccessfully, 1), 2),
        new(new(EndingTypedText.CompletedSuccessfully, 2), 4),
        new(new(EndingTypedText.CompletedSuccessfully, 3), 6),
        new(new(EndingTypedText.CompletedSuccessfully, 4), 8),
        new(new(EndingTypedText.CompletedSuccessfully, 5), 10),
        new(new(EndingTypedText.CompletedSuccessfully, 6), 12),
        new(new(EndingTypedText.CompletedSuccessfully, 7), 14),
        new(new(EndingTypedText.CompletedSuccessfully, 8), 16),
        new(new(EndingTypedText.CompletedSuccessfully, 9), 18),
        new(new(EndingTypedText.CompletedSuccessfully, 10), 20),
        new(new(EndingTypedText.CompletedSuccessfully, 11), 22),
        new(new(EndingTypedText.CompletedSuccessfully, 12), 24),
        new(new(EndingTypedText.CompletedSuccessfully, 13), 26),
        new(new(EndingTypedText.CompletedSuccessfully, 14), 28),
        new(new(EndingTypedText.CompletedSuccessfully, 15), 30),
        new(new(EndingTypedText.CompletedSuccessfully, 16), 32),
        new(new(EndingTypedText.CompletedSuccessfully, 17), 34),
        new(new(EndingTypedText.CompletedSuccessfully, 18), 36),
        new(new(EndingTypedText.CompletedSuccessfully, 19), 38),
        new(new(EndingTypedText.CompletedSuccessfully, 20), 40),
        new(new(EndingTypedText.CompletedSuccessfully, 21), 42),
        new(new(EndingTypedText.ClearTime, 1), 2),
        new(new(EndingTypedText.ClearTime, 2), 4),
        new(new(EndingTypedText.ClearTime, 3), 6),
        new(new(EndingTypedText.ClearTime, 4), 8),
        new(new(EndingTypedText.ClearTime, 5), 10),
        new(new(EndingTypedText.ClearTime, 6), 12),
        new(new(EndingTypedText.ClearTime, 7), 14),
        new(new(EndingTypedText.ClearTime, 8), 16),
        new(new(EndingTypedText.ClearTime, 9), 18),
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
        new("colon", CinematicSpritePose.ClearTimeColon, 2),
    ];
}
