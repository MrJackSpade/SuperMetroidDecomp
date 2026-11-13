namespace SuperMetroid.Core.Audio;

/// <summary>
/// One sound effect the game plays: a member of <see cref="SamusSound"/>, <see cref="EnemySound"/> or
/// <see cref="RoomSound"/>, together with the library whose voice queue plays it.
/// </summary>
/// <remarks>
/// The library is part of the identity because it decides which queue a request waits in and which sounds interrupt one
/// another. Each library's sounds convert implicitly, so callers name the sound itself. The identity holds no number:
/// the import pipeline alone maps a sound to the installed sound effect it requests.
/// </remarks>
public readonly record struct SoundEffectId
{
    /// <summary>Samus's sound when <see cref="Library"/> is Samus's library.</summary>
    private readonly SamusSound samus;
    /// <summary>The enemies' sound when <see cref="Library"/> is the enemies' library.</summary>
    private readonly EnemySound enemies;
    /// <summary>The room's sound when <see cref="Library"/> is the room's library.</summary>
    private readonly RoomSound room;

    /// <summary>Records one library's sound; the other libraries' fields keep their defaults.</summary>
    private SoundEffectId(SoundEffectLibrary library, SamusSound samus, EnemySound enemies, RoomSound room)
    {
        Library = library;
        this.samus = samus;
        this.enemies = enemies;
        this.room = room;
    }

    /// <summary>The library whose voice queue plays the sound.</summary>
    public SoundEffectLibrary Library { get; }

    /// <summary>A sound of Samus's library.</summary>
    public static implicit operator SoundEffectId(SamusSound sound) =>
        new(SoundEffectLibrary.Samus, sound, default, default);

    /// <summary>A sound of the enemies' library.</summary>
    public static implicit operator SoundEffectId(EnemySound sound) =>
        new(SoundEffectLibrary.Enemies, default, sound, default);

    /// <summary>A sound of the room's library.</summary>
    public static implicit operator SoundEffectId(RoomSound sound) =>
        new(SoundEffectLibrary.Room, default, default, sound);

    /// <inheritdoc />
    public override string ToString() => Library switch
    {
        SoundEffectLibrary.Samus => $"{nameof(SamusSound)}.{samus}",
        SoundEffectLibrary.Enemies => $"{nameof(EnemySound)}.{enemies}",
        SoundEffectLibrary.Room => $"{nameof(RoomSound)}.{room}",
        _ => throw new InvalidOperationException("The default sound identity names no sound."),
    };
}
