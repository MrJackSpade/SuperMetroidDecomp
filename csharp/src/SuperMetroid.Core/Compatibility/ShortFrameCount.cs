namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where the game's short frame count stands in its period (#2550). <see cref="FrameClock"/> owns it: it counts every frame
/// the game shows, returns to zero after <see cref="Period"/> frames, and a door's scroll restarts it. Enemy cadences, the
/// pause screen's animations and the minimap's blink read where it stands, so its return to zero is gameplay-observable.
/// </summary>
internal readonly record struct ShortFrameCount
{
    /// <summary>How many frames the count counts before it returns to zero.</summary>
    internal const int Period = 256;

    /// <summary>A count standing <paramref name="frames"/> into its period.</summary>
    /// <param name="frames">How far into its period the count stands, from zero up to one less than <see cref="Period"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frames"/> lies outside the period.</exception>
    internal ShortFrameCount(int frames) => Frames = Ensure.BetweenInclusive(0, Period - 1, frames);

    /// <summary>The count power-on, a soft reset and a door's scroll leave.</summary>
    internal static ShortFrameCount Zero => default;

    /// <summary>How far into its period the count stands.</summary>
    internal int Frames { get; }

    /// <summary>The count <paramref name="frames"/> frames later, returned to zero at the end of each period.</summary>
    /// <param name="frames">How many frames pass; never negative.</param>
    internal ShortFrameCount After(int frames) => new((Frames + Ensure.AtLeastZero(frames) % Period) % Period);

    /// <summary>How far into its period the count stands, for the cadences that read it.</summary>
    internal int ToInt32() => Frames;

    /// <summary>How far into its period <paramref name="count"/> stands, for the cadences that read it.</summary>
    public static implicit operator int(ShortFrameCount count) => count.Frames;
}
