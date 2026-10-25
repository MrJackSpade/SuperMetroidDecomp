namespace SuperMetroid.Core.Audio;

/// <summary>
/// One track of the loaded music data set. A data set holds a handful of tracks numbered from one, the numbering room
/// headers select tracks by and the clip catalog keys each rendered track by (<see cref="MusicTrackKey"/>); playing a
/// track needs only its number because the data set it belongs to is the one loaded.
/// </summary>
public readonly record struct MusicTrack
{
    /// <summary>The highest track number a data set can hold.</summary>
    public const int LastNumber = 127;

    /// <summary>Names track <paramref name="number"/> of the loaded data set.</summary>
    /// <param name="number">The track's number within its data set, 1 through <see cref="LastNumber"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> is outside 1 through <see cref="LastNumber"/>.</exception>
    public MusicTrack(int number) => Number = Ensure.BetweenInclusive(1, LastNumber, number);

    /// <summary>The track's number within its data set, 1 through <see cref="LastNumber"/>.</summary>
    public int Number { get; }

    /// <inheritdoc />
    public override string ToString() => $"track {Number}";
}
