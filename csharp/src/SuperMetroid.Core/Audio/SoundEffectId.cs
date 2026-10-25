namespace SuperMetroid.Core.Audio;

/// <summary>
/// One sound effect the game plays: a member of <see cref="Library1Sound"/>, <see cref="Library2Sound"/> or
/// <see cref="Library3Sound"/>, together with the library whose voice queue plays it.
/// </summary>
/// <remarks>
/// The library is part of the identity because it decides which queue a request waits in and which sounds interrupt one
/// another. Each library's sounds convert implicitly, so callers name the sound itself. The identity holds no number:
/// the import pipeline alone maps a sound to its request in the installed sound program.
/// </remarks>
public readonly record struct SoundEffectId
{
    /// <summary>The library-one sound when <see cref="Library"/> is library one.</summary>
    private readonly Library1Sound library1;
    /// <summary>The library-two sound when <see cref="Library"/> is library two.</summary>
    private readonly Library2Sound library2;
    /// <summary>The library-three sound when <see cref="Library"/> is library three.</summary>
    private readonly Library3Sound library3;

    /// <summary>Records one library's sound; the other libraries' fields keep their defaults.</summary>
    private SoundEffectId(SoundEffectLibrary library, Library1Sound library1, Library2Sound library2, Library3Sound library3)
    {
        Library = library;
        this.library1 = library1;
        this.library2 = library2;
        this.library3 = library3;
    }

    /// <summary>The library whose voice queue plays the sound.</summary>
    public SoundEffectLibrary Library { get; }

    /// <summary>A library-one sound.</summary>
    public static implicit operator SoundEffectId(Library1Sound sound) =>
        new(SoundEffectLibrary.Library1, sound, default, default);

    /// <summary>A library-two sound.</summary>
    public static implicit operator SoundEffectId(Library2Sound sound) =>
        new(SoundEffectLibrary.Library2, default, sound, default);

    /// <summary>A library-three sound.</summary>
    public static implicit operator SoundEffectId(Library3Sound sound) =>
        new(SoundEffectLibrary.Library3, default, default, sound);

    /// <summary>Applies the function for the sound's own library to the sound.</summary>
    /// <exception cref="InvalidOperationException">The identity is the default value, which names no sound.</exception>
    public TResult Match<TResult>(
        Func<Library1Sound, TResult> library1Sound,
        Func<Library2Sound, TResult> library2Sound,
        Func<Library3Sound, TResult> library3Sound)
    {
        Ensure.NotNull(library1Sound);
        Ensure.NotNull(library2Sound);
        Ensure.NotNull(library3Sound);
        return Library switch
        {
            SoundEffectLibrary.Library1 => library1Sound(library1),
            SoundEffectLibrary.Library2 => library2Sound(library2),
            SoundEffectLibrary.Library3 => library3Sound(library3),
            _ => throw new InvalidOperationException("The default sound identity names no sound."),
        };
    }

    /// <inheritdoc />
    public override string ToString() => Library switch
    {
        SoundEffectLibrary.Library1 => $"{nameof(Library1Sound)}.{library1}",
        SoundEffectLibrary.Library2 => $"{nameof(Library2Sound)}.{library2}",
        SoundEffectLibrary.Library3 => $"{nameof(Library3Sound)}.{library3}",
        _ => throw new InvalidOperationException("The default sound identity names no sound."),
    };
}
