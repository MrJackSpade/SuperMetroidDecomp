using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending's typed texts' spellings and typing cadence (#1513, #2195).</summary>
internal static partial class EndingTypedTextDefinitions
{
    /// <summary>Each text as it is spelled, a single space between its parts.</summary>
    internal static IReadOnlyDictionary<EndingTypedText, string> Spellings { get; } = new Dictionary<EndingTypedText, string>
    {
        [EndingTypedText.OperationWas] = "THE OPERATION WAS",
        [EndingTypedText.CompletedSuccessfully] = "COMPLETED SUCCESSFULLY",
        [EndingTypedText.ClearTime] = "CLEAR TIME",
    };

    /// <summary>The updates each letter shows before the next is typed.</summary>
    internal const int LetterUpdates = 8;

    /// <summary>
    /// The updates the last letter before a space or the text's end shows before the text types on: the pause the space
    /// is typed in.
    /// </summary>
    internal const int LastLetterUpdates = 15;

    /// <summary>
    /// The clear time's columns in the order CLEAR TIME spawns them, each by the cue that spawns it (#2557): the hours' tens and
    /// units, the colon, and the minutes' tens and units.
    /// </summary>
    internal static IReadOnlyList<EndingSceneCue> ClearTimeColumns { get; } =
    [
        EndingSceneCue.SpawnHoursTens,
        EndingSceneCue.SpawnHoursUnits,
        EndingSceneCue.SpawnColon,
        EndingSceneCue.SpawnMinutesTens,
        EndingSceneCue.SpawnMinutesUnits,
    ];

    /// <summary>The updates CLEAR TIME shows between spawning each column of the clear time.</summary>
    internal const int ClearTimeColumnUpdates = 8;

    /// <summary>The updates the whole clear time shows before the ending moves on to the credits.</summary>
    internal const int ClearTimeShownUpdates = 128;
}
